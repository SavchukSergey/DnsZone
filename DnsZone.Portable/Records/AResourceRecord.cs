using System.Net;

namespace DnsZone.Records {
    public class AResourceRecord : ResourceRecord {

        public IPAddress Address { get; set; }

        public override ResourceRecordType Type => ResourceRecordType.A;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

    }
}
