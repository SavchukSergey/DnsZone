namespace DnsZone.Records {
    public class PtrResourceRecord : ResourceRecord {

        public string HostName { get; set; }

        public override ResourceRecordType Type => ResourceRecordType.PTR;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

    }
}
