using System;

namespace DnsZone.IO {
    public class StringDnsSource : IDnsSource {

        private readonly string _content;

        public StringDnsSource(string content) {
            _content = content;
        }

        public string LoadContent(string fileName) {
            if (string.IsNullOrWhiteSpace(fileName)) { 
                return _content;
            }
            throw new InvalidOperationException("cannot use $INCLUDE directive for single file DNS zone");
        }

        public string ResolveFile(string fileName, string referrer) {
            return fileName;
        }

    }
}
