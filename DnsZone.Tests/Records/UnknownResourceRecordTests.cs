using System.Linq;
using DnsZone.Records;
using DnsZone.Tokens;
using NUnit.Framework;

namespace DnsZone.Tests.Records {
    [TestFixture]
    public class UnknownResourceRecordTests {

        // Examples from RFC 3597 section 5
        [Test]
        public void ParseTest() {
            const string str = @"
$TTL 1h
$ORIGIN example.com.
a.example.   IN          TYPE731         \# 6 abcd (
                         ef 01 23 45 )
b.example.   HS          TYPE62347       \# 0
www          IN          A               192.0.2.1";
            var zone = DnsZoneFile.Parse(str);
            Assert.That(zone.Records.Count, Is.EqualTo(3));

            var first = (UnknownResourceRecord)zone.Records[0];
            Assert.That(first.Name, Is.EqualTo("a.example"));
            Assert.That(first.Type, Is.EqualTo(ResourceRecordType.Unknown));
            Assert.That(first.TypeCode, Is.EqualTo(731));
            Assert.That(first.Data, Is.EqualTo(new byte[] { 0xab, 0xcd, 0xef, 0x01, 0x23, 0x45 }));

            var second = (UnknownResourceRecord)zone.Records[1];
            Assert.That(second.TypeCode, Is.EqualTo(62347));
            Assert.That(second.Class, Is.EqualTo("HS"));
            Assert.That(second.Data, Is.Empty);

            Assert.That(zone.Records[2], Is.AssignableTo<AResourceRecord>());
        }

        [Test]
        public void OutputTest() {
            var zone = new DnsZoneFile();
            zone.Records.Add(new UnknownResourceRecord {
                Name = "a.example.com",
                Class = "IN",
                TypeCode = 731,
                Data = [0xab, 0xcd, 0xef]
            });
            zone.Records.Add(new UnknownResourceRecord {
                Name = "b.example.com",
                Class = "IN",
                TypeCode = 62347
            });
            var sOutput = zone.ToString();
            Assert.That(sOutput, Is.EqualTo(
                ";TYPE731 records\na.example.com.\tIN\t0\tTYPE731\t\\#\t3\tABCDEF\t\n\n" +
                ";TYPE62347 records\nb.example.com.\tIN\t0\tTYPE62347\t\\#\t0\t\n\n"));

            var reparsed = DnsZoneFile.Parse(sOutput);
            Assert.That(reparsed.ToString(), Is.EqualTo(sOutput));
        }

        [Test]
        public void LengthMismatchTest() {
            Assert.Throws<TokenException>(() => DnsZoneFile.Parse(@"a.example.com. 3600 IN TYPE731 \# 3 abcd"));
        }

        [Test]
        public void MissingGenericMarkerTest() {
            Assert.Throws<TokenException>(() => DnsZoneFile.Parse(@"a.example.com. 3600 IN TYPE731 abcd"));
        }

        [Test]
        public void InvalidHexTest() {
            Assert.Throws<TokenException>(() => DnsZoneFile.Parse(@"a.example.com. 3600 IN TYPE731 \# 2 zzzz"));
        }
    }
}
