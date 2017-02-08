using System;
using System.Net;
using System.Text;
using DnsZone.Records;

namespace DnsZone.Formatter {
    public class DnsZoneFormatterContext {

        private const string TAB_CHAR = "\t";

        public string Origin { get; set; }

        public TimeSpan? DefaultTtl { get; set; }


        public string PrevName { get; set; }

        public string PrevClass { get; set; }


        public DnsZoneFile Zone { get; }

        public StringBuilder Sb { get; }

        public DnsZoneFormatterContext(DnsZoneFile zone, StringBuilder sb) {
            Sb = sb;
            Zone = zone;
        }

        public void WritePreference(ushort val) {
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
            Sb.Append(TAB_CHAR);
        }

        public void WriteTimeSpan(TimeSpan val) {
            Sb.Append(DnsZoneUtils.FormatTimeSpan(val));
            Sb.Append(TAB_CHAR);
        }

        public void WriteString(string val) {
            val = val
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
            Sb.Append($"\"{val}\"");
            Sb.Append(TAB_CHAR);
        }

        public void WriteClass(string val) {
            Sb.Append(val);
            Sb.Append(TAB_CHAR);
        }

        public void WriteOrigin(string origin) {
            Sb.AppendLine($"$ORIGIN {origin}.");
        }

        public void WriteResourceRecordType(ResourceRecordType  val) {
            Sb.Append(DnsZoneUtils.FormatResourceRecordType(val));
            Sb.Append(TAB_CHAR);
        }

        public string CompressDomainName(string val) {
            if (val == Origin) {
                return "@";
            }
            var relativeSuffix = "." + Origin;
            if (Origin != null && val.EndsWith(relativeSuffix)) {
                return val.Substring(0, val.Length - relativeSuffix.Length);
            }
            return val + ".";
        }

    }
}
