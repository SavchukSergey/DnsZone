using DnsZone.Records;

namespace DnsZone.Formatter {
    public class ResourceRecordWriter : IResourceRecordVisitor<DnsZoneFormatterContext, ResourceRecord> {

        public ResourceRecord Visit(AResourceRecord record, DnsZoneFormatterContext context) {
            context.WriteIpAddress(record.Address);
            return record;
        }

        public ResourceRecord Visit(AaaaResourceRecord record, DnsZoneFormatterContext context) {
            context.WriteIpAddress(record.Address);
            return record;
        }

        public ResourceRecord Visit(AliasResourceRecord record, DnsZoneFormatterContext context) {
            context.WriteDomainName(record.Target);
            return record;
        }

        public ResourceRecord Visit(CNameResourceRecord record, DnsZoneFormatterContext context) {
            context.WriteAndCompressDomainName(record.CanonicalName);
            return record;
        }

        public ResourceRecord Visit(DNameResourceRecord record, DnsZoneFormatterContext context) {
            context.WriteDomainName(record.Target);
            return record;
        }

        public ResourceRecord Visit(DsResourceRecord record, DnsZoneFormatterContext context) {
            context.WriteTag(record.KeyTag);
            context.WriteU16(record.Algorithm);
            context.WriteU16(record.HashType);
            context.WriteTag(record.Hash);
            return record;
        }

        public ResourceRecord Visit(HInfoResourceRecord record, DnsZoneFormatterContext context) {
            context.WriteString(record.Cpu);
            context.WriteString(record.Os);
            return record;
        }

        public ResourceRecord Visit(LuaResourceRecord record, DnsZoneFormatterContext context) {
            context.WriteTag(record.TargetType);
            context.WriteString(record.Script);
            return record;
        }

        public ResourceRecord Visit(MxResourceRecord record, DnsZoneFormatterContext context) {
            context.WriteU16(record.Preference);
            context.WriteAndCompressDomainName(record.Exchange);
            return record;
        }

        public ResourceRecord Visit(NaptrResourceRecord record, DnsZoneFormatterContext context) {
            context.WriteU16(record.Order);
            context.WriteU16(record.Preference);
            context.WriteString(record.Flags);
            context.WriteString(record.Services);
            context.WriteString(record.Regexp);
            context.WriteDomainName(record.Replacement);
            return record;
        }

        public ResourceRecord Visit(CAAResourceRecord record, DnsZoneFormatterContext context) {
            context.WriteU16(record.Flag);
            context.WriteTag(record.Tag);
            context.WriteString(record.Value);
            return record;
        }

        public ResourceRecord Visit(TLSAResourceRecord record, DnsZoneFormatterContext context) {
            context.WriteU16(record.CertificateUsage);
            context.WriteU16(record.Selector);
            context.WriteU16(record.MatchingType);
            context.WriteTag(record.CertificateAssociationData);
            return record;
        }

        public ResourceRecord Visit(SSHFPResourceRecord record, DnsZoneFormatterContext context) {
            context.WriteU16(record.AlgorithmNumber);
            context.WriteU16(record.FingerprintType);
            context.WriteTag(record.Fingerprint);
            return record;
        }

        public ResourceRecord Visit(NsResourceRecord record, DnsZoneFormatterContext context) {
            context.WriteAndCompressDomainName(record.NameServer);
            return record;
        }

        public ResourceRecord Visit(PtrResourceRecord record, DnsZoneFormatterContext context) {
            context.WriteAndCompressDomainName(record.HostName);
            return record;
        }

        public ResourceRecord Visit(SoaResourceRecord record, DnsZoneFormatterContext context) {
            context.WriteAndCompressDomainName(record.NameServer);
            context.WriteEmail(record.ResponsibleEmail);
            context.WriteSerialNumber(record.SerialNumber);
            context.WriteTimeSpan(record.Refresh);
            context.WriteTimeSpan(record.Retry);
            context.WriteTimeSpan(record.Expiry);
            context.WriteTimeSpan(record.Minimum);
            return record;
        }

        public ResourceRecord Visit(SrvResourceRecord record, DnsZoneFormatterContext context) {
            context.WriteU16(record.Priority);
            context.WriteU16(record.Weight);
            context.WriteU16(record.Port);
            context.WriteAndCompressDomainName(record.Target);
            return record;
        }

        public ResourceRecord Visit(TxtResourceRecord record, DnsZoneFormatterContext context) {
            context.WriteString(record.Content);
            return record;
        }

    }
}
