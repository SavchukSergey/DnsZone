using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DnsZone.Formatter;
using DnsZone.IO;
using DnsZone.Parser;
using DnsZone.Records;
using DnsZone.Tokens;
using System.Net.Http;

namespace DnsZone {
    public class DnsZoneFile {

        private static readonly ResourceRecordReader _reader = new();

        public IList<ResourceRecord> Records { get; } = [];

        public DnsZoneFile Filter(string origin) {
            var res = new DnsZoneFile();
            origin = NormalizeOrigin(origin)!;
            var subdomain = "." + origin;
            foreach (var record in Records) {
                var name = NormalizeOrigin(record.Name);
                if (name == null) continue;
                if (string.Equals(name, origin, StringComparison.OrdinalIgnoreCase) || name.EndsWith(subdomain, StringComparison.OrdinalIgnoreCase)) {
                    res.Records.Add(record);
                }
            }
            return res;
        }

        /// <summary>
        /// Removes single trailing dot from the domain name (except root domain)
        /// </summary>
        internal static string? NormalizeOrigin(string? origin) {
            if (string.IsNullOrWhiteSpace(origin)) return null;
            if (origin == ".") return origin;
            return DnsZoneUtils.IsAbsoluteDomainName(origin) ? origin[..^1] : origin;
        }

        public DnsZoneFile Add(DnsZoneFile other) {
            foreach (var record in other.Records) {
                Records.Add(record);
            }
            return this;
        }

        public T Single<T>(string origin) where T : ResourceRecord {
            return Records.OfType<T>().Single(item => string.Equals(item.Name, origin, StringComparison.OrdinalIgnoreCase));
        }

        public override string ToString() {
            return ToString(null);
        }

        public string ToString(string? origin = null, TimeSpan? defaultTtl = null) {
            origin = NormalizeOrigin(origin);
            var sb = new StringBuilder();
            var context = new DnsZoneFormatterContext(this, sb) {
                Origin = origin
            };
            if (!string.IsNullOrWhiteSpace(origin)) {
                context.WriteOrigin(origin);
            }
            if (defaultTtl.HasValue) {
                context.WriteTtl(defaultTtl.Value);
            }
            var writer = new ResourceRecordWriter();
            foreach (var recordGroup in Records.GroupBy(DnsZoneUtils.FormatResourceRecordType)) {
                context.AppendLine($";{recordGroup.Key} records");
                foreach (var record in recordGroup) {
                    context.WriteAndCompressDomainName(record.Name);
                    context.WriteClass(record.Class);
                    context.WriteTimeSpanTab(record.Ttl);
                    context.WriteResourceRecordType(recordGroup.Key);
                    record.AcceptVistor(writer, context);
                    context.AppendLine();
                }
                context.AppendLine();
            }
            return sb.ToString();
        }

        /// <summary>
        /// Parses DNS zone file
        /// </summary>
        /// <param name="content">DNS zone file content</param>
        /// <param name="origin">Explicit origin if needed</param>
        /// <returns></returns>
        public static DnsZoneFile Parse(string content, string? origin = null) {
            return Parse(new StringDnsSource(content), origin);
        }

        /// <summary>
        /// Parses DNS zone file
        /// </summary>
        /// <param name="source">DNS zone file source</param>
        /// <param name="origin">Explicit origin if needed</param>
        /// <returns></returns>
        public static DnsZoneFile Parse(IDnsSource source, string? origin = null) {
            var fileSource = new FileSource {
                Content = source.LoadContent(null!)
            };
            var tokens = Tokenizer.Read(fileSource).ToArray();
            var context = new DnsZoneParseContext(tokens, source) {
                Origin = NormalizeOrigin(origin)
            };
            Process(context);
            return context.Zone;
        }

        private static void Process(DnsZoneParseContext context) {
            while (context.TryPeek(out var token)) {
                switch (token.Type) {
                    case TokenType.NewLine:
                    case TokenType.Comments:
                        context.Tokens.Dequeue();
                        break;
                    case TokenType.Control:
                        ProcessControlDirective(context);
                        break;
                    case TokenType.Whitespace:
                    case TokenType.Literal:
                        ParseResourceRecord(context);
                        break;
                    default:
                        throw new TokenException($"unexpected token {token.Type}", token);
                }
            }
        }

        private static bool IsEndOfLine(DnsZoneParseContext context) {
            return !context.TryPeek(out var token) || token.Type == TokenType.NewLine;
        }

        private static void ProcessControlDirective(DnsZoneParseContext context) {
            var token = context.Next("control directive expected");
            var directive = token.StringValue;
            switch (directive.ToUpperInvariant()) {
                case "ORIGIN":
                    context.Origin = context.ReadAndResolveDomainName();
                    break;
                case "INCLUDE":
                    var fileToken = context.Next("file name expected");
                    if (fileToken.Type != TokenType.Literal && fileToken.Type != TokenType.QuotedString) {
                        throw new TokenException("file name expected", fileToken);
                    }
                    string? origin = null;
                    if (!IsEndOfLine(context)) {
                        origin = context.ReadAndResolveDomainName();
                    }
                    ProcessIncludeDirective(context, origin, fileToken);
                    break;
                case "TTL":
                    context.DefaultTtl = context.ReadTimeSpan();
                    break;
                default:
                    throw new TokenException($"unknown control directive '{directive}'", token);
            }
            if (!IsEndOfLine(context)) {
                var extra = context.Tokens.Peek();
                throw new TokenException($"unexpected token '{extra.StringValue}' after {directive} directive", extra);
            }
        }

        private static void ProcessIncludeDirective(DnsZoneParseContext context, string? origin, Token fileToken) {
            var childContext = context.CreateChildContext(fileToken.StringValue, fileToken);
            if (origin != null) {
                childContext.Origin = origin;
            }
            Process(childContext);
            context.Zone.Add(childContext.Zone);
        }

        private static void ParseResourceRecord(DnsZoneParseContext context) {
            string? @class = null;
            TimeSpan? ttl = null;

            string? name = null;
            var nameToken = context.Next("resource record expected");
            if (nameToken.Type == TokenType.Literal) {
                name = nameToken.StringValue;
            } else if (nameToken.Type == TokenType.Whitespace) {
                if (IsEndOfLine(context)) {
                    return;
                }
            } else {
                return;
            }

            while (@class == null || ttl == null) {
                if (!context.TryPeek(out var token)) {
                    throw new TokenException("missing record type", nameToken);
                }
                if (token.Type == TokenType.Literal) {
                    if (@class == null) {
                        if (context.TryParseClass(out @class)) continue;
                    }
                    if (@ttl == null) {
                        if (context.TryParseTtl(out ttl)) continue;
                        if (DnsZoneParseContext.LooksLikeTtl(token)) {
                            throw new TokenException($"invalid ttl value '{token.StringValue}' (max {int.MaxValue} seconds)", token);
                        }
                    }
                    break;
                } else if (token.Type == TokenType.Comments || token.Type == TokenType.NewLine) {
                    throw new TokenException("missing record type", token);
                } else {
                    throw new TokenException("unexpected token", token);
                }
            }

            var type = context.ReadResourceRecordType(out var unknownTypeCode);

            string domainName;
            if (name != null) {
                try {
                    domainName = context.ResolveDomainName(name);
                } catch (ArgumentException exc) {
                    throw new TokenException(exc.Message, nameToken);
                }
            } else {
                domainName = context.PrevName ?? throw new TokenException("owner name is omitted, but there is no previous owner", nameToken);
            }
            ResourceRecord record;
            try {
                record = type == ResourceRecordType.Unknown
                    ? new UnknownResourceRecord { TypeCode = unknownTypeCode }
                    : DnsZoneUtils.CreateRecord(type);
            } catch (NotSupportedException exc) {
                throw new TokenException(exc.Message, nameToken);
            }
            record.Name = domainName;
            record.Class = !string.IsNullOrWhiteSpace(@class) ? @class : context.PrevClass!;
            record.Ttl = context.GetTimeSpan(ttl, nameToken);

            record.AcceptVistor(_reader, context);

            context.Zone.Records.Add(record);

            context.PrevName = domainName;

            if (!string.IsNullOrWhiteSpace(@class)) {
                context.PrevClass = @class;
            }

        }

        public static async Task<DnsZoneFile> LoadFromUriAsync(string uri, string? explicitOrigin = null) {
            using var client = new HttpClient();
            var content = await client.GetStringAsync(uri);
            return Parse(content, explicitOrigin);
        }

        public static async Task<DnsZoneFile> LoadFromFileAsync(string path, string? explicitOrigin = null) {
            using var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(file);
            var content = await reader.ReadToEndAsync();
            return Parse(new FileDnsSource(path, content), explicitOrigin);
        }

    }
}
