namespace DnsZone.Records {
    public class HInfoResourceRecord : ResourceRecord {

        public string Cpu { get; set; }

        public string Os { get; set; }

        public override ResourceRecordType Type => ResourceRecordType.HINFO;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }
    }
}
