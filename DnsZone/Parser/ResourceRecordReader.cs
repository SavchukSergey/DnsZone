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

        public ResourceRecord Visit(AliasResourceRecord record, DnsZoneParseContext context) {
            record.Target = context.ReadDomainName();
            return record;
        }

        public ResourceRecord Visit(CNameResourceRecord record, DnsZoneParseContext context) {
            record.CanonicalName = context.ReadAndResolveDomainName();
            return record;
        }

        public ResourceRecord Visit(DNameResourceRecord record, DnsZoneParseContext context) {
            record.Target = context.ReadDomainName();
            return record;
        }

        public ResourceRecord Visit(DsResourceRecord record, DnsZoneParseContext context) {
            record.KeyTag = context.ReadSerialNumber();
            record.Algorithm = context.ReadU16();
            record.HashType = context.ReadU16();
            record.Hash = context.ReadSerialNumber();
            return record;
        }

        public ResourceRecord Visit(HInfoResourceRecord record, DnsZoneParseContext context) {
            record.Cpu = context.ReadStringValue();
            record.Os = context.ReadStringValue();
            return record;
        }

        public ResourceRecord Visit(LuaResourceRecord record, DnsZoneParseContext context) {
            record.TargetType = context.ReadString();
            record.Script = context.ReadStringValue();
            return record;
        }

        public ResourceRecord Visit(MxResourceRecord record, DnsZoneParseContext context) {
            record.Preference = context.ReadU16();
            record.Exchange = context.ReadAndResolveDomainName();
            return record;
        }

        public ResourceRecord Visit(NaptrResourceRecord record, DnsZoneParseContext context) {
            record.Order = context.ReadU16();
            record.Preference = context.ReadU16();
            record.Flags = context.ReadStringValue();
            record.Services = context.ReadStringValue();
            record.Regexp = context.ReadStringValue();
            record.Replacement = context.ReadAndResolveDomainName();

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
            record.Priority = context.ReadU16();
            record.Weight = context.ReadU16();
            record.Port = context.ReadU16();
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

        public ResourceRecord Visit(CAAResourceRecord record, DnsZoneParseContext context) {
            record.Flag = context.ReadU16();
            record.Tag = context.Tokens.Dequeue().StringValue;
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
            record.Value = sb.ToString();

            return record;
        }

        public ResourceRecord Visit(TLSAResourceRecord record, DnsZoneParseContext context) {
            record.CertificateUsage = context.ReadU16();
            record.Selector = context.ReadU16();
            record.MatchingType = context.ReadU16();
            record.CertificateAssociationData = context.ReadString();

            return record;
        }

        public ResourceRecord Visit(SSHFPResourceRecord record, DnsZoneParseContext context) {
            record.AlgorithmNumber = context.ReadU16();
            record.FingerprintType = context.ReadU16();
            record.Fingerprint = context.ReadString();

            return record;
        }
    }
}
