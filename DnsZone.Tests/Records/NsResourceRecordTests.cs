using System.Linq;
using DnsZone.Records;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace DnsZone.Tests.Records
{
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
            ClassicAssert.AreEqual(2, zone.Records.Count);

            ClassicAssert.IsAssignableFrom<NsResourceRecord>(zone.Records.First());

            var record = (NsResourceRecord)zone.Records.First();
            ClassicAssert.AreEqual("example.com", record.Name);
            ClassicAssert.AreEqual("IN", record.Class);
            ClassicAssert.AreEqual(ResourceRecordType.NS, record.Type);
            ClassicAssert.AreEqual("ns1.example.net", record.NameServer);
        }

    }
}