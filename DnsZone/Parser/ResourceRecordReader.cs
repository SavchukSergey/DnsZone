using DnsZone.Records;

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
            record.Hash = context.ReadEncodedData("digest expected");
            return record;
        }

        public ResourceRecord Visit(CdsResourceRecord record, DnsZoneParseContext context) {
            record.KeyTag = context.ReadSerialNumber();
            record.Algorithm = context.ReadU16();
            record.HashType = context.ReadU16();
            record.Hash = context.ReadEncodedData("digest expected");
            return record;
        }

        public ResourceRecord Visit(DnsKeyResourceRecord record, DnsZoneParseContext context) {
            record.Flags = context.ReadU16();
            record.Protocol = context.ReadU8();
            record.Algorithm = context.ReadU8();
            record.PublicKey = context.ReadEncodedData("public key expected");
            return record;
        }

        public ResourceRecord Visit(CDnsKeyResourceRecord record, DnsZoneParseContext context) {
            record.Flags = context.ReadU16();
            record.Protocol = context.ReadU8();
            record.Algorithm = context.ReadU8();
            record.PublicKey = context.ReadEncodedData("public key expected");
            return record;
        }

        public ResourceRecord Visit(RrsigResourceRecord record, DnsZoneParseContext context) {
            record.TypeCovered = context.ReadTypeMnemonic();
            record.Algorithm = context.ReadU8();
            record.Labels = context.ReadU8();
            record.OriginalTtl = context.ReadTimeSpan();
            record.SignatureExpiration = context.ReadDnssecTime();
            record.SignatureInception = context.ReadDnssecTime();
            record.KeyTag = context.ReadU16();
            record.SignerName = context.ReadAndResolveDomainName();
            record.Signature = context.ReadEncodedData("signature expected");
            return record;
        }

        public ResourceRecord Visit(NsecResourceRecord record, DnsZoneParseContext context) {
            record.NextDomainName = context.ReadAndResolveDomainName();
            record.Types = context.ReadTypeMnemonics();
            return record;
        }

        public ResourceRecord Visit(Nsec3ResourceRecord record, DnsZoneParseContext context) {
            record.HashAlgorithm = context.ReadU8();
            record.Flags = context.ReadU8();
            record.Iterations = context.ReadU16();
            record.Salt = context.ReadSalt();
            record.NextHashedOwnerName = context.ReadString();
            record.Types = context.ReadTypeMnemonics();
            return record;
        }

        public ResourceRecord Visit(Nsec3ParamResourceRecord record, DnsZoneParseContext context) {
            record.HashAlgorithm = context.ReadU8();
            record.Flags = context.ReadU8();
            record.Iterations = context.ReadU16();
            record.Salt = context.ReadSalt();
            return record;
        }

        public ResourceRecord Visit(UnknownResourceRecord record, DnsZoneParseContext context) {
            record.Data = context.ReadGenericData();
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
            record.Content = string.Concat(context.ReadStringValues());
            return record;
        }

        public ResourceRecord Visit(CAAResourceRecord record, DnsZoneParseContext context) {
            record.Flag = context.ReadU16();
            record.Tag = context.ReadStringValue();
            record.Value = string.Concat(context.ReadStringValues());
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
