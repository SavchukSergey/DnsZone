namespace DnsZone.Records {
    public class TxtResourceRecord : ResourceRecord {

        public string Content { get; set; }

        public override ResourceRecordType Type => ResourceRecordType.TXT;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

    }
}
