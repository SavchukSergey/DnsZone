namespace DnsZone.Records {
    public class CNameResourceRecord : ResourceRecord {

        public string CanonicalName { get; set; }

        public override ResourceRecordType Type => ResourceRecordType.CNAME;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

    }
}
