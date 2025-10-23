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
            Assert.That(zone.Records.Count, Is.EqualTo(6));

            Assert.That(zone.Records.First(), Is.AssignableTo<SrvResourceRecord>());

            var record = (SrvResourceRecord)zone.Records.First();
            Assert.That(record.Name, Is.EqualTo("_foobar._tcp.example.com"));
            Assert.That(record.Class, Is.EqualTo(null));
            Assert.That(record.Type, Is.EqualTo(ResourceRecordType.SRV));
            Assert.That(record.Priority, Is.EqualTo(0));
        }

        [Test]
        public void NameTest() {
            var record = new SrvResourceRecord {
                Name = "_foobar._tcp.example.com"
            };
            Assert.That(record.Service, Is.EqualTo("foobar"));
            Assert.That(record.Protocol, Is.EqualTo("tcp"));
            Assert.That(record.Host, Is.EqualTo("example.com"));

            record.Service = "test";
            Assert.That(record.Name, Is.EqualTo("_test._tcp.example.com"));

            record.Protocol = "udp";
            Assert.That(record.Name, Is.EqualTo("_test._udp.example.com"));

            record.Host = "vcap.me";
            Assert.That(record.Name, Is.EqualTo("_test._udp.vcap.me"));
        }
    }
}
