using System;
using System.Diagnostics.CodeAnalysis;
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
                _ => throw new NotSupportedException($"unsupported resource record type {val}"),
            };

        }

        public static string FormatResourceRecordType(ResourceRecordType val) {
            return val switch {
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
                _ => throw new NotSupportedException($"unsupported resource record type {type}"),
            };

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

        public static TimeSpan ParseTimeSpan(string val) {
            var res = TimeSpan.Zero;
            int? part = null;
            foreach (var ch in val) {
                if (ch >= '0' && ch <= '9') {
                    part = (part ?? 0) * 10 + (ch - '0');
                } else {
                    if (part == null) throw new FormatException("timespan value expected");
                    res += char.ToLowerInvariant(ch) switch {
                        'w' => TimeSpan.FromDays(part.Value * 7),
                        'd' => TimeSpan.FromDays(part.Value),
                        'h' => TimeSpan.FromHours(part.Value),
                        'm' => TimeSpan.FromMinutes(part.Value),
                        's' => TimeSpan.FromSeconds(part.Value),
                        _ => throw new NotSupportedException($"timespan unit {ch} is not supported"),
                    };

                    part = null;
                }
            }
            if (part != null) {
                res += TimeSpan.FromSeconds(part.Value);
            }
            return res;
        }

        public static bool TryParseTimeSpan(string val, out TimeSpan timestamp) {
            timestamp = TimeSpan.Zero;
            int? part = null;
            foreach (var ch in val) {
                if (ch >= '0' && ch <= '9') {
                    part = (part ?? 0) * 10 + (ch - '0');
                } else {
                    if (part == null) return false;
                    switch (char.ToLower(ch)) {
                        case 'w':
                            timestamp += TimeSpan.FromDays(part.Value * 7);
                            break;
                        case 'd':
                            timestamp += TimeSpan.FromDays(part.Value);
                            break;
                        case 'h':
                            timestamp += TimeSpan.FromHours(part.Value);
                            break;
                        case 'm':
                            timestamp += TimeSpan.FromMinutes(part.Value);
                            break;
                        case 's':
                            timestamp += TimeSpan.FromSeconds(part.Value);
                            break;
                        default:
                            return false;
                    }
                    part = null;
                }
            }
            if (part != null) {
                timestamp += TimeSpan.FromSeconds(part.Value);
            }
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

            return sb.ToString();
        }

    }

}
