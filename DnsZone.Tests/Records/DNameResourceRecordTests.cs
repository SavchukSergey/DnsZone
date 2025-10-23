using System.Linq;
using DnsZone.Records;
using NUnit.Framework;

namespace DnsZone.Tests.Records {
    [TestFixture]
    public class DNameResourceRecordTests {

        [Test]
        public void ParseTest() {
            const string str = @"
; zone fragment example.com
; mail servers in the same zone
; will support incoming email with addresses of the format 
; user@example.com
$TTL 2d ; zone default = 2 days or 172800 seconds
$ORIGIN example.com.
subdomain.example.com. IN	DNAME  host.example.org.";
            var zone = DnsZoneFile.Parse(str);
            Assert.That(zone.Records.Count, Is.EqualTo(1));

            Assert.That(zone.Records.First(), Is.AssignableTo<DNameResourceRecord>());

            var record = (DNameResourceRecord)zone.Records.First();
            Assert.That(record.Name, Is.EqualTo("subdomain.example.com"));
            Assert.That(record.Class, Is.EqualTo("IN"));
            Assert.That(record.Type, Is.EqualTo(ResourceRecordType.DNAME));
            Assert.That(record.Target, Is.EqualTo("host.example.org."));
        }
        
        [Test]
        public void OutputTest() {
            var zone = new DnsZoneFile();

            var record = new DNameResourceRecord {
                Name = "subdomain.example.com",
                Class = "IN",
                Target = "host.example.org."
            };
            
            zone.Records.Add(record);
            var sOutput = zone.ToString();
            Assert.That(sOutput, Is.EqualTo(";DNAME records\nsubdomain.example.com.\tIN\t\tDNAME\thost.example.org.\t\n\n"));
        }
    }
}
