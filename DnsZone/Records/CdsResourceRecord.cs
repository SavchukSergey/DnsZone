namespace DnsZone.Records {
    /// <summary>
    /// CDS record (RFC 7344 section 3.1). Has the same format as DS.
    /// </summary>
    public class CdsResourceRecord : ResourceRecord {

        public string KeyTag { get; set; } = "";

        public ushort Algorithm { get; set; }

        public ushort HashType { get; set; }

        public string Hash { get; set; } = "";

        public override ResourceRecordType Type => ResourceRecordType.CDS;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

    }
}
