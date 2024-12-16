using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace DnsZone.Tests {
    [TestFixture]
    public class DnsZoneUtilsTests {

        [TestCase("2w", "14.0:0:0")]
        [TestCase("3d", "3.0:0:0")]
        [TestCase("5h", "5:0:0")]
        [TestCase("7m", "0:7:0")]
        [TestCase("13s", "0:0:13")]
        [TestCase("1w2d5h3m10s", "9.5:3:10")]
        public void ParseTimeSpanTests(string zoneStr, string spanStr) {
            var actual = DnsZoneUtils.ParseTimeSpan(zoneStr);
            var expected = TimeSpan.Parse(spanStr);
            ClassicAssert.AreEqual(expected, actual);
        }

        [TestCase("2w", "14.0:0:0")]
        [TestCase("3d", "3.0:0:0")]
        [TestCase("5h", "5:0:0")]
        [TestCase("7m", "0:7:0")]
        [TestCase("13", "0:0:13")]
        [TestCase("1w2d5h3m10s", "9.5:3:10")]
        public void FormatTimeSpanTests(string expected, string span) {
            var val = TimeSpan.Parse(span);
            var actual = DnsZoneUtils.FormatTimeSpan(val);
            ClassicAssert.AreEqual(expected, actual);
        }
    }
}
