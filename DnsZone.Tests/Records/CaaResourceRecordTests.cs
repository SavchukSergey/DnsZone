using System.Linq;
using DnsZone.Records;
using NUnit.Framework;

namespace DnsZone.Tests.Records {
    [TestFixture]
    public class CaaResourceRecordTests {

        [Test]
        public void ParseTest() {
            const string str = @"
; zone fragment example.com
; mail servers in the same zone
; will support incoming email with addresses of the format 
; user@example.com
$TTL 2d ; zone default = 2 days or 172800 seconds
$ORIGIN example.com.
example.com. IN	CAA 0	iodef		""mailto: hostmaster@example.com""
    IN  CAA 0   issue       ""letsencrypt.org""";
            var zone = DnsZoneFile.Parse(str);
            Assert.That(zone.Records.Count, Is.EqualTo(2));

            Assert.That(zone.Records.First(), Is.AssignableTo<CAAResourceRecord>());

            var record = (CAAResourceRecord)zone.Records.First();
            Assert.That(record.Name, Is.EqualTo("example.com"));
            Assert.That(record.Class, Is.EqualTo("IN"));
            Assert.That(record.Type, Is.EqualTo(ResourceRecordType.CAA));
            Assert.That(record.Flag, Is.EqualTo(0));
            Assert.That(record.Tag, Is.EqualTo("iodef"));
            Assert.That(record.Value, Is.EqualTo("mailto: hostmaster@example.com"));
        }
        
        [Test]
        public void OutputTest() {
            var zone = new DnsZoneFile();

            var record = new CAAResourceRecord {
                Name = "example.com",
                Class = "IN",
                Flag = 0,
                Tag = "iodef",
                Value = "letsencrypt.org"
            };

            zone.Records.Add(record);
            var sOutput = zone.ToString();
            Assert.That(sOutput, Is.EqualTo(";CAA records\nexample.com.\tIN\t\tCAA\t0\tiodef\t\"letsencrypt.org\"\t\n\n"));
        }

    }
}
