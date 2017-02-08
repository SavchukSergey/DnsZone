using System.Text;

namespace DnsZone.Tokens {
    public struct TokenPosition {

        public int Line;

        public int LineStart;

        public FileSource File;

        public string GetLine() {
            var sb = new StringBuilder();
            for (var i = LineStart; i < File.Content.Length; i++) {
                var ch = File.Content[i];
                if (ch == '\r' || ch == '\n') break;
                sb.Append(ch);
            }
            return sb.ToString();
        }

    }
}
