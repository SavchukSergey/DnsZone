using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        private static readonly ResourceRecordReader _reader = new ResourceRecordReader();

        public IList<ResourceRecord> Records { get; } = new List<ResourceRecord>();

        public DnsZoneFile Filter(string origin) {
            var res = new DnsZoneFile();
            origin = origin.ToLowerInvariant();
            var subdomain = ("." + origin);
            foreach (var record in Records) {
                if (record.Name.ToLowerInvariant() == origin || record.Name.EndsWith(subdomain)) {
                    res.Records.Add(record);
                }
            }
            return res;
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

        public string ToString(string origin) {
            var sb = new StringBuilder();
            var context = new DnsZoneFormatterContext(this, sb) {
                Origin = origin
            };
            if (!string.IsNullOrWhiteSpace(origin)) {
                context.WriteOrigin(origin);
            }
            var writer = new ResourceRecordWriter();
            foreach (var recordGroup in Records.GroupBy(item => item.Type)) {
                context.Sb.AppendLine($";{recordGroup.Key} records");
                foreach (var record in recordGroup) {
                    context.WriteAndCompressDomainName(record.Name);
                    context.WriteClass(record.Class);
                    context.WriteTimeSpan(record.Ttl);
                    context.WriteResourceRecordType(record.Type);
                    record.AcceptVistor(writer, context);
                    context.Sb.AppendLine();
                }
                context.Sb.AppendLine();
            }
            return sb.ToString();
        }

        /// <summary>
        /// Parses DNS zone file
        /// </summary>
        /// <param name="content">DNS zone file content</param>
        /// <param name="origin">Explicit origin if needed</param>
        /// <returns></returns>
        public static DnsZoneFile Parse(string content, string origin = null) {
            return Parse(new StringDnsSource(content), origin);
        }

        /// <summary>
        /// Parses DNS zone file
        /// </summary>
        /// <param name="source">DNS zone file source</param>
        /// <param name="origin">Explicit origin if needed</param>
        /// <returns></returns>
        public static DnsZoneFile Parse(IDnsSource source, string origin = null) {
            var tokenizer = new Tokenizer();
            var fileSource = new FileSource {
                Content = source.LoadContent(null)
            };
            var tokens = tokenizer.Read(fileSource).ToArray();
            var context = new DnsZoneParseContext(tokens, source) {
                Origin = origin
            };
            Process(context);
            return context.Zone;
        }

        private static void Process(DnsZoneParseContext context) {
            while (!context.IsEof) {
                var token = context.Tokens.Peek();
                switch (token.Type) {
                    case TokenType.NewLine:
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
                        throw new NotSupportedException($"not supported type {token.Type}");
                }
            }
        }

        private static void ProcessControlDirective(DnsZoneParseContext context) {
            var token = context.Tokens.Dequeue();
            var directive = token.StringValue;
            switch (directive.ToUpperInvariant()) {
                case "ORIGIN":
                    context.Origin = context.ReadAndResolveDomainName();
                    break;
                case "INCLUDE":
                    var firstToken = context.Tokens.Dequeue();
                    var secondToken = context.Tokens.Dequeue();
                    if (secondToken.Type == TokenType.NewLine) {
                        ProcessIncludeDirective(context, context.Origin, firstToken.StringValue);
                    } else {
                        context.Tokens.Dequeue(); //end of line
                        ProcessIncludeDirective(context, secondToken.StringValue, firstToken.StringValue);
                    }
                    break;
                case "TTL":
                    context.DefaultTtl = context.ReadTimeSpan();
                    break;
                default:
                    throw new NotSupportedException($"Unknown control directive '{directive}'");
            }
        }

        private static void ProcessIncludeDirective(DnsZoneParseContext context, string origin, string fileName) {
            var childContext = context.CreateChildContext(fileName);
            childContext.Origin = context.ResolveDomainName(origin);
            Process(childContext);
            context.Zone.Add(childContext.Zone);
        }

        private static void ParseResourceRecord(DnsZoneParseContext context) {
            string @class;
            TimeSpan? ttl;

            var nameToken = context.Tokens.Dequeue();
            var name = nameToken.Type != TokenType.Whitespace ? nameToken.StringValue : "";

            if (context.TryParseClass(out @class)) {
                context.TryParseTtl(out ttl);
            } else if (context.TryParseTtl(out ttl)) {
                context.TryParseClass(out @class);
            }

            var type = context.ReadResourceRecordType();

            string domainName;
            try {
                domainName = context.ResolveDomainName(!string.IsNullOrWhiteSpace(name) ? name : context.PrevName);
            } catch (ArgumentException exc) {
                throw new TokenException(exc.Message, nameToken);
            }
            var record = DnsZoneUtils.CreateRecord(type);
            record.Name = domainName;
            record.Class = !string.IsNullOrWhiteSpace(@class) ? @class : context.PrevClass;
            record.Ttl = context.GetTimeSpan(ttl);

            record.AcceptVistor(_reader, context);

            context.Zone.Records.Add(record);

            context.PrevName = name;

            if (!string.IsNullOrWhiteSpace(@class)) {
                context.PrevClass = @class;
            }

        }

        public static async Task<DnsZoneFile> LoadFromUriAsync(string uri, string explicitOrigin = null) {
            using (var client = new HttpClient()) {
                var content = await client.GetStringAsync(uri);
                return Parse(content, explicitOrigin);
            }
        }

        public static async Task<DnsZoneFile> LoadFromFileAsync(string path, string explicitOrigin = null) {
            using (var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                using (var reader = new StreamReader(file)) {
                    var content = await reader.ReadToEndAsync();
                    return Parse(content, explicitOrigin);
                }
            }
        }

    }
}
