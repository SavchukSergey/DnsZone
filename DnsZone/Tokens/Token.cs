namespace DnsZone.Tokens {
    public readonly struct Token {

        public required readonly TokenPosition Position { get; init; }

        public readonly TokenType Type { get; init; }

        public readonly string StringValue { get; init; }

        public override readonly string ToString() {
            return Type switch {
                TokenType.Control => $"${StringValue}",
                TokenType.Literal => StringValue,
                TokenType.NewLine => "\\r\\n",
                TokenType.Comments => StringValue,
                TokenType.Whitespace => " ",
                _ => StringValue,
            };

        }

    }
}
