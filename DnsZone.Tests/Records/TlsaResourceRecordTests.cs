using System.Linq;
using DnsZone.Records;
using NUnit.Framework;

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
            Assert.That(zone.Records.Count, Is.EqualTo(1));

            Assert.That(zone.Records.First(), Is.AssignableTo<TLSAResourceRecord>());

            var record = (TLSAResourceRecord)zone.Records.First();
            Assert.That(record.Name, Is.EqualTo("_443._tcp.keyserver.example.com"));
            Assert.That(record.Class, Is.EqualTo("IN"));
            Assert.That(record.Type, Is.EqualTo(ResourceRecordType.TLSA));
            Assert.That(record.CertificateUsage, Is.EqualTo(3));
            Assert.That(record.Selector, Is.EqualTo(1));
            Assert.That(record.MatchingType, Is.EqualTo(1));
            Assert.That(record.CertificateAssociationData, Is.EqualTo("e677073271638e936eb3846c7aacfd3d387b831aa953b7486dc8f6227798f70b"));
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
            Assert.That(sOutput, Is.EqualTo(";TLSA records\n_443._tcp.sil.example.com.\tIN\t\tTLSA\t3\t1\t2\tdd5f45b479cc19e697c33c676161df9e6466a9a728584b1c881e18222f9ada31\t\n\n"));
        }

        [Test]
        public void NameTest() {
            var record = new TLSAResourceRecord {
                Name = "_443._tcp.example.com"
            };
            Assert.That(record.Port, Is.EqualTo(443));
            Assert.That(record.Protocol, Is.EqualTo("tcp"));
            Assert.That(record.Host, Is.EqualTo("example.com"));

            record.Port = 444;
            Assert.That(record.Name, Is.EqualTo("_444._tcp.example.com"));

            record.Protocol = "udp";
            Assert.That(record.Name, Is.EqualTo("_444._udp.example.com"));

            record.Host = "vcap.me";
            Assert.That(record.Name, Is.EqualTo("_444._udp.vcap.me"));
        }
    }
}
