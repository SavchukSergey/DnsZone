namespace DnsZone.Records {
    /// <summary>
    /// Record of type not known to the library, in RFC 3597 generic format: <c>TYPE123 \# 4 0A000001</c>
    /// </summary>
    public class UnknownResourceRecord : ResourceRecord {

        /// <summary>
        /// Numeric resource record type
        /// </summary>
        public ushort TypeCode { get; set; }

        /// <summary>
        /// Raw RDATA
        /// </summary>
        public byte[] Data { get; set; } = [];

        public override ResourceRecordType Type => ResourceRecordType.Unknown;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

    }
}
