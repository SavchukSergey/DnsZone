namespace DnsZone.Records {
    public class LuaResourceRecord : ResourceRecord {

        public string TargetType { get; set; }

        public string Script { get; set; }

        public override ResourceRecordType Type => ResourceRecordType.LUA;

        public override TResult AcceptVistor<TArg, TResult>(IResourceRecordVisitor<TArg, TResult> visitor, TArg arg) {
            return visitor.Visit(this, arg);
        }

    }
}
