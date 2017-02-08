namespace DnsZone.Records {
    public class MxResourceRecord : ResourceRecord {

        public ushort Preference { get; set; }

        public string Exchange { get; set; }

        public override ResourceRecordType Type => ResourceRecordType.MX;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

    }
}
