using System.Text;

namespace DnsZone.Records {
    /// <summary>
    /// Helpers for owner names prefixed with underscore labels, like _service._proto.host (SRV) or _port._proto.host (TLSA)
    /// </summary>
    internal static class ServiceName {

        public static string Format(string? first, string? protocol, string? host) {
            var sb = new StringBuilder();
            if (first != null) sb.Append('_').Append(first).Append('.');
            if (protocol != null) sb.Append('_').Append(protocol).Append('.');
            sb.Append(host);
            return sb.ToString();
        }

        /// <summary>
        /// Takes leading underscore label (without underscore) if present
        /// </summary>
        public static bool TryTakeLabel(ref string value, out string label) {
            label = "";
            if (!value.StartsWith('_')) return false;
            var dotIndex = value.IndexOf('.');
            if (dotIndex < 0) return false;
            label = value[1..dotIndex];
            value = value[(dotIndex + 1)..];
            return true;
        }

    }
}
