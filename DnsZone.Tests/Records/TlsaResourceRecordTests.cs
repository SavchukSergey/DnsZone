using System.Linq;
using DnsZone.Records;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace DnsZone.Tests.Records {
    [TestFixture]
    public class TlsaResourceRecordTests {

        [Test]
        public void ParseTest() {
            const string str = @"
; zone fragment example.com
; mail servers in the same zone
; will support incoming email with addresses of the format 
; user@example.com
$TTL 2d ; zone default = 2 days or 172800 seconds
$ORIGIN example.com.
_443._tcp.keyserver.example.com. IN TLSA 3 1 1 e677073271638e936eb3846c7aacfd3d387b831aa953b7486dc8f6227798f70b";
            var zone = DnsZoneFile.Parse(str);
            ClassicAssert.AreEqual(1, zone.Records.Count);

            ClassicAssert.IsAssignableFrom<TLSAResourceRecord>(zone.Records.First());

            var record = (TLSAResourceRecord)zone.Records.First();
            ClassicAssert.AreEqual("_443._tcp.keyserver.example.com", record.Name);
            ClassicAssert.AreEqual("IN", record.Class);
            ClassicAssert.AreEqual(ResourceRecordType.TLSA, record.Type);
            ClassicAssert.AreEqual(3, record.CertificateUsage);
            ClassicAssert.AreEqual(1, record.Selector);
            ClassicAssert.AreEqual(1, record.MatchingType);
            ClassicAssert.AreEqual("e677073271638e936eb3846c7aacfd3d387b831aa953b7486dc8f6227798f70b", record.CertificateAssociationData);
        }

        [Test]
        public void OutputTest() {
            var zone = new DnsZoneFile();

            var record = new TLSAResourceRecord {
                Name = "_443._tcp.sil.example.com",
                Class = "IN",
                CertificateUsage = 3,
                Selector = 1,
                MatchingType = 2,
                CertificateAssociationData = @"dd5f45b479cc19e697c33c676161df9e6466a9a728584b1c881e18222f9ada31"
            };
            zone.Records.Add(record);
            var sOutput = zone.ToString();
            ClassicAssert.AreEqual(";TLSA records\r\n_443._tcp.sil.example.com.\tIN\t\tTLSA\t3\t1\t2\tdd5f45b479cc19e697c33c676161df9e6466a9a728584b1c881e18222f9ada31\t\r\n\r\n", sOutput);
        }

        [Test]
        public void NameTest() {
            var record = new TLSAResourceRecord {
                Name = "_443._tcp.example.com"
            };
            ClassicAssert.AreEqual(443, record.Port);
            ClassicAssert.AreEqual("tcp", record.Protocol);
            ClassicAssert.AreEqual("example.com", record.Host);

            record.Port = 444;
            ClassicAssert.AreEqual("_444._tcp.example.com", record.Name);

            record.Protocol = "udp";
            ClassicAssert.AreEqual("_444._udp.example.com", record.Name);

            record.Host = "vcap.me";
            ClassicAssert.AreEqual("_444._udp.vcap.me", record.Name);
        }
    }
}
