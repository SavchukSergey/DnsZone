namespace DnsZone.IO {
    public interface IDnsSource {

        string LoadContent(string fileName);

        /// <summary>
        /// Resolves included file name relative to referrer. Returns null if file is not found.
        /// </summary>
        string? ResolveFile(string fileName, string? referrer);

    }
}
