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
            get { return ServiceName.Format(Port?.ToString(System.Globalization.CultureInfo.InvariantCulture), Protocol, Host); }
            set {
                Port = null;
                Protocol = null;
                Host = value;
                if (value == null) return;
                var rest = value;
                if (ServiceName.TryTakeLabel(ref rest, out var label)) {
                    if (ushort.TryParse(label, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var port)) {
                        Port = port;
                        value = rest;
                        if (ServiceName.TryTakeLabel(ref rest, out var protocol)) {
                            Protocol = protocol;
                            value = rest;
                        }
                    } else {
                        Protocol = label;
                        value = rest;
                    }
                }
                Host = value;
            }
        }

        public override ResourceRecordType Type => ResourceRecordType.TLSA;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

    }
}
