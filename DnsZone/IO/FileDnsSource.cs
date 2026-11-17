using System;
using System.IO;

namespace DnsZone.IO {
    public class FileDnsSource : IDnsSource {

        private readonly string? _startFile;

        private readonly string? _startContent;

        public FileDnsSource() {
        }

        /// <summary>
        /// Creates file source
        /// </summary>
        /// <param name="startFile">Root zone file. Relative $INCLUDE paths are resolved against its directory.</param>
        public FileDnsSource(string startFile) {
            _startFile = startFile;
        }

        internal FileDnsSource(string startFile, string startContent) {
            _startFile = startFile;
            _startContent = startContent;
        }

        public string LoadContent(string fileName) {
            if (fileName == null && _startContent != null) return _startContent;
            fileName ??= _startFile ?? throw new InvalidOperationException("start file is not specified");
            using var file = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(file);
            return reader.ReadToEnd();
        }

        public string? ResolveFile(string fileName, string? referrer) {
            referrer ??= _startFile;
            if (Path.IsPathRooted(fileName)) {
                return FileExists(fileName) ? fileName : null;
            }
            var basePath = referrer != null ? Path.GetDirectoryName(Path.GetFullPath(referrer)) : Directory.GetCurrentDirectory();
            if (basePath != null) {
                var path = Path.Combine(basePath, fileName);
                if (FileExists(path)) return path;
            }

            return null;
        }

        private static bool FileExists(string fileName) {
            return File.Exists(fileName);
        }

    }
}
