namespace DnsZone.Records {
    /// <summary>
    /// NSEC3PARAM record (RFC 5155 section 4)
    /// </summary>
    public class Nsec3ParamResourceRecord : ResourceRecord {

        public byte HashAlgorithm { get; set; }

        public byte Flags { get; set; }

        public ushort Iterations { get; set; }

        /// <summary>
        /// Salt in hex encoding. Empty string means no salt ("-" in zone file).
        /// </summary>
        public string Salt { get; set; } = "";

        public override ResourceRecordType Type => ResourceRecordType.NSEC3PARAM;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

    }
}
