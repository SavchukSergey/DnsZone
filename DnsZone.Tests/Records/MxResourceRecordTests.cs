using System.Linq;
using DnsZone.Records;
using NUnit.Framework;

namespace DnsZone.Tests.Records {
    [TestFixture]
    public class MxResourceRecordTests {

        [Test]
        public void MxRecordParseTest() {
            const string str = @"
; zone fragment example.com
; mail servers in the same zone
; will support incoming email with addresses of the format 
; user@example.com
$TTL 2d ; zone default = 2 days or 172800 seconds
$ORIGIN example.com.
@               IN     MX     10  mail.foo.com.
@               IN     MX     20  mail2.foo.com.";
            var zone = DnsZoneFile.Parse(str);
            Assert.That(zone.Records.Count, Is.EqualTo(2));

            Assert.That(zone.Records.First(), Is.AssignableTo<MxResourceRecord>());

            var record = (MxResourceRecord)zone.Records.First();
            Assert.That(record.Name, Is.EqualTo("example.com"));
            Assert.That(record.Class, Is.EqualTo("IN"));
            Assert.That(record.Type, Is.EqualTo(ResourceRecordType.MX));
            Assert.That(record.Preference, Is.EqualTo(10));
            Assert.That(record.Exchange, Is.EqualTo("mail.foo.com"));
        }

    }
}
