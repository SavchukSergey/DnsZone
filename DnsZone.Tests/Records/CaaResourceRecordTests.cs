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
            Assert.AreEqual(2, zone.Records.Count);

            Assert.IsAssignableFrom<CAAResourceRecord>(zone.Records.First());

            var record = (CAAResourceRecord)zone.Records.First();
            Assert.AreEqual("example.com", record.Name);
            Assert.AreEqual("IN", record.Class);
            Assert.AreEqual(ResourceRecordType.CAA, record.Type);
            Assert.AreEqual(0, record.Flag);
            Assert.AreEqual("iodef", record.Tag);
            Assert.AreEqual("mailto: hostmaster@example.com", record.Value);
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
            string sOutput = zone.ToString();
            Assert.AreEqual(";CAA records\r\nexample.com.\tIN\t\tCAA\t0\tiodef\t\"letsencrypt.org\"\t\r\n\r\n", sOutput);
        }

    }
}
