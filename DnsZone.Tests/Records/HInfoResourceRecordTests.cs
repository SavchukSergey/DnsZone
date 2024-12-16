using System.Linq;
using DnsZone.Records;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace DnsZone.Tests.Records {
    [TestFixture]
    public class HInfoResourceRecordTests {

        [Test]
        public void ParseTest() {
            const string str = @"
; zone fragment example.com
; mail servers in the same zone
; will support incoming email with addresses of the format 
; user@example.com
$TTL 2d ; zone default = 2 days or 172800 seconds
$ORIGIN example.com.
example.com. IN HINFO ""INTEL-386"" ""Windows""";
            var zone = DnsZoneFile.Parse(str);
            ClassicAssert.AreEqual(1, zone.Records.Count);

            ClassicAssert.IsAssignableFrom<HInfoResourceRecord>(zone.Records.First());

            var record = (HInfoResourceRecord)zone.Records.First();
            ClassicAssert.AreEqual("example.com", record.Name);
            ClassicAssert.AreEqual("IN", record.Class);
            ClassicAssert.AreEqual(ResourceRecordType.HINFO, record.Type);
            ClassicAssert.AreEqual("INTEL-386", record.Cpu);
            ClassicAssert.AreEqual("Windows", record.Os);
        }
        
        [Test]
        public void OutputTest() {
            var zone = new DnsZoneFile();

            var record = new HInfoResourceRecord {
                Name = "example.com",
                Class = "IN",
                Cpu = "INTEL-386",
                Os = "Windows",
            };
            
            zone.Records.Add(record);
            var sOutput = zone.ToString();
            ClassicAssert.AreEqual(";HINFO records\nexample.com.\tIN\t\tHINFO\t\"INTEL-386\"\t\"Windows\"\t\n\n", sOutput);
        }
    }
}
