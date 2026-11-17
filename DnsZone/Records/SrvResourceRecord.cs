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
            get { return ServiceName.Format(Service, Protocol, Host); }
            set {
                Service = null;
                Protocol = null;
                Host = value;
                if (value == null) return;
                if (ServiceName.TryTakeLabel(ref value, out var service)) {
                    Service = service;
                    if (ServiceName.TryTakeLabel(ref value, out var protocol)) {
                        Protocol = protocol;
                    }
                    Host = value;
                }
            }
        }

        public override ResourceRecordType Type => ResourceRecordType.SRV;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

    }
}
