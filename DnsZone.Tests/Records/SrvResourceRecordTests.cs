using System.Linq;
using DnsZone.Records;
using NUnit.Framework;

namespace DnsZone.Tests.Records {
    [TestFixture]
    public class SrvResourceRecordTests {

        [Test]
        public void ParseTest() {
            const string str = @"
$ORIGIN example.com.
$TTL 2d ; 172800 secs
; foobar - use old-slow-box or new-fast-box if either is
; available, make three quarters of the logins go to
; new-fast-box.
_foobar._tcp    SRV 0 1 9 old-slow-box.example.com.
                SRV 0 3 9 new-fast-box.example.com.
; if neither old-slow-box or new-fast-box is up, switch to
; using the sysdmin's box and the server
                SRV 1 0 9 sysadmins-box.example.com.
                SRV 1 0 9 server.example.com.
*._tcp          SRV  0 0 0 .
*._udp          SRV  0 0 0 .";
            var zone = DnsZoneFile.Parse(str);
            Assert.AreEqual(6, zone.Records.Count);

            Assert.IsAssignableFrom<SrvResourceRecord>(zone.Records.First());

            var record = (SrvResourceRecord)zone.Records.First();
            Assert.AreEqual("_foobar._tcp.example.com", record.Name);
            Assert.AreEqual(null, record.Class);
            Assert.AreEqual(ResourceRecordType.SRV, record.Type);
            Assert.AreEqual(0, record.Priority);
        }

        [Test]
        public void NameTest() {
            var record = new SrvResourceRecord {
                Name = "_foobar._tcp.example.com"
            };
            Assert.AreEqual("foobar", record.Service);
            Assert.AreEqual("tcp", record.Protocol);
            Assert.AreEqual("example.com", record.Host);

            record.Service = "test";
            Assert.AreEqual("_test._tcp.example.com", record.Name);

            record.Protocol = "udp";
            Assert.AreEqual("_test._udp.example.com", record.Name);

            record.Host = "vcap.me";
            Assert.AreEqual("_test._udp.vcap.me", record.Name);
        }
    }
}
