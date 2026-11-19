using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using DnsZone.IO;
using DnsZone.Records;
using DnsZone.Tokens;

namespace DnsZone.Parser {
    public class DnsZoneParseContext {

        private const int MAX_INCLUDE_DEPTH = 32;

        private Token? _lastToken;

        public IDnsSource Source { get; }

        protected string? FileName { get; set; }

        protected int Depth { get; set; }


        public string? Origin { get; set; }

        /// <summary>
        /// Default TTL set by $TTL directive
        /// </summary>
        public TimeSpan? DefaultTtl { get; set; }

        /// <summary>
        /// Last explicitly specified TTL. Used when no $TTL directive is present (RFC 1035 section 5.1)
        /// </summary>
        public TimeSpan? LastTtl { get; set; }


        public string? PrevName { get; set; }

        public string? PrevClass { get; set; }


        public DnsZoneFile Zone { get; }

        public Queue<Token> Tokens { get; }

        public bool IsEof => Tokens.Count == 0;

        public DnsZoneParseContext(IEnumerable<Token> tokens, IDnsSource source) {
            Source = source;
            Zone = new DnsZoneFile();
            Tokens = new Queue<Token>(tokens);
        }

        public DnsZoneParseContext CreateChildContext(string fileName) {
            if (_lastToken == null) throw new InvalidOperationException("include token is unknown");
            return CreateChildContext(fileName, _lastToken.Value);
        }

        public DnsZoneParseContext CreateChildContext(string fileName, Token includeToken) {
            if (Depth >= MAX_INCLUDE_DEPTH) {
                throw new TokenException("$INCLUDE nesting is too deep", includeToken);
            }
            var newFile = Source.ResolveFile(fileName, FileName!);
            if (newFile == null) {
                throw new TokenException($"include file '{fileName}' is not found", includeToken);
            }

            var fileSource = new FileSource {
                FileName = newFile,
                Content = Source.LoadContent(newFile)
            };
            var tokens = Tokenizer.Read(fileSource).ToArray();
            return new DnsZoneParseContext(tokens, Source) {
                FileName = newFile,
                Depth = Depth + 1,
                Origin = Origin,
                DefaultTtl = DefaultTtl,
                LastTtl = LastTtl,
                PrevName = PrevName,
                PrevClass = PrevClass
            };
        }

        /// <summary>
        /// Dequeues next token. Throws <see cref="TokenException"/> if there are no more tokens.
        /// </summary>
        public Token Next(string expected) {
            if (Tokens.Count == 0) {
                throw CreateEofException(expected);
            }
            var token = Tokens.Dequeue();
            _lastToken = token;
            return token;
        }

        /// <summary>
        /// Dequeues next literal token. Throws <see cref="TokenException"/> if token has other type or there are no more tokens.
        /// </summary>
        public Token NextLiteral(string expected) {
            var token = Next(expected);
            if (token.Type != TokenType.Literal) throw new TokenException(expected, token);
            return token;
        }

        public bool TryPeek(out Token token) {
            return Tokens.TryPeek(out token);
        }

        private Exception CreateEofException(string expected) {
            var message = $"{expected}, but end of file reached";
            if (_lastToken.HasValue) {
                return new TokenException(message, _lastToken.Value);
            }
            return new FormatException(message);
        }

        public ushort ReadU16() {
            var token = NextLiteral("U16 value expected");
            if (!ushort.TryParse(token.StringValue, NumberStyles.None, CultureInfo.InvariantCulture, out var val)) {
                throw new TokenException($"U16 value expected, but '{token.StringValue}' found", token);
            }
            return val;
        }

        public byte ReadU8() {
            var token = NextLiteral("U8 value expected");
            if (!byte.TryParse(token.StringValue, NumberStyles.None, CultureInfo.InvariantCulture, out var val)) {
                throw new TokenException($"U8 value expected, but '{token.StringValue}' found", token);
            }
            return val;
        }

        public bool IsEndOfRecord => !TryPeek(out var token) || token.Type == TokenType.NewLine;

        /// <summary>
        /// Reads all literal tokens till the end of the record
        /// </summary>
        public List<Token> ReadRemainingLiterals() {
            var res = new List<Token>();
            while (!IsEndOfRecord) {
                res.Add(NextLiteral("literal expected"));
            }
            return res;
        }

        /// <summary>
        /// Reads Base64 or hex data till the end of the record. Data may be split by whitespace (RFC 4034 section 2.2).
        /// </summary>
        private string ReadEncodedData(string expected, out Token firstToken) {
            if (IsEndOfRecord) {
                if (TryPeek(out var token)) throw new TokenException(expected, token);
                throw CreateEofException(expected);
            }
            var tokens = ReadRemainingLiterals();
            firstToken = tokens[0];
            return string.Concat(tokens.Select(item => item.StringValue));
        }

        /// <summary>
        /// Reads Base64 data till the end of the record (RFC 4034 section 2.2). Data may be split by whitespace.
        /// </summary>
        public string ReadBase64(string expected) {
            var val = ReadEncodedData(expected, out var token);
            if (!DnsZoneUtils.IsBase64(val)) {
                throw new TokenException($"{expected}, but invalid Base64 '{val}' found", token);
            }
            return val;
        }

        /// <summary>
        /// Reads hex data till the end of the record (RFC 4034 section 5.3). Data may be split by whitespace.
        /// </summary>
        public string ReadHex(string expected) {
            var val = ReadEncodedData(expected, out var token);
            if (!DnsZoneUtils.TryParseHex(val, out _)) {
                throw new TokenException($"{expected}, but invalid hex '{val}' found", token);
            }
            return val;
        }

        /// <summary>
        /// Reads Base32hex value without padding (RFC 4648 section 7, RFC 5155 section 3.3)
        /// </summary>
        public string ReadBase32Hex(string expected) {
            var token = NextLiteral(expected);
            if (!DnsZoneUtils.IsBase32Hex(token.StringValue)) {
                throw new TokenException($"{expected}, but invalid Base32hex '{token.StringValue}' found", token);
            }
            return token.StringValue;
        }

        /// <summary>
        /// Reads type mnemonic (e.g. A, RRSIG or TYPE65534)
        /// </summary>
        public string ReadTypeMnemonic() {
            var token = NextLiteral("resource record type expected");
            if (!DnsZoneUtils.IsTypeMnemonic(token.StringValue)) {
                throw new TokenException($"resource record type expected, but '{token.StringValue}' found", token);
            }
            return token.StringValue.ToUpperInvariant();
        }

        /// <summary>
        /// Reads type bit maps till the end of the record (RFC 4034 section 4.2)
        /// </summary>
        public List<string> ReadTypeMnemonics() {
            var res = new List<string>();
            while (!IsEndOfRecord) {
                res.Add(ReadTypeMnemonic());
            }
            return res;
        }

        public DateTime ReadDnssecTime() {
            var token = NextLiteral("signature time expected");
            if (!DnsZoneUtils.TryParseDnssecTime(token.StringValue, out var val)) {
                throw new TokenException($"signature time expected, but '{token.StringValue}' found", token);
            }
            return val;
        }

        /// <summary>
        /// Reads NSEC3 salt: hex string or "-" for empty salt (RFC 5155 section 3.3)
        /// </summary>
        public string ReadSalt() {
            var token = NextLiteral("salt expected");
            var val = token.StringValue;
            if (val == "-") return "";
            if (!DnsZoneUtils.TryParseHex(val, out var bytes) || bytes.Length > 255) {
                throw new TokenException($"salt expected, but '{val}' found", token);
            }
            return val;
        }

        /// <summary>
        /// Reads RFC 3597 generic RDATA: \# length hex-data
        /// </summary>
        public byte[] ReadGenericData() {
            var marker = NextLiteral("generic rdata expected");
            if (marker.StringValue != @"\#") {
                throw new TokenException($@"generic rdata (\#) expected, but '{marker.StringValue}' found", marker);
            }
            var length = ReadU16();
            var hexTokens = ReadRemainingLiterals();
            var hex = string.Concat(hexTokens.Select(item => item.StringValue));
            if (!DnsZoneUtils.TryParseHex(hex, out var data)) {
                throw new TokenException("generic rdata must be hex encoded", hexTokens.Count > 0 ? hexTokens[0] : marker);
            }
            if (data.Length != length) {
                throw new TokenException($"generic rdata length mismatch: {length} expected, but {data.Length} found", marker);
            }
            return data;
        }

        public string ReadDomainName() {
            return NextLiteral("domain name expected").StringValue;
        }

        public string ReadAndResolveDomainName() {
            var token = NextLiteral("domain name expected");
            try {
                return ResolveDomainName(token.StringValue);
            } catch (ArgumentException exc) {
                throw new TokenException(exc.Message, token);
            }
        }

        public string ReadEmail() {
            return NextLiteral("email expected").StringValue;
        }

        public string ReadString() {
            return NextLiteral("string expected").StringValue;
        }

        /// <summary>
        /// Reads character-string (RFC 1035 section 5.1): either quoted or unquoted.
        /// </summary>
        public string ReadStringValue() {
            var token = Next("string expected");
            return token.Type switch {
                TokenType.QuotedString => token.StringValue,
                TokenType.Literal => DnsZoneUtils.UnescapeCharacterString(token.StringValue),
                _ => throw new TokenException("string expected", token)
            };
        }

        /// <summary>
        /// Reads all character-strings till the end of the record and concatenates them
        /// </summary>
        public IEnumerable<string> ReadStringValues() {
            while (TryPeek(out var token) && token.Type != TokenType.NewLine) {
                yield return ReadStringValue();
            }
        }

        public string ReadSerialNumber() {
            return NextLiteral("serial number expected").StringValue;
        }

        public IPAddress ReadIpAddress() {
            var token = NextLiteral("ip address expected");
            if (!IPAddress.TryParse(token.StringValue, out var address)) {
                throw new TokenException($"ip address expected, but '{token.StringValue}' found", token);
            }
            return address;
        }

        public TimeSpan ReadTimeSpan() {
            var token = NextLiteral("timespan expected");
            if (!DnsZoneUtils.TryParseTimeSpan(token.StringValue, out var val)) {
                throw new TokenException($"timespan expected, but '{token.StringValue}' found", token);
            }
            return val;
        }

        public ResourceRecordType ReadResourceRecordType() {
            return ReadResourceRecordType(out _);
        }

        /// <summary>
        /// Reads record type. Generic TYPEnnn mnemonic (RFC 3597) results in <see cref="ResourceRecordType.Unknown"/>
        /// </summary>
        public ResourceRecordType ReadResourceRecordType(out ushort unknownTypeCode) {
            var token = NextLiteral("resource record type expected");
            if (DnsZoneUtils.TryParseUnknownType(token.StringValue, out unknownTypeCode)) {
                return ResourceRecordType.Unknown;
            }
            try {
                return DnsZoneUtils.ParseResourceRecordType(token.StringValue);
            } catch (NotSupportedException exc) {
                throw new TokenException(exc.Message, token);
            }
        }

        public string ResolveDomainName(string? val) {
            if (string.IsNullOrEmpty(val)) {
                throw new ArgumentException("domain name expected");
            }
            if (val == "@") {
                if (string.IsNullOrWhiteSpace(Origin)) {
                    throw new ArgumentException("couldn't resolve @ domain");
                }
                return Origin;
            }
            if (val == ".") {
                return ".";
            }
            if (val.StartsWith('.') || val.Contains("..")) {
                throw new ArgumentException($"domain name '{val}' contains empty label");
            }
            if (DnsZoneUtils.IsAbsoluteDomainName(val)) {
                return val[..^1];
            }
            if (string.IsNullOrWhiteSpace(Origin)) {
                throw new ArgumentException("couldn't resolve relative domain name");
            }
            return Origin == "." ? val : val + "." + Origin;
        }

        public TimeSpan GetTimeSpan(TimeSpan? explicitValue) {
            if (explicitValue.HasValue) return GetTimeSpan(explicitValue, default);
            if (DefaultTtl.HasValue) return DefaultTtl.Value;
            if (LastTtl.HasValue) return LastTtl.Value;
            throw new FormatException("unknown ttl value");
        }

        public TimeSpan GetTimeSpan(TimeSpan? explicitValue, Token token) {
            if (explicitValue.HasValue) {
                LastTtl = explicitValue.Value;
                return explicitValue.Value;
            }
            if (DefaultTtl.HasValue) return DefaultTtl.Value;
            if (LastTtl.HasValue) return LastTtl.Value;
            throw new TokenException("unknown ttl value", token);
        }

        public bool TryParseTtl(out TimeSpan val) {
            val = TimeSpan.Zero;
            if (!TryPeek(out var token)) return false;
            if (token.Type != TokenType.Literal) return false;

            if (DnsZoneUtils.TryParseTimeSpan(token.StringValue, out val)) {
                Next("ttl expected");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks whether token looks like TTL value (starts with digit), even if it can't be parsed
        /// </summary>
        public static bool LooksLikeTtl(Token token) {
            return token.Type == TokenType.Literal && token.StringValue.Length > 0 && char.IsAsciiDigit(token.StringValue[0]);
        }

        public bool TryParseTtl(out TimeSpan? timestamp) {
            if (TryParseTtl(out TimeSpan val)) {
                timestamp = val;
                return true;
            }
            timestamp = null;
            return false;
        }

        protected void SkipWhiteAndComments() {
            while (TryPeek(out var token)) {
                switch (token.Type) {
                    case TokenType.Whitespace:
                    case TokenType.Comments:
                        Tokens.Dequeue();
                        break;
                    default:
                        return;
                }
            }
        }

        public bool TryParseClass(out string? @class) {
            @class = null;
            if (!TryPeek(out var token)) return false;
            if (token.Type != TokenType.Literal) return false;
            if (DnsZoneUtils.TryParseClass(token.StringValue, out @class)) {
                Next("class expected");
                return true;
            }
            return false;
        }

    }
}
