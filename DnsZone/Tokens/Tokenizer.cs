using System;
using System.Collections.Generic;
using System.Text;

namespace DnsZone.Tokens {
    public static class Tokenizer {

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
            if (code > 255) throw new TokenException("escape sequence value is out of range", new Token { Position = position });
            return (char)code;
        }

        private static string ReadString(ref string content, ref int pos, ref TokenPosition position) {
            var quoteCh = content[pos++];
            var token = new StringBuilder();
            while (pos < content.Length) {
                var ch = content[pos++];
                if (ch == '\r' || ch == '\n') {
                    throw new TokenException("missing end quote", new Token { Position = position });
                }
                if (ch == '\\') {
                    if (pos >= content.Length) throw new TokenException("unexpected end of escape sequence", new Token { Position = position });
                    ch = content[pos];
                    if (ch >= '0' && ch <= '9') {
                        token.Append(ReadCharCode(ref content, ref pos, ref position));
                    } else {
                        pos++;
                        token.Append(ch);
                    }
                } else if (ch == quoteCh) {
                    return token.ToString();
                } else {
                    token.Append(ch);
                }
            }
            throw new TokenException("missing end quote", new Token { Position = position });
        }

        private static bool IsTokenTerminator(char ch) {
            return char.IsWhiteSpace(ch) || ch == ';' || ch == '(' || ch == ')' || ch == '"';
        }

        /// <summary>
        /// Reads unquoted token. Escape sequences are preserved as is (so that escaped dots in domain names keep their meaning),
        /// but escaped characters never terminate the token.
        /// </summary>
        private static string ReadCharacterString(ref string content, ref int chIndex, ref TokenPosition position) {
            var startIndex = chIndex;
            while (chIndex < content.Length) {
                var ch = content[chIndex];
                if (ch == '\\') {
                    chIndex++;
                    if (chIndex >= content.Length) throw new TokenException("unexpected end of escape sequence", new Token { Position = position });
                    if (content[chIndex] >= '0' && content[chIndex] <= '9') {
                        ReadCharCode(ref content, ref chIndex, ref position);
                    } else {
                        chIndex++;
                    }
                    continue;
                }
                if (IsTokenTerminator(ch)) {
                    break;
                }
                chIndex++;
            }
            var res = content.Substring(startIndex, chIndex - startIndex);
            SkipWhitespace(ref content, ref chIndex);
            return res;
        }

        public static IEnumerable<Token> Read(FileSource source) {
            var content = source.Content;
            var pos = 0;
            var lineNumber = 1;
            var lineStart = 0;
            var parentheses = 0;
            while (pos < content.Length) {
                var position = new TokenPosition { File = source, Line = lineNumber, LineStart = lineStart };

                var ch = content[pos];

                switch (ch) {
                    case '$' when pos == lineStart && parentheses == 0:
                        pos++;
                        yield return new Token {
                            Type = TokenType.Control,
                            Position = position,
                            StringValue = ReadCharacterString(ref content, ref pos, ref position)
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
                            if (pos < content.Length && char.IsWhiteSpace(content[pos]) && content[pos] != '\r' && content[pos] != '\n') {
                                pos++; // other unicode whitespace
                            }
                        } else {
                            yield return new Token {
                                StringValue = ReadCharacterString(ref content, ref pos, ref position),
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
