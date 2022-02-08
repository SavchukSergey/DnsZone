using System;
using System.Linq;
using DnsZone.Records;
using NUnit.Framework;

namespace DnsZone.Tests.Records {
    [TestFixture]
    public class SoaResourceRecordTests {

        [Test]
        public void SoaRecordParseTest() {
            const string str = @"
; zone fragment example.com
; mail servers in the same zone
; will support incoming email with addresses of the format 
; user@example.com
$TTL 2d ; zone default = 2 days or 172800 seconds
$ORIGIN example.com.
@   3600 IN SOA master.example.com. hostmaster.example.com. (
    2014031700 ; serial
    3600       ; refresh
    1800       ; retry
    604800     ; expire
    600 )      ; negatives caching, prev. minimum";
            var zone = DnsZoneFile.Parse(str);
            Assert.AreEqual(1, zone.Records.Count);

            Assert.IsAssignableFrom<SoaResourceRecord>(zone.Records.First());

            var record = (SoaResourceRecord)zone.Records.First();
            Assert.AreEqual("example.com", record.Name);
            Assert.AreEqual("IN", record.Class);
            Assert.AreEqual(ResourceRecordType.SOA, record.Type);
            Assert.AreEqual("master.example.com", record.NameServer);
            Assert.AreEqual("hostmaster.example.com.", record.ResponsibleEmail);
            Assert.AreEqual("2014031700", record.SerialNumber);
            Assert.AreEqual(TimeSpan.FromSeconds(3600), record.Refresh);
            Assert.AreEqual(TimeSpan.FromSeconds(1800), record.Retry);
            Assert.AreEqual(TimeSpan.FromSeconds(604800), record.Expiry);
            Assert.AreEqual(TimeSpan.FromSeconds(600), record.Minimum);
            Assert.AreEqual("master.example.com hostmaster.example.com. 2014031700 3600 1800 604800 600", record.ToString());
        }

    }
}
