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
            Assert.AreEqual(2, zone.Records.Count);

            Assert.IsAssignableFrom<MxResourceRecord>(zone.Records.First());

            var record = (MxResourceRecord)zone.Records.First();
            Assert.AreEqual("example.com", record.Name);
            Assert.AreEqual("IN", record.Class);
            Assert.AreEqual(ResourceRecordType.MX, record.Type);
            Assert.AreEqual(10, record.Preference);
            Assert.AreEqual("mail.foo.com", record.Exchange);
        }

    }
}
