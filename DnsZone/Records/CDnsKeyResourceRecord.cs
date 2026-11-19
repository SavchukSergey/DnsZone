namespace DnsZone.Records {
    /// <summary>
    /// CDNSKEY record (RFC 7344 section 3.2). Has the same format as DNSKEY.
    /// </summary>
    public class CDnsKeyResourceRecord : ResourceRecord {

        public ushort Flags { get; set; }

        public byte Protocol { get; set; } = 3;

        public byte Algorithm { get; set; }

        /// <summary>
        /// Public key in Base64 encoding
        /// </summary>
        public string PublicKey { get; set; } = "";

        public override ResourceRecordType Type => ResourceRecordType.CDNSKEY;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

    }
}
