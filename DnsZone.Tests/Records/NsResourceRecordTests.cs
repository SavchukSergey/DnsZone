using System.Linq;
using DnsZone.Records;
using NUnit.Framework;

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
            Assert.AreEqual(2, zone.Records.Count);

            Assert.IsAssignableFrom<NsResourceRecord>(zone.Records.First());

            var record = (NsResourceRecord)zone.Records.First();
            Assert.AreEqual("example.com", record.Name);
            Assert.AreEqual("IN", record.Class);
            Assert.AreEqual(ResourceRecordType.NS, record.Type);
            Assert.AreEqual("ns1.example.net", record.NameServer);
        }

    }
}