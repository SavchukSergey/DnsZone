namespace DnsZone.Records {
    /// <summary>
    /// CDS record (RFC 7344 section 3.1). Has the same format as DS.
    /// </summary>
    public class CdsResourceRecord : ResourceRecord {

        public ushort KeyTag { get; set; }

        public byte Algorithm { get; set; }

        public byte HashType { get; set; }

        /// <summary>
        /// Digest in hex encoding
        /// </summary>
        public string Hash { get; set; } = "";

        public override ResourceRecordType Type => ResourceRecordType.CDS;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

    }
}
