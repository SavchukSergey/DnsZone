using System.Linq;
using System.Net;
using DnsZone.Records;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace DnsZone.Tests.Records {
    [TestFixture]
    public class AaaaResourceRecordTests {

        [Test]
        public void ParseTest() {
            const string str = @"
; zone fragment example.com
; mail servers in the same zone
; will support incoming email with addresses of the format
; user@example.com
$TTL 2d ; zone default = 2 days or 172800 seconds
$ORIGIN example.com.
example.com.  IN  AAAA  2001:db8:10::1        ; IPv6 address for example.com
ns            IN  AAAA  2001:db8:10::2        ; IPv6 address for ns.example.com
joe        IN      AAAA      2001:db8::3  ; joe & www = same ip
www        IN      AAAA      2001:db8::3
; functionally the same as the record above
www.example.com.   AAAA      2001:db8::3
fred  3600 IN      AAAA      2001:db8::4  ; ttl =3600 overrides $TTL default
ftp        IN      AAAA      2001:db8::5 ; round robin with next
           IN      AAAA      2001:db8::6
mail       IN      AAAA      2001:db8::7  ; mail = round robin
mail       IN      AAAA      2001:db8::32
mail       IN      AAAA      2001:db8::33
squat      IN      AAAA      2001:db8:0:0:1::13  ; address in another subnet";
            var zone = DnsZoneFile.Parse(str);
            Assert.That(zone.Records.Count, Is.EqualTo(12));

            ClassicAssert.IsAssignableFrom<AaaaResourceRecord>(zone.Records.First());

            var record = (AaaaResourceRecord)zone.Records.First();
            ClassicAssert.AreEqual("example.com", record.Name);
            ClassicAssert.AreEqual("IN", record.Class);
            ClassicAssert.AreEqual(ResourceRecordType.AAAA, record.Type);
            ClassicAssert.AreEqual(IPAddress.Parse("2001:db8:10::1"), record.Address);
        }

        [Test]
        public void OutputTest() {
            var zone = new DnsZoneFile();

            var record = new AaaaResourceRecord {
                Name = "example.com",
                Class = "IN",
                Address = IPAddress.Parse("2001:db8:10::1")
            };

            zone.Records.Add(record);
            var sOutput = zone.ToString();
            ClassicAssert.AreEqual(";AAAA records\nexample.com.\tIN\t\tAAAA\t2001:db8:10::1\t\n\n", sOutput);
        }
    }
}