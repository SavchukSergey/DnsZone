namespace DnsZone.Records {
    public class CAAResourceRecord : ResourceRecord {
        
        public ushort Flag { get; set; }
        
        public string Tag { get; set; }
        
        public string Value { get; set; }
        
        public override ResourceRecordType Type => ResourceRecordType.CAA;
        
        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

    }
}
