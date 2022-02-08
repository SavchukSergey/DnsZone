namespace DnsZone.Records {
    public class SSHFPResourceRecord : ResourceRecord {
        
        public ushort AlgorithmNumber { get; set; }
        
        public ushort FingerprintType { get; set; }
        
        public string Fingerprint { get; set; }
        
        public override ResourceRecordType Type => ResourceRecordType.SSHFP;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

        public override string ToString() {
            return $"{AlgorithmNumber} {FingerprintType} {Fingerprint}";
        }
    }
}
