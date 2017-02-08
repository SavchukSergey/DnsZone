using System;

namespace DnsZone.Records {
    public abstract class ResourceRecord {

        public virtual string Name { get; set; }

        public string Class { get; set; }

        public TimeSpan Ttl { get; set; }

        public abstract ResourceRecordType Type { get; }

        public abstract TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg);
    }
}
