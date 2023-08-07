namespace DnsZone.Records {
    public class TLSAResourceRecord : ResourceRecord {

        public ushort? Port { get; set; }
        public string Protocol { get; set; }
        public string Host { get; set; }

        public ushort CertificateUsage { get; set; }

        public ushort Selector { get; set; }

        public ushort MatchingType { get; set; }

        public string CertificateAssociationData { get; set; }

        public override string Name {
            get { return $"_{Port}._{Protocol}.{Host}"; }
            set {
                if (value != null) {
                    if (value.StartsWith("_")) {
                        var dotIndex = value.IndexOf('.');
                        Port = ushort.Parse(value.Substring(1, dotIndex - 1));
                        value = value.Substring(dotIndex + 1);
                    }
                    if (value.StartsWith("_")) {
                        var dotIndex = value.IndexOf('.');
                        Protocol = value.Substring(1, dotIndex - 1);
                        value = value.Substring(dotIndex + 1);
                    }
                    Host = value;
                } else {
                    Port = null;
                    Protocol = null;
                    Host = null;
                }
            }
        }

        public override ResourceRecordType Type => ResourceRecordType.TLSA;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

        public override string ToString() {
            return $"{CertificateUsage} {Selector} {MatchingType} {CertificateAssociationData}";
        }
    }
}
