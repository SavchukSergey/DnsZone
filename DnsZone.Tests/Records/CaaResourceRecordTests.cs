using System.Linq;
using DnsZone.Records;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            ClassicAssert.AreEqual(2, zone.Records.Count);

            ClassicAssert.IsAssignableFrom<CAAResourceRecord>(zone.Records.First());

            var record = (CAAResourceRecord)zone.Records.First();
            ClassicAssert.AreEqual("example.com", record.Name);
            ClassicAssert.AreEqual("IN", record.Class);
            ClassicAssert.AreEqual(ResourceRecordType.CAA, record.Type);
            ClassicAssert.AreEqual(0, record.Flag);
            ClassicAssert.AreEqual("iodef", record.Tag);
            ClassicAssert.AreEqual("mailto: hostmaster@example.com", record.Value);
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
            ClassicAssert.AreEqual(";CAA records\r\nexample.com.\tIN\t\tCAA\t0\tiodef\t\"letsencrypt.org\"\t\r\n\r\n", sOutput);
        }

    }
}
