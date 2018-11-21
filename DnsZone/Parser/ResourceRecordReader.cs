using System;
using System.Text;
using DnsZone.Records;
using DnsZone.Tokens;

namespace DnsZone.Parser {
    public class ResourceRecordReader : IResourceRecordVisitor<DnsZoneParseContext, ResourceRecord> {

        public ResourceRecord Visit(AResourceRecord record, DnsZoneParseContext context) {
            record.Address = context.ReadIpAddress();
            return record;
        }

        public ResourceRecord Visit(AaaaResourceRecord record, DnsZoneParseContext context) {
            record.Address = context.ReadIpAddress();
            return record;
        }

        public ResourceRecord Visit(CNameResourceRecord record, DnsZoneParseContext context) {
            record.CanonicalName = context.ReadAndResolveDomainName();
            return record;
        }

        public ResourceRecord Visit(MxResourceRecord record, DnsZoneParseContext context) {
            record.Preference = context.ReadPreference();
            record.Exchange = context.ReadAndResolveDomainName();
            return record;
        }

        public ResourceRecord Visit(NsResourceRecord record, DnsZoneParseContext context) {
            record.NameServer = context.ReadAndResolveDomainName();
            return record;
        }

        public ResourceRecord Visit(PtrResourceRecord record, DnsZoneParseContext context) {
            record.HostName = context.ReadAndResolveDomainName();
            return record;
        }

        public ResourceRecord Visit(SoaResourceRecord record, DnsZoneParseContext context) {
            record.NameServer = context.ReadAndResolveDomainName();
            record.ResponsibleEmail = context.ReadEmail();
            record.SerialNumber = context.ReadSerialNumber();
            record.Refresh = context.ReadTimeSpan();
            record.Retry = context.ReadTimeSpan();
            record.Expiry = context.ReadTimeSpan();
            record.Minimum = context.ReadTimeSpan();
            return record;
        }

        public ResourceRecord Visit(SrvResourceRecord record, DnsZoneParseContext context) {
            record.Priority = context.ReadPreference();
            record.Weight = context.ReadPreference();
            record.Port = context.ReadPreference();
            record.Target = context.ReadAndResolveDomainName();
            return record;
        }

        public ResourceRecord Visit(TxtResourceRecord record, DnsZoneParseContext context) {
            var sb = new StringBuilder();
            while (!context.IsEof) {
                var token = context.Tokens.Peek();
                if (token.Type == TokenType.NewLine) break;
                if (token.Type == TokenType.QuotedString || token.Type == TokenType.Literal) {
                    sb.Append(token.StringValue);
                    context.Tokens.Dequeue();
                } else {
                    throw new NotSupportedException($"unexpected token {token.Type}");
                }
            }
            record.Content = sb.ToString();
            return record;
        }

    }
}
