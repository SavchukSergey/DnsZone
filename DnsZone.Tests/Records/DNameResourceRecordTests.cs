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
            Assert.AreEqual(1, zone.Records.Count);

            Assert.IsAssignableFrom<DnameResourceRecord>(zone.Records.First());

            var record = (DnameResourceRecord)zone.Records.First();
            Assert.AreEqual("subdomain.example.com", record.Name);
            Assert.AreEqual("IN", record.Class);
            Assert.AreEqual(ResourceRecordType.DNAME, record.Type);
            Assert.AreEqual("host.example.org.", record.Content);
        }
        
        [Test]
        public void OutputTest() {
            var zone = new DnsZoneFile();

            var record = new DnameResourceRecord {
                Name = "subdomain.example.com",
                Class = "IN",
                Content = "host.example.org."
            };
            
            zone.Records.Add(record);
            var sOutput = zone.ToString();
            Assert.AreEqual(";DNAME records\r\nsubdomain.example.com.\tIN\t\tDNAME\thost.example.org.\t\r\n\r\n", sOutput);
        }
    }
}
