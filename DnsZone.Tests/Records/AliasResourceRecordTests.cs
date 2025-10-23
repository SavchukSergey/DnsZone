using System.Linq;
using DnsZone.Records;
using NUnit.Framework;

namespace DnsZone.Tests.Records {
    [TestFixture]
    public class AliasResourceRecordTests {

        [Test]
        public void ParseTest() {
            const string str = @"
; zone fragment example.com
; mail servers in the same zone
; will support incoming email with addresses of the format 
; user@example.com
$TTL 2d ; zone default = 2 days or 172800 seconds
$ORIGIN example.com.
test           IN  ALIAS     host.external.org          ; Alias for test.example.com
alias1         IN  ALIAS     new.origin.com             ; Alias for alias1.example.com";
            var zone = DnsZoneFile.Parse(str);
            Assert.That(zone.Records.Count, Is.EqualTo(2));

            Assert.That(zone.Records.First(), Is.AssignableTo<AliasResourceRecord>());

            var record = (AliasResourceRecord)zone.Records.First();
            Assert.That(record.Name, Is.EqualTo("test.example.com"));
            Assert.That(record.Class, Is.EqualTo("IN"));
            Assert.That(record.Type, Is.EqualTo(ResourceRecordType.ALIAS));
            Assert.That(record.Target, Is.EqualTo("host.external.org"));
        }
        
        [Test]
        public void OutputTest() {
            var zone = new DnsZoneFile();

            var record = new AliasResourceRecord {
                Name = "example.com",
                Class = "IN",
                Target = "host.external.org",
            };
            
            zone.Records.Add(record);
            var sOutput = zone.ToString();
            Assert.That(sOutput, Is.EqualTo(";ALIAS records\nexample.com.\tIN\t\tALIAS\thost.external.org\t\n\n"));
        }
    }
}
