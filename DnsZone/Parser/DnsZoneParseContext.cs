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
            var token = NextLiteral("resource record type expected");
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
