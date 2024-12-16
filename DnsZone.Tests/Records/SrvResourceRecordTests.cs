using System.Linq;
using DnsZone.Records;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            ClassicAssert.AreEqual(6, zone.Records.Count);

            ClassicAssert.IsAssignableFrom<SrvResourceRecord>(zone.Records.First());

            var record = (SrvResourceRecord)zone.Records.First();
            ClassicAssert.AreEqual("_foobar._tcp.example.com", record.Name);
            ClassicAssert.AreEqual(null, record.Class);
            ClassicAssert.AreEqual(ResourceRecordType.SRV, record.Type);
            ClassicAssert.AreEqual(0, record.Priority);
        }

        [Test]
        public void NameTest() {
            var record = new SrvResourceRecord {
                Name = "_foobar._tcp.example.com"
            };
            ClassicAssert.AreEqual("foobar", record.Service);
            ClassicAssert.AreEqual("tcp", record.Protocol);
            ClassicAssert.AreEqual("example.com", record.Host);

            record.Service = "test";
            ClassicAssert.AreEqual("_test._tcp.example.com", record.Name);

            record.Protocol = "udp";
            ClassicAssert.AreEqual("_test._udp.example.com", record.Name);

            record.Host = "vcap.me";
            ClassicAssert.AreEqual("_test._udp.vcap.me", record.Name);
        }
    }
}
