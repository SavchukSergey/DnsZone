namespace DnsZone.Records {
    /// <summary>
    /// https://www.rfc-editor.org/rfc/rfc6672
    /// </summary>
    public class DNameResourceRecord : ResourceRecord {

        public string Target { get; set; }

        public override ResourceRecordType Type => ResourceRecordType.DNAME;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }
    }
}
