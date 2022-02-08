using System;

namespace DnsZone.Records {
    public class SoaResourceRecord : ResourceRecord {

        public string NameServer { get; set; }

        public string ResponsibleEmail { get; set; }

        public string SerialNumber { get; set; }

        public TimeSpan Refresh { get; set; }

        public TimeSpan Retry { get; set; }

        public TimeSpan Expiry { get; set; }

        public TimeSpan Minimum { get; set; }

        public override ResourceRecordType Type => ResourceRecordType.SOA;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

        public override string ToString() {
            return $"{NameServer} {ResponsibleEmail} {SerialNumber} {Refresh.TotalSeconds} {Retry.TotalSeconds} {Expiry.TotalSeconds} {Minimum.TotalSeconds}";
        }
    }
}
