namespace DnsZone.Records {
    public class AliasResourceRecord : ResourceRecord {

        public string Target { get; set; }

        public override ResourceRecordType Type => ResourceRecordType.ALIAS;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

    }
}
