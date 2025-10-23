using System.Linq;
using DnsZone.Records;
using NUnit.Framework;

namespace DnsZone.Tests.Records {
    [TestFixture]
    public class CNameResourceRecordTests {

        [Test]
        public void ParseTest() {
            const string str = @"
; zone fragment example.com
; mail servers in the same zone
; will support incoming email with addresses of the format
; user@example.com
$TTL 2d ; zone default = 2 days or 172800 seconds
$ORIGIN example.com.
autodiscover    IN  CNAME   autodiscover.test.com.";
            var zone = DnsZoneFile.Parse(str);
            Assert.That(zone.Records.Count, Is.EqualTo(1));

            Assert.That(zone.Records.First(), Is.AssignableTo<CNameResourceRecord>());

            var record = (CNameResourceRecord)zone.Records.First();
            Assert.That(record.Name, Is.EqualTo("autodiscover.example.com"));
            Assert.That(record.Class, Is.EqualTo("IN"));
            Assert.That(record.Type, Is.EqualTo(ResourceRecordType.CNAME));
            Assert.That(record.CanonicalName, Is.EqualTo("autodiscover.test.com"));
        }

        [Test]
        public void OutputTest() {
            var zone = new DnsZoneFile();

            var record = new CNameResourceRecord {
                Name = "autodiscover.example.com",
                Class = "IN",
                CanonicalName = "autodiscover.test.com"
            };

            zone.Records.Add(record);
            var sOutput = zone.ToString();
            Assert.That(sOutput, Is.EqualTo(";CNAME records\nautodiscover.example.com.\tIN\t\tCNAME\tautodiscover.test.com.\t\n\n"));
        }
    }
}