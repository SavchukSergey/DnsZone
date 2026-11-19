using System.Linq;
using System.Text;
using DnsZone.Formatter;
using DnsZone.IO;
using DnsZone.Parser;
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

        [Test]
        public void TypeCodeOutOfRangeTest() {
            var exc = Assert.Throws<TokenException>(() => DnsZoneFile.Parse(@"a.example.com. 3600 IN TYPE70000 \# 0"));
            Assert.That(exc!.Message, Does.Contain("unsupported resource record type TYPE70000"));
        }

        [Test]
        public void TypeZeroTest() {
            var exc = Assert.Throws<TokenException>(() => DnsZoneFile.Parse(@"a.example.com. 3600 IN TYPE0 \# 0"));
            Assert.That(exc!.Message, Does.Contain("unsupported resource record type TYPE0"));
        }

        [Test]
        public void UnknownTypeUtilsTest() {
            Assert.That(DnsZoneUtils.CreateRecord(ResourceRecordType.Unknown), Is.TypeOf<UnknownResourceRecord>());
            Assert.Throws<System.NotSupportedException>(() => DnsZoneUtils.FormatResourceRecordType(ResourceRecordType.Unknown));
            Assert.That(DnsZoneUtils.FormatResourceRecordType(new UnknownResourceRecord { TypeCode = 731 }), Is.EqualTo("TYPE731"));
        }

        [Test]
        public void ReadResourceRecordTypeTest() {
            var tokens = Tokenizer.Read(new FileSource { Content = "rrsig TYPE731" });
            var context = new DnsZoneParseContext(tokens, new StringDnsSource(""));
            Assert.That(context.ReadResourceRecordType(), Is.EqualTo(ResourceRecordType.RRSIG));
            Assert.That(context.ReadResourceRecordType(out var typeCode), Is.EqualTo(ResourceRecordType.Unknown));
            Assert.That(typeCode, Is.EqualTo(731));
        }

        [Test]
        public void WriteResourceRecordTypeTest() {
            var sb = new StringBuilder();
            var context = new DnsZoneFormatterContext(new DnsZoneFile(), sb);
            context.WriteResourceRecordType(ResourceRecordType.NSEC3PARAM);
            context.WriteResourceRecordType("TYPE731");
            Assert.That(sb.ToString(), Is.EqualTo("NSEC3PARAM\tTYPE731\t"));
        }
    }
}
