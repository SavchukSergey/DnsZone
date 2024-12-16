using System.Linq;
using DnsZone.Records;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            ClassicAssert.AreEqual(1, zone.Records.Count);

            ClassicAssert.IsAssignableFrom<CNameResourceRecord>(zone.Records.First());

            var record = (CNameResourceRecord)zone.Records.First();
            ClassicAssert.AreEqual("autodiscover.example.com", record.Name);
            ClassicAssert.AreEqual("IN", record.Class);
            ClassicAssert.AreEqual(ResourceRecordType.CNAME, record.Type);
            ClassicAssert.AreEqual("autodiscover.test.com", record.CanonicalName);
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
            ClassicAssert.AreEqual(";CNAME records\nautodiscover.example.com.\tIN\t\tCNAME\tautodiscover.test.com.\t\n\n", sOutput);
        }
    }
}