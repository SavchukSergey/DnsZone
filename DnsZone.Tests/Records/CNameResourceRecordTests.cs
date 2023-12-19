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
            Assert.AreEqual(1, zone.Records.Count);

            Assert.IsAssignableFrom<CNameResourceRecord>(zone.Records.First());

            var record = (CNameResourceRecord)zone.Records.First();
            Assert.AreEqual("autodiscover.example.com", record.Name);
            Assert.AreEqual("IN", record.Class);
            Assert.AreEqual(ResourceRecordType.CNAME, record.Type);
            Assert.AreEqual("autodiscover.test.com", record.CanonicalName);
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
            Assert.AreEqual(";CNAME records\r\nautodiscover.example.com.\tIN\t\tCNAME\tautodiscover.test.com.\t\r\n\r\n", sOutput);
        }
    }
}