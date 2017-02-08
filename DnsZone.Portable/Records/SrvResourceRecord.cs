namespace DnsZone.Records {
    public class SrvResourceRecord : ResourceRecord {

        public ushort Priority { get; set; }

        public ushort Weight { get; set; }

        public ushort Port { get; set; }

        public string Target { get; set; }

        public string Service { get; set; }

        public string Protocol { get; set; }

        public string Host { get; set; }

        public override string Name {
            get { return $"_{Service}._{Protocol}.{Host}"; }
            set {
                if (value != null) {
                    if (value.StartsWith("_")) {
                        var dotIndex = value.IndexOf('.');
                        Service = value.Substring(1, dotIndex - 1);
                        value = value.Substring(dotIndex + 1);
                    }
                    if (value.StartsWith("_")) {
                        var dotIndex = value.IndexOf('.');
                        Protocol = value.Substring(1, dotIndex - 1);
                        value = value.Substring(dotIndex + 1);
                    }
                    Host = value;
                } else {
                    Service = null;
                    Protocol = null;
                    Host = null;
                }
            }
        }

        public override ResourceRecordType Type => ResourceRecordType.SRV;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

    }
}
