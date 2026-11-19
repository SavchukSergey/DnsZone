using System;
using System.Globalization;
using System.Net;
using System.Text;
using DnsZone.Records;

namespace DnsZone.Formatter {
    public class DnsZoneFormatterContext {

        private const char TAB_CHAR = '\t';

        private const int MAX_CHARACTER_STRING_LENGTH = 255;

        public string? Origin { get; set; }

        public TimeSpan? DefaultTtl { get; set; }


        public string PrevName { get; set; }

        public string PrevClass { get; set; }


        public DnsZoneFile Zone { get; }

        private StringBuilder Sb { get; }

        public DnsZoneFormatterContext(DnsZoneFile zone, StringBuilder sb) {
            Sb = sb;
            Zone = zone;
        }

        public void WriteU16(ushort val) {
            Sb.Append(val);
            Sb.Append(TAB_CHAR);
        }

        public void WriteDomainName(string val) {
            Sb.Append(val);
            Sb.Append(TAB_CHAR);
        }

        public void WriteAndCompressDomainName(string val) {
            WriteDomainName(CompressDomainName(val));
        }

        public void WriteEmail(string val) {
            Sb.Append(val);
            Sb.Append(TAB_CHAR);
        }

        public void WriteSerialNumber(string val) {
            Sb.Append(val);
            Sb.Append(TAB_CHAR);
        }

        public void WriteIpAddress(IPAddress val) {
            Sb.Append(val);
        }

        public void WriteIpAddressTab(IPAddress val) {
            Sb.Append(val);
            Sb.Append(TAB_CHAR);
        }

        public void WriteTimeSpan(TimeSpan val) {
            Sb.Append(DnsZoneUtils.FormatTimeSpan(val));
        }

        public void WriteTimeSpanTab(TimeSpan val) {
            Sb.Append(DnsZoneUtils.FormatTimeSpan(val));
            Sb.Append(TAB_CHAR);
        }

        public void WriteTag(string val) {
            Sb.Append(val);
            Sb.Append(TAB_CHAR);
        }

        public void WriteString(string? val) {
            Sb.Append('"');
            foreach (var ch in val ?? "") {
                if (ch == '\\' || ch == '"') {
                    Sb.Append('\\');
                    Sb.Append(ch);
                } else if (ch < 0x20 || ch == 0x7f) {
                    Sb.Append('\\');
                    Sb.Append(((int)ch).ToString("D3", CultureInfo.InvariantCulture));
                } else {
                    Sb.Append(ch);
                }
            }
            Sb.Append('"');
            Sb.Append(TAB_CHAR);
        }

        /// <summary>
        /// Writes text as a sequence of character-strings, each one at most 255 octets long (RFC 1035 section 3.3)
        /// </summary>
        public void WriteStrings(string? val) {
            val ??= "";
            if (val.Length == 0) {
                WriteString(val);
                return;
            }
            var start = 0;
            var octets = 0;
            for (var i = 0; i < val.Length; i++) {
                var charLength = char.IsSurrogatePair(val, i) ? 2 : 1;
                var charOctets = Encoding.UTF8.GetByteCount(val.AsSpan(i, charLength));
                if (octets + charOctets > MAX_CHARACTER_STRING_LENGTH) {
                    WriteString(val[start..i]);
                    start = i;
                    octets = 0;
                }
                octets += charOctets;
                i += charLength - 1;
            }
            WriteString(val[start..]);
        }

        public void WriteClass(string val) {
            Sb.Append(val);
            Sb.Append(TAB_CHAR);
        }

        public void WriteOrigin(string origin) {
            AppendLine(origin == "." ? "$ORIGIN ." : $"$ORIGIN {origin}.");
        }

        public void WriteTtl(TimeSpan value) {
            Sb.Append("$TTL ");
            WriteTimeSpan(value);
            Sb.Append('\n');
        }

        public void WriteResourceRecordType(ResourceRecordType val) {
            WriteResourceRecordType(DnsZoneUtils.FormatResourceRecordType(val));
        }

        public void WriteResourceRecordType(string val) {
            Sb.Append(val);
            Sb.Append(TAB_CHAR);
        }

        public void WriteU8(byte val) {
            Sb.Append(val);
            Sb.Append(TAB_CHAR);
        }

        public void WriteU32(uint val) {
            Sb.Append(val);
            Sb.Append(TAB_CHAR);
        }

        public string CompressDomainName(string val) {
            if (val == ".") {
                return val;
            }
            if (DnsZoneUtils.IsAbsoluteDomainName(val)) {
                val = val[..^1];
            }
            if (Origin != null) {
                if (string.Equals(val, Origin, StringComparison.OrdinalIgnoreCase)) {
                    return "@";
                }
                if (Origin != ".") {
                    var relativeSuffix = "." + Origin;
                    if (val.EndsWith(relativeSuffix, StringComparison.OrdinalIgnoreCase)) {
                        return val[..^relativeSuffix.Length];
                    }
                }
            }
            return val + ".";
        }

        public void AppendLine(string value) {
            Sb.Append(value);
            Sb.Append('\n');
        }

        public void AppendLine() {
            Sb.Append('\n');
        }

    }
}
