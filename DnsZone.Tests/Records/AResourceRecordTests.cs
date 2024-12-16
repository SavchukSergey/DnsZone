using System.Linq;
using System.Net;
using DnsZone.Records;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            ClassicAssert.AreEqual(3, zone.Records.Count);

            ClassicAssert.IsAssignableFrom<AResourceRecord>(zone.Records.First());

            var record = (AResourceRecord)zone.Records.First();
            ClassicAssert.AreEqual("mail.example.com", record.Name);
            ClassicAssert.AreEqual("IN", record.Class);
            ClassicAssert.AreEqual(ResourceRecordType.A, record.Type);
            ClassicAssert.AreEqual(IPAddress.Parse("192.0.2.3"), record.Address);
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
            ClassicAssert.AreEqual(";A records\nexample.com.\tIN\t\tA\t192.0.2.3\t\n\n", sOutput);
        }
    }
}