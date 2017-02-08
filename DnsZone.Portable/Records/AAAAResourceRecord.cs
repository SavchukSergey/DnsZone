using System.Net;

namespace DnsZone.Records {
    public class AaaaResourceRecord : ResourceRecord {

        public IPAddress Address { get; set; }

        public override ResourceRecordType Type => ResourceRecordType.AAAA;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

    }
}
