using System.IO;

namespace DnsZone.IO {
    public class FileDnsSource : IDnsSource {

        public string LoadContent(string fileName) {
            using (var file = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                using (var reader = new StreamReader(file)) {
                    return reader.ReadToEnd();
                }
            }
        }

        public string ResolveFile(string fileName, string referrer) {
            var basePath = Path.GetDirectoryName(referrer);
            if (basePath != null) {
                var path = Path.Combine(basePath, fileName);
                if (FileExists(path)) return path;
            }

            return null;
        }

        private bool FileExists(string fileName) {
            return File.Exists(fileName);
        }

    }
}
