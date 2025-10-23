using System.Linq;
using DnsZone.Records;
using NUnit.Framework;

namespace DnsZone.Tests.Records {
    [TestFixture]
    public class NsResourceRecordTests {

        [Test]
        public void NsRecordParseTest() {
            const string str = @"
; zone fragment example.com
; mail servers in the same zone
; will support incoming email with addresses of the format 
; user@example.com
$TTL 2d ; zone default = 2 days or 172800 seconds
$ORIGIN example.com.
@               IN      NS     ns1.example.net.
@               IN      NS     ns1.example.org.";
            var zone = DnsZoneFile.Parse(str);
            Assert.That(zone.Records.Count, Is.EqualTo(2));

            Assert.That(zone.Records.First(), Is.AssignableTo<NsResourceRecord>());

            var record = (NsResourceRecord)zone.Records.First();
            Assert.That(record.Name, Is.EqualTo("example.com"));
            Assert.That(record.Class, Is.EqualTo("IN"));
            Assert.That(record.Type, Is.EqualTo(ResourceRecordType.NS));
            Assert.That(record.NameServer, Is.EqualTo("ns1.example.net"));
        }

    }
}