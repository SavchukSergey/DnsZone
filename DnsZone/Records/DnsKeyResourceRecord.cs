namespace DnsZone.Records {
    /// <summary>
    /// DNSKEY record (RFC 4034 section 2)
    /// </summary>
    public class DnsKeyResourceRecord : ResourceRecord {

        public ushort Flags { get; set; }

        public byte Protocol { get; set; } = 3;

        public byte Algorithm { get; set; }

        /// <summary>
        /// Public key in Base64 encoding
        /// </summary>
        public string PublicKey { get; set; } = "";

        public override ResourceRecordType Type => ResourceRecordType.DNSKEY;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

    }
}
