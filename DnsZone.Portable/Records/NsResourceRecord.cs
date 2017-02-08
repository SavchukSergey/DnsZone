namespace DnsZone.Records {
    public class NsResourceRecord : ResourceRecord {

        public string NameServer { get; set; }

        public override ResourceRecordType Type => ResourceRecordType.NS;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

    }
}
