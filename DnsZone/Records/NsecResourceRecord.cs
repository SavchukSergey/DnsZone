using System.Collections.Generic;

namespace DnsZone.Records {
    /// <summary>
    /// NSEC record (RFC 4034 section 4)
    /// </summary>
    public class NsecResourceRecord : ResourceRecord {

        public string NextDomainName { get; set; } = "";

        /// <summary>
        /// Type mnemonics present at the owner name, e.g. "A", "RRSIG", "TYPE65534"
        /// </summary>
        public IList<string> Types { get; set; } = [];

        public override ResourceRecordType Type => ResourceRecordType.NSEC;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

    }
}
