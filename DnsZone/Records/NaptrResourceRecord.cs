namespace DnsZone.Records {
    public class NaptrResourceRecord : ResourceRecord {

        public ushort Order { get; set; }

        public ushort Preference { get; set; }
        
        public string Flags { get; set; }

        public string Services { get; set; }

        public string Regexp { get; set; }

        public string Replacement { get; set; }

        public override ResourceRecordType Type => ResourceRecordType.NAPTR;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

        public override string ToString() {
            return $"{Order} {Preference} {Flags} {Services} {Regexp} {Replacement}";
        }
    }
}
