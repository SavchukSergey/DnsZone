using System.Text;

namespace DnsZone.Tokens {
    public readonly struct TokenPosition {

        public required readonly int Line { get; init; }

        public required readonly int LineStart { get; init; }

        public required readonly FileSource File { get; init; }

        public readonly string GetLine() {
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
