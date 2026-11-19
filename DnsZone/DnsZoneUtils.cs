using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using DnsZone.Records;

namespace DnsZone {
    public static class DnsZoneUtils {

        public static ResourceRecordType ParseResourceRecordType(string val) {
            return val.ToUpperInvariant() switch {
                "A" => ResourceRecordType.A,
                "AAAA" => ResourceRecordType.AAAA,
                "NS" => ResourceRecordType.NS,
                "MD" => ResourceRecordType.MD,
                "MF" => ResourceRecordType.MF,
                "CNAME" => ResourceRecordType.CNAME,
                "SOA" => ResourceRecordType.SOA,
                "MB" => ResourceRecordType.MB,
                "MG" => ResourceRecordType.MG,
                "MR" => ResourceRecordType.MR,
                "NULL" => ResourceRecordType.NULL,
                "WKS" => ResourceRecordType.WKS,
                "PTR" => ResourceRecordType.PTR,
                "SRV" => ResourceRecordType.SRV,
                "SSHFP" => ResourceRecordType.SSHFP,
                "HINFO" => ResourceRecordType.HINFO,
                "MX" => ResourceRecordType.MX,
                "TXT" => ResourceRecordType.TXT,
                "CAA" => ResourceRecordType.CAA,
                "TLSA" => ResourceRecordType.TLSA,
                "ALIAS" => ResourceRecordType.ALIAS,
                "DNAME" => ResourceRecordType.DNAME,
                "DS" => ResourceRecordType.DS,
                "LUA" => ResourceRecordType.LUA,
                "NAPTR" => ResourceRecordType.NAPTR,
                "DNSKEY" => ResourceRecordType.DNSKEY,
                "RRSIG" => ResourceRecordType.RRSIG,
                "NSEC" => ResourceRecordType.NSEC,
                "NSEC3" => ResourceRecordType.NSEC3,
                "NSEC3PARAM" => ResourceRecordType.NSEC3PARAM,
                "CDS" => ResourceRecordType.CDS,
                "CDNSKEY" => ResourceRecordType.CDNSKEY,
                _ => throw new NotSupportedException($"unsupported resource record type {val}"),
            };

        }

        /// <summary>
        /// Formats record type mnemonic. <see cref="ResourceRecordType.Unknown"/> has no mnemonic of its own:
        /// use <see cref="FormatResourceRecordType(ResourceRecord)"/> or <see cref="FormatUnknownType"/> to get TYPEnnn.
        /// </summary>
        public static string FormatResourceRecordType(ResourceRecordType val) {
            return val switch {
                ResourceRecordType.Unknown => throw new NotSupportedException("unknown record type has no mnemonic, format it from UnknownResourceRecord.TypeCode"),
                ResourceRecordType.A => "A",
                ResourceRecordType.AAAA => "AAAA",
                ResourceRecordType.NS => "NS",
                ResourceRecordType.MD => "MD",
                ResourceRecordType.MF => "MF",
                ResourceRecordType.CNAME => "CNAME",
                ResourceRecordType.SOA => "SOA",
                ResourceRecordType.MB => "MB",
                ResourceRecordType.MG => "MG",
                ResourceRecordType.MR => "MR",
                ResourceRecordType.NULL => "NULL",
                ResourceRecordType.WKS => "WKS",
                ResourceRecordType.PTR => "PTR",
                ResourceRecordType.SRV => "SRV",
                ResourceRecordType.HINFO => "HINFO",
                ResourceRecordType.MX => "MX",
                ResourceRecordType.TXT => "TXT",
                ResourceRecordType.CAA => "CAA",
                ResourceRecordType.TLSA => "TLSA",
                ResourceRecordType.SSHFP => "SSHFP",
                ResourceRecordType.ALIAS => "ALIAS",
                ResourceRecordType.DNAME => "DNAME",
                ResourceRecordType.DS => "DS",
                ResourceRecordType.LUA => "LUA",
                ResourceRecordType.NAPTR => "NAPTR",
                ResourceRecordType.DNSKEY => "DNSKEY",
                ResourceRecordType.RRSIG => "RRSIG",
                ResourceRecordType.NSEC => "NSEC",
                ResourceRecordType.NSEC3 => "NSEC3",
                ResourceRecordType.NSEC3PARAM => "NSEC3PARAM",
                ResourceRecordType.CDS => "CDS",
                ResourceRecordType.CDNSKEY => "CDNSKEY",
                _ => throw new NotSupportedException($"unsupported resource record type {val}"),
            };

        }


        public static ResourceRecord CreateRecord(ResourceRecordType type) {
            return type switch {
                ResourceRecordType.A => new AResourceRecord(),
                ResourceRecordType.AAAA => new AaaaResourceRecord(),
                ResourceRecordType.NS => new NsResourceRecord(),
                //case ResourceRecordType.MD: return ResourceRecordType.MD;
                //case ResourceRecordType.MF: return ResourceRecordType.MF;
                ResourceRecordType.CNAME => new CNameResourceRecord(),
                ResourceRecordType.SOA => new SoaResourceRecord(),
                //case ResourceRecordType.MB: return ResourceRecordType.MB;
                //case ResourceRecordType.MG: return ResourceRecordType.MG;
                //case ResourceRecordType.MR: return ResourceRecordType.MR;
                //case ResourceRecordType.NULL: return ResourceRecordType.NULL;
                //case ResourceRecordType.WKS: return ResourceRecordType.WKS;
                ResourceRecordType.PTR => new PtrResourceRecord(),
                ResourceRecordType.SRV => new SrvResourceRecord(),
                ResourceRecordType.HINFO => new HInfoResourceRecord(),
                ResourceRecordType.MX => new MxResourceRecord(),
                ResourceRecordType.TXT => new TxtResourceRecord(),
                ResourceRecordType.CAA => new CAAResourceRecord(),
                ResourceRecordType.TLSA => new TLSAResourceRecord(),
                ResourceRecordType.SSHFP => new SSHFPResourceRecord(),
                ResourceRecordType.ALIAS => new AliasResourceRecord(),
                ResourceRecordType.DNAME => new DNameResourceRecord(),
                ResourceRecordType.DS => new DsResourceRecord(),
                ResourceRecordType.LUA => new LuaResourceRecord(),
                ResourceRecordType.NAPTR => new NaptrResourceRecord(),
                ResourceRecordType.Unknown => new UnknownResourceRecord(),
                ResourceRecordType.DNSKEY => new DnsKeyResourceRecord(),
                ResourceRecordType.RRSIG => new RrsigResourceRecord(),
                ResourceRecordType.NSEC => new NsecResourceRecord(),
                ResourceRecordType.NSEC3 => new Nsec3ResourceRecord(),
                ResourceRecordType.NSEC3PARAM => new Nsec3ParamResourceRecord(),
                ResourceRecordType.CDS => new CdsResourceRecord(),
                ResourceRecordType.CDNSKEY => new CDnsKeyResourceRecord(),
                _ => throw new NotSupportedException($"unsupported resource record type {type}"),
            };

        }

        /// <summary>
        /// Formats type of the record. Unknown records are formatted as TYPEnnn (RFC 3597 section 5)
        /// </summary>
        public static string FormatResourceRecordType(ResourceRecord record) {
            if (record is UnknownResourceRecord unknown) {
                return FormatUnknownType(unknown.TypeCode);
            }
            return FormatResourceRecordType(record.Type);
        }

        /// <summary>
        /// Parses generic type mnemonic TYPEnnn (RFC 3597 section 5)
        /// </summary>
        public static bool TryParseUnknownType(string val, out ushort typeCode) {
            typeCode = 0;
            if (val.Length <= 4 || !val.StartsWith("TYPE", StringComparison.OrdinalIgnoreCase)) return false;
            // type 0 is reserved (RFC 6895 section 3.1)
            return ushort.TryParse(val.AsSpan(4), NumberStyles.None, CultureInfo.InvariantCulture, out typeCode) && typeCode != 0;
        }

        public static string FormatUnknownType(ushort typeCode) {
            return "TYPE" + typeCode.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Checks whether value looks like a type mnemonic (e.g. A, RRSIG or generic TYPEnnn).
        /// Mnemonics of types not supported by the library are allowed too.
        /// </summary>
        public static bool IsTypeMnemonic(string val) {
            if (val.Length == 0 || !char.IsAsciiLetter(val[0])) return false;
            foreach (var ch in val) {
                if (!char.IsAsciiLetterOrDigit(ch) && ch != '-') return false;
            }
            return true;
        }

        private const string DNSSEC_TIME_FORMAT = "yyyyMMddHHmmss";

        /// <summary>
        /// Parses RRSIG signature time (RFC 4034 section 3.2): either YYYYMMDDHHmmSS in UTC or seconds since epoch
        /// </summary>
        public static bool TryParseDnssecTime(string val, out DateTime time) {
            time = default;
            if (val.Length == 14) {
                return DateTime.TryParseExact(val, DNSSEC_TIME_FORMAT, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out time);
            }
            if (!uint.TryParse(val, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds)) return false;
            time = DateTime.UnixEpoch.AddSeconds(seconds);
            return true;
        }

        public static string FormatDnssecTime(DateTime time) {
            if (time.Kind == DateTimeKind.Local) time = time.ToUniversalTime();
            return time.ToString(DNSSEC_TIME_FORMAT, CultureInfo.InvariantCulture);
        }

        public static bool IsBase64(string val) {
            if (val.Length == 0 || val.Length % 4 != 0) return false;
            var buffer = new byte[val.Length / 4 * 3];
            return Convert.TryFromBase64String(val, buffer, out _);
        }

        /// <summary>
        /// Checks whether value is Base32hex without padding (RFC 4648 section 7) as used by NSEC3 (RFC 5155 section 3.3)
        /// </summary>
        public static bool IsBase32Hex(string val) {
            if (val.Length == 0) return false;
            var rest = val.Length % 8;
            if (rest == 1 || rest == 3 || rest == 6) return false;
            foreach (var ch in val) {
                var upper = char.ToUpperInvariant(ch);
                if (!char.IsAsciiDigit(upper) && (upper < 'A' || upper > 'V')) return false;
            }
            return true;
        }

        public static bool TryParseHex(string val, out byte[] data) {
            data = [];
            if (val.Length % 2 != 0) return false;
            try {
                data = Convert.FromHexString(val);
                return true;
            } catch (FormatException) {
                return false;
            }
        }

        public static bool TryParseClass(string val, [NotNullWhen(true)] out string? @class) {
            @class = null;
            switch (val.ToUpperInvariant()) {
                case "IN":
                    @class = "IN";
                    break;
                case "CS":
                    @class = "CS";
                    break;
                case "CH":
                    @class = "CH";
                    break;
                case "HS":
                    @class = "HS";
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Maximum TTL value according to RFC 2181 section 8
        /// </summary>
        public static readonly TimeSpan MaxTtl = TimeSpan.FromSeconds(int.MaxValue);

        public static TimeSpan ParseTimeSpan(string val) {
            if (!TryParseTimeSpan(val, out var res)) {
                throw new FormatException($"invalid timespan value '{val}'");
            }
            return res;
        }

        public static bool TryParseTimeSpan(string? val, out TimeSpan timestamp) {
            timestamp = TimeSpan.Zero;
            if (string.IsNullOrEmpty(val)) return false;
            long total = 0;
            long? part = null;
            foreach (var ch in val) {
                if (ch >= '0' && ch <= '9') {
                    part = (part ?? 0) * 10 + (ch - '0');
                    if (part > int.MaxValue) return false;
                } else {
                    if (part == null) return false;
                    long multiplier;
                    switch (char.ToLowerInvariant(ch)) {
                        case 'w':
                            multiplier = 7 * 24 * 3600;
                            break;
                        case 'd':
                            multiplier = 24 * 3600;
                            break;
                        case 'h':
                            multiplier = 3600;
                            break;
                        case 'm':
                            multiplier = 60;
                            break;
                        case 's':
                            multiplier = 1;
                            break;
                        default:
                            return false;
                    }
                    total += part.Value * multiplier;
                    if (total > int.MaxValue) return false;
                    part = null;
                }
            }
            if (part != null) {
                total += part.Value;
                if (total > int.MaxValue) return false;
            }
            timestamp = TimeSpan.FromSeconds(total);
            return true;
        }

        public static string FormatTimeSpan(TimeSpan val) {
            var sb = new StringBuilder();

            var weeks = Math.Floor(val.TotalDays / 7);
            if (weeks > 0) {
                sb.Append($"{weeks}w");
                val = val.Subtract(TimeSpan.FromDays(7 * weeks));
            }

            var days = Math.Floor(val.TotalDays);
            if (days > 0) {
                sb.Append($"{days}d");
                val = val.Subtract(TimeSpan.FromDays(days));
            }

            var hours = Math.Floor(val.TotalHours);
            if (hours > 0) {
                sb.Append($"{hours}h");
                val = val.Subtract(TimeSpan.FromHours(hours));
            }

            var minutes = Math.Floor(val.TotalMinutes);
            if (minutes > 0) {
                sb.Append($"{minutes}m");
                val = val.Subtract(TimeSpan.FromMinutes(minutes));
            }

            var seconds = Math.Floor(val.TotalSeconds);
            if (seconds > 0) {
                sb.Append(sb.Length > 0 ? $"{seconds}s" : $"{seconds}");
            }

            return sb.Length > 0 ? sb.ToString() : "0";
        }

        /// <summary>
        /// Checks whether domain name in presentation format is absolute (ends with non-escaped dot)
        /// </summary>
        public static bool IsAbsoluteDomainName(string val) {
            if (!val.EndsWith('.')) return false;
            var backslashes = 0;
            for (var i = val.Length - 2; i >= 0 && val[i] == '\\'; i--) {
                backslashes++;
            }
            return backslashes % 2 == 0;
        }

        /// <summary>
        /// Decodes RFC 1035 escape sequences (\X and \DDD) of unquoted character-string
        /// </summary>
        public static string UnescapeCharacterString(string val) {
            if (val.IndexOf('\\') < 0) return val;
            var sb = new StringBuilder(val.Length);
            for (var i = 0; i < val.Length; i++) {
                var ch = val[i];
                if (ch == '\\' && i + 1 < val.Length) {
                    if (i + 3 < val.Length && char.IsAsciiDigit(val[i + 1]) && char.IsAsciiDigit(val[i + 2]) && char.IsAsciiDigit(val[i + 3])) {
                        sb.Append((char)((val[i + 1] - '0') * 100 + (val[i + 2] - '0') * 10 + (val[i + 3] - '0')));
                        i += 3;
                    } else {
                        sb.Append(val[i + 1]);
                        i++;
                    }
                } else {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }

    }

}
