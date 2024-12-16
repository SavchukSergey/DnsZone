using System.Linq;
using DnsZone.Records;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            ClassicAssert.AreEqual(2, zone.Records.Count);

            ClassicAssert.IsAssignableFrom<MxResourceRecord>(zone.Records.First());

            var record = (MxResourceRecord)zone.Records.First();
            ClassicAssert.AreEqual("example.com", record.Name);
            ClassicAssert.AreEqual("IN", record.Class);
            ClassicAssert.AreEqual(ResourceRecordType.MX, record.Type);
            ClassicAssert.AreEqual(10, record.Preference);
            ClassicAssert.AreEqual("mail.foo.com", record.Exchange);
        }

    }
}
