namespace DnsZone.Records {
    public interface IResourceRecordVisitor<TArg, TResult> {

        TResult Visit(AResourceRecord record, TArg arg);

        TResult Visit(AaaaResourceRecord record, TArg arg);

        TResult Visit(CNameResourceRecord record, TArg arg);

        TResult Visit(MxResourceRecord record, TArg arg);

        TResult Visit(NsResourceRecord record, TArg arg);

        TResult Visit(PtrResourceRecord record, TArg arg);

        TResult Visit(SoaResourceRecord record, TArg arg);

        TResult Visit(SrvResourceRecord record, TArg arg);

        TResult Visit(TxtResourceRecord record, TArg arg);

    }
}
