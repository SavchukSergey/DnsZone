using System.Linq;
using DnsZone.Records;
using NUnit.Framework;

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
            Assert.That(zone.Records.Count, Is.EqualTo(1));

            Assert.That(zone.Records.First(), Is.AssignableTo<HInfoResourceRecord>());

            var record = (HInfoResourceRecord)zone.Records.First();
            Assert.That(record.Name, Is.EqualTo("example.com"));
            Assert.That(record.Class, Is.EqualTo("IN"));
            Assert.That(record.Type, Is.EqualTo(ResourceRecordType.HINFO));
            Assert.That(record.Cpu, Is.EqualTo("INTEL-386"));
            Assert.That(record.Os, Is.EqualTo("Windows"));
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
            Assert.That(sOutput, Is.EqualTo(";HINFO records\nexample.com.\tIN\t\tHINFO\t\"INTEL-386\"\t\"Windows\"\t\n\n"));
        }
    }
}
