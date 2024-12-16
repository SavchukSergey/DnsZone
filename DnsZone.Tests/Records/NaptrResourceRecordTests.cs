using System.Linq;
using DnsZone.Records;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            ClassicAssert.AreEqual(2, zone.Records.Count);

            ClassicAssert.IsAssignableFrom<NaptrResourceRecord>(zone.Records.First());

            var record = (NaptrResourceRecord)zone.Records.First();
            ClassicAssert.AreEqual("service.example.com", record.Name);
            ClassicAssert.AreEqual("IN", record.Class);
            ClassicAssert.AreEqual(ResourceRecordType.NAPTR, record.Type);
            ClassicAssert.AreEqual(100, record.Order);
            ClassicAssert.AreEqual(10, record.Preference);
            ClassicAssert.AreEqual("A", record.Flags);
            ClassicAssert.AreEqual("", record.Services);
            ClassicAssert.AreEqual("", record.Regexp);
            ClassicAssert.AreEqual("prodserver.example.com", record.Replacement);
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
            ClassicAssert.AreEqual(";NAPTR records\nexample.com.\tIN\t\tNAPTR\t100\t10\t\"A\"\t\"\"\t\"\"\tprodserver.example.com\t\n\n", sOutput);
        }
    }
}
