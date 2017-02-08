using System.Collections.Generic;

namespace DnsZone.Tokens {
    public class Tokenizer {

        private static void SkipWhitespace(ref string content, ref int chIndex) {
            while (chIndex < content.Length) {
                var ch = content[chIndex];
                if (ch != ' ' && ch != '\t') return;
                chIndex++;
            }
        }

        private static void SkipOne(ref string content, ref int chIndex, char match) {
            if (chIndex < content.Length) {
                if (content[chIndex] == match) {
                    chIndex++;
                }
            }
        }

        private static void SkipComment(ref string content, ref int pos) {
            while (pos < content.Length) {
                var ch = content[pos];
                if (ch == 0x0d || ch == 0x0a) {
                    return;
                }
                pos++;
            }
        }

        private static void SkipNewLineChar(ref string content, ref int pos, char firstChar) {
            SkipOne(ref content, ref pos, firstChar == '\n' ? '\r' : '\n');
        }

        private static char ReadCharCode(ref string content, ref int pos, ref TokenPosition position) {
            var code = 0;
            for (var i = 0; i < 3; i++) {
                if (pos >= content.Length) throw new TokenException("unexpected end of escape sequence", new Token { Position = position });
                var ch = content[pos++];
                if (ch < '0' || ch > '9') throw new TokenException("unexpected escape sequence", new Token { Position = position });
                code = code * 10 + (ch - '0');
            }
            return (char)code;
        }

        private static string ReadString(ref string content, ref int pos, ref TokenPosition position) {
            var quoteCh = content[pos++];
            var token = "";
            while (pos < content.Length) {
                var ch = content[pos++];
                if (ch == '\r' || ch == '\n') {
                    throw new TokenException("missing end quote", new Token { Position = position });
                }
                if (ch == '\\') {
                    if (pos >= content.Length) throw new TokenException("unexpected end of escape sequence", new Token { Position = position });
                    ch = content[pos];
                    if (char.IsDigit(ch)) {
                        ch = ReadCharCode(ref content, ref pos, ref position);
                        token += ch;
                    } else {
                        pos++;
                        token += ch;
                    }
                } else if (ch == quoteCh) {
                    return token;
                } else {
                    token += ch;
                }
            }
            throw new TokenException("missing end quote", new Token { Position = position });
        }

        private static string ReadCharacterString(ref string content, ref int chIndex) {
            var token = "";
            while (chIndex < content.Length) {
                var ch = content[chIndex++];
                if (char.IsWhiteSpace(ch)) {
                    SkipWhitespace(ref content, ref chIndex);
                    return token;
                }
                token += ch;
            }
            return token;
        }

        public IEnumerable<Token> Read(FileSource source) {
            var content = source.Content;
            var pos = 0;
            var lineNumber = 1;
            var lineStart = 0;
            var parentheses = 0;
            while (pos < content.Length) {
                if (pos >= content.Length) break;
                var position = new TokenPosition { File = source, Line = lineNumber, LineStart = lineStart };

                var ch = content[pos];

                switch (ch) {
                    case '$':
                        pos++;
                        yield return new Token {
                            Type = TokenType.Control,
                            Position = position,
                            StringValue = ReadCharacterString(ref content, ref pos)
                        };
                        break;
                    case '"':
                        yield return new Token {
                            Type = TokenType.QuotedString,
                            StringValue = ReadString(ref content, ref pos, ref position),
                            Position = position
                        };
                        SkipWhitespace(ref content, ref pos);
                        break;
                    case '(':
                        pos++;
                        parentheses++;
                        SkipWhitespace(ref content, ref pos);
                        break;
                    case ')':
                        pos++;
                        if (parentheses <= 0) throw new TokenException("unexpected closing parentheses", new Token {
                            Position = position
                        });
                        parentheses--;
                        SkipWhitespace(ref content, ref pos);
                        break;
                    case '\r':
                    case '\n':
                        pos++;
                        SkipNewLineChar(ref content, ref pos, ch);
                        lineNumber++;
                        lineStart = pos;
                        if (parentheses == 0) {
                            yield return new Token {
                                Type = TokenType.NewLine,
                                Position = position
                            };
                        }
                        break;
                    case ';':
                        SkipComment(ref content, ref pos);
                        break;
                    default:
                        if (char.IsWhiteSpace(ch)) {
                            if (pos == lineStart && parentheses == 0) {
                                yield return new Token {
                                    Type = TokenType.Whitespace,
                                    Position = position,
                                    StringValue = ""
                                };
                            }
                            SkipWhitespace(ref content, ref pos);
                        } else {
                            yield return new Token {
                                StringValue = ReadCharacterString(ref content, ref pos),
                                Position = position,
                                Type = TokenType.Literal
                            };
                        }
                        break;
                }
            }

            if (parentheses != 0) throw new TokenException("closing parentheses expected", new Token {
                Position = new TokenPosition {
                    File = source,
                    Line = lineNumber,
                    LineStart = lineStart
                }
            });
        }
    }
}
