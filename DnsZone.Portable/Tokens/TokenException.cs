using System;

namespace DnsZone.Tokens {
    public class TokenException : Exception {

        public TokenException(string message, Token token)
            : base(message) {
            Token = token;
        }

        public Token Token { get; }

    }
}
