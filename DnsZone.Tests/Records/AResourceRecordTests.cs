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
            Assert.AreEqual(3, zone.Records.Count);

            Assert.IsAssignableFrom<AResourceRecord>(zone.Records.First());

            var record = (AResourceRecord)zone.Records.First();
            Assert.AreEqual("mail.example.com", record.Name);
            Assert.AreEqual("IN", record.Class);
            Assert.AreEqual(ResourceRecordType.A, record.Type);
            Assert.AreEqual(IPAddress.Parse("192.0.2.3"), record.Address);
            Assert.AreEqual("192.0.2.3", record.ToString());
        }
        
        [Test]
        public void OutputTest() {
            var zone = new DnsZoneFile();

            var record = new AResourceRecord() {
                Name = "example.com",
                Class = "IN",
                Address = IPAddress.Parse("192.0.2.3"),
            };
            
            zone.Records.Add(record);
            var sOutput = zone.ToString();
            Assert.AreEqual(";A records\r\nexample.com.\tIN\t\tA\t192.0.2.3\t\r\n\r\n", sOutput);
        }
    }
}
