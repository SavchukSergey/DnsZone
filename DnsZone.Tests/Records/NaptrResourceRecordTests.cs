using System.Linq;
using DnsZone.Records;
using NUnit.Framework;

namespace DnsZone.Tests.Records {
    [TestFixture]
    public class NaptrResourceRecordTests {

        [Test]
        public void ParseTest() {
            const string str = @"
; zone fragment example.com
; mail servers in the same zone
; will support incoming email with addresses of the format 
; user@example.com
$TTL 2d ; zone default = 2 days or 172800 seconds
$ORIGIN example.com.
service.example.com.  IN  NAPTR  100  10  ""A"" """"  """"  prodserver.example.com.
new.example.com.  IN  NAPTR  100  10  ""A"" """"  ""!^.*$!prodserver.example.com!"".";
            var zone = DnsZoneFile.Parse(str);
            Assert.AreEqual(2, zone.Records.Count);

            Assert.IsAssignableFrom<NaptrResourceRecord>(zone.Records.First());

            var record = (NaptrResourceRecord)zone.Records.First();
            Assert.AreEqual("service.example.com", record.Name);
            Assert.AreEqual("IN", record.Class);
            Assert.AreEqual(ResourceRecordType.NAPTR, record.Type);
            Assert.AreEqual(100, record.Order);
            Assert.AreEqual(10, record.Preference);
            Assert.AreEqual("A", record.Flags);
            Assert.AreEqual("", record.Services);
            Assert.AreEqual("", record.Regexp);
            Assert.AreEqual("prodserver.example.com", record.Replacement);
        }
        
        [Test]
        public void OutputTest() {
            var zone = new DnsZoneFile();

            var record = new NaptrResourceRecord {
                Name = "example.com",
                Class = "IN",
                Order = 100,
                Preference = 10,
                Flags = "A",
                Services = "",
                Regexp= "",
                Replacement= "prodserver.example.com",
            };
            
            zone.Records.Add(record);
            var sOutput = zone.ToString();
            Assert.AreEqual(";NAPTR records\r\nexample.com.\tIN\t\tNAPTR\t100\t10\t\"A\"\t\"\"\t\"\"\tprodserver.example.com\t\r\n\r\n", sOutput);
        }
    }
}
