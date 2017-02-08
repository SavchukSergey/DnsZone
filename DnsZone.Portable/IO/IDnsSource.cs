namespace DnsZone.IO {
    public interface IDnsSource {

        string LoadContent(string fileName);

        string ResolveFile(string fileName, string referrer);

    }
}
