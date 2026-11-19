using System.Collections.Generic;

namespace DnsZone.Records {
    /// <summary>
    /// NSEC3 record (RFC 5155 section 3)
    /// </summary>
    public class Nsec3ResourceRecord : ResourceRecord {

        public byte HashAlgorithm { get; set; }

        public byte Flags { get; set; }

        public ushort Iterations { get; set; }

        /// <summary>
        /// Salt in hex encoding. Empty string means no salt ("-" in zone file).
        /// </summary>
        public string Salt { get; set; } = "";

        /// <summary>
        /// Next hashed owner name in Base32hex encoding
        /// </summary>
        public string NextHashedOwnerName { get; set; } = "";

        /// <summary>
        /// Type mnemonics present at the original owner name, e.g. "A", "RRSIG", "TYPE65534"
        /// </summary>
        public IList<string> Types { get; set; } = [];

        public override ResourceRecordType Type => ResourceRecordType.NSEC3;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

    }
}
