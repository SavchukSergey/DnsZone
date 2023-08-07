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
            context.WriteClass(record.Content);
            return record;
        }

        public ResourceRecord Visit(CNameResourceRecord record, DnsZoneFormatterContext context) {
            context.WriteDomainName(record.CanonicalName);
            return record;
        }

        public ResourceRecord Visit(DnameResourceRecord record, DnsZoneFormatterContext context) {
            context.WriteTag(record.Content);
            return record;
        }

        public ResourceRecord Visit(DsResourceRecord record, DnsZoneFormatterContext context) {
            context.WriteTag(record.KeyTag);
            context.WritePreference(record.Algorithm);
            context.WritePreference(record.HashType);
            context.WriteTag(record.Hash);
            return record;
        }

        public ResourceRecord Visit(HinfoResourceRecord record, DnsZoneFormatterContext context) {
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
            context.WritePreference(record.Preference);
            context.WriteAndCompressDomainName(record.Exchange);
            return record;
        }

        public ResourceRecord Visit(NaptrResourceRecord record, DnsZoneFormatterContext context) {
            context.WritePreference(record.Order);
            context.WritePreference(record.Preference);
            context.WriteString(record.Flags);
            context.WriteString(record.Services);
            context.WriteString(record.Regexp);
            context.WriteDomainName(record.Replacement);
            return record;
        }

        public ResourceRecord Visit(CAAResourceRecord record, DnsZoneFormatterContext context) {
            context.WritePreference(record.Flag);
            context.WriteTag(record.Tag);
            context.WriteString(record.Value);
            return record;
        }

        public ResourceRecord Visit(TLSAResourceRecord record, DnsZoneFormatterContext context) {
            context.WritePreference(record.CertificateUsage);
            context.WritePreference(record.Selector);
            context.WritePreference(record.MatchingType);
            context.WriteTag(record.CertificateAssociationData);
            return record;
        }

        public ResourceRecord Visit(SSHFPResourceRecord record, DnsZoneFormatterContext context) {
            context.WritePreference(record.AlgorithmNumber);
            context.WritePreference(record.FingerprintType);
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
            context.WritePreference(record.Priority);
            context.WritePreference(record.Weight);
            context.WritePreference(record.Port);
            context.WriteAndCompressDomainName(record.Target);
            return record;
        }

        public ResourceRecord Visit(TxtResourceRecord record, DnsZoneFormatterContext context) {
            context.WriteString(record.Content);
            return record;
        }

    }
}
