using System.Linq;
using DnsZone.Records;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            ClassicAssert.AreEqual(1, zone.Records.Count);

            ClassicAssert.IsAssignableFrom<DNameResourceRecord>(zone.Records.First());

            var record = (DNameResourceRecord)zone.Records.First();
            ClassicAssert.AreEqual("subdomain.example.com", record.Name);
            ClassicAssert.AreEqual("IN", record.Class);
            ClassicAssert.AreEqual(ResourceRecordType.DNAME, record.Type);
            ClassicAssert.AreEqual("host.example.org.", record.Target);
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
            ClassicAssert.AreEqual(";DNAME records\r\nsubdomain.example.com.\tIN\t\tDNAME\thost.example.org.\t\r\n\r\n", sOutput);
        }
    }
}
