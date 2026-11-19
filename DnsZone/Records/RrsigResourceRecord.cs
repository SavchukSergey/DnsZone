using System;

namespace DnsZone.Records {
    /// <summary>
    /// RRSIG record (RFC 4034 section 3)
    /// </summary>
    public class RrsigResourceRecord : ResourceRecord {

        /// <summary>
        /// Type mnemonic of the covered RRset, e.g. "A" or "TYPE65534"
        /// </summary>
        public string TypeCovered { get; set; } = "";

        public byte Algorithm { get; set; }

        public byte Labels { get; set; }

        public TimeSpan OriginalTtl { get; set; }

        /// <summary>
        /// Signature expiration time (UTC)
        /// </summary>
        public DateTime SignatureExpiration { get; set; }

        /// <summary>
        /// Signature inception time (UTC)
        /// </summary>
        public DateTime SignatureInception { get; set; }

        public ushort KeyTag { get; set; }

        public string SignerName { get; set; } = "";

        /// <summary>
        /// Signature in Base64 encoding
        /// </summary>
        public string Signature { get; set; } = "";

        public override ResourceRecordType Type => ResourceRecordType.RRSIG;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

    }
}
