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
            Assert.That(zone.Records.Count, Is.EqualTo(2));

            Assert.That(zone.Records.First(), Is.AssignableTo<NaptrResourceRecord>());

            var record = (NaptrResourceRecord)zone.Records.First();
            Assert.That(record.Name, Is.EqualTo("service.example.com"));
            Assert.That(record.Class, Is.EqualTo("IN"));
            Assert.That(record.Type, Is.EqualTo(ResourceRecordType.NAPTR));
            Assert.That(record.Order, Is.EqualTo(100));
            Assert.That(record.Preference, Is.EqualTo(10));
            Assert.That(record.Flags, Is.EqualTo("A"));
            Assert.That(record.Services, Is.EqualTo(""));
            Assert.That(record.Regexp, Is.EqualTo(""));
            Assert.That(record.Replacement, Is.EqualTo("prodserver.example.com"));
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
            Assert.That(sOutput, Is.EqualTo(";NAPTR records\nexample.com.\tIN\t\tNAPTR\t100\t10\t\"A\"\t\"\"\t\"\"\tprodserver.example.com\t\n\n"));
        }
    }
}
