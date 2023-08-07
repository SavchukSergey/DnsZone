namespace DnsZone.Records {
    public class DnameResourceRecord : ResourceRecord {

        public string Content { get; set; }

        public override ResourceRecordType Type => ResourceRecordType.DNAME;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

        public override string ToString() {
            return Content;
        }
    }
}
