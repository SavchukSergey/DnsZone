using System.Linq;
using System.Net;
using DnsZone.Records;
using NUnit.Framework;

namespace DnsZone.Tests.Records {
    [TestFixture]
    public class AResourceRecordTests {

        [Test]
        public void ParseTest() {
            const string str = @"
; zone fragment example.com
; mail servers in the same zone
; will support incoming email with addresses of the format
; user@example.com
$TTL 2d ; zone default = 2 days or 172800 seconds
$ORIGIN example.com.
mail          IN  A     192.0.2.3             ; IPv4 address for mail.example.com
mail2         IN  A     192.0.2.4             ; IPv4 address for mail2.example.com
mail3         IN  A     192.0.2.5             ; IPv4 address for mail3.example.com";
            var zone = DnsZoneFile.Parse(str);
            Assert.That(zone.Records.Count, Is.EqualTo(3));

            Assert.That(zone.Records.First(), Is.AssignableTo<AResourceRecord>());

            var record = (AResourceRecord)zone.Records.First();
            Assert.That(record.Name, Is.EqualTo("mail.example.com"));
            Assert.That(record.Class, Is.EqualTo("IN"));
            Assert.That(record.Type, Is.EqualTo(ResourceRecordType.A));
            Assert.That(record.Address, Is.EqualTo(IPAddress.Parse("192.0.2.3")));
        }

        [Test]
        public void OutputTest() {
            var zone = new DnsZoneFile();

            var record = new AResourceRecord {
                Name = "example.com",
                Class = "IN",
                Address = IPAddress.Parse("192.0.2.3")
            };

            zone.Records.Add(record);
            var sOutput = zone.ToString();
            Assert.That(sOutput, Is.EqualTo(";A records\nexample.com.\tIN\t\tA\t192.0.2.3\t\n\n"));
        }
    }
}