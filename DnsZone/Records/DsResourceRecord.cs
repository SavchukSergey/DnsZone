namespace DnsZone.Records {
    public class DsResourceRecord : ResourceRecord {
        
        public string KeyTag { get; set; }
        
        public ushort Algorithm { get; set; }
        
        public ushort HashType { get; set; }

        public string Hash { get; set; }

        public override ResourceRecordType Type => ResourceRecordType.DS;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

        public override string ToString() {
            return $"{KeyTag} {Algorithm} {HashType} {Hash}";
        }
    }
}
