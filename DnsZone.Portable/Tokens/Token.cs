namespace DnsZone.Tokens {
    public struct Token {

        public TokenPosition Position { get; set; }

        public TokenType Type { get; set; }

        public string StringValue { get; set; }

        public override string ToString() {
            switch (Type) {
                case TokenType.Control: return $"${StringValue}";
                case TokenType.Literal: return StringValue;
                case TokenType.NewLine: return "\\r\\n";
                case TokenType.Comments: return StringValue;
                case TokenType.Whitespace: return " ";
            }
            return StringValue;
        }

    }
}
