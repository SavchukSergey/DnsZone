using System.Linq;
using DnsZone.Records;
using NUnit.Framework;

namespace DnsZone.Tests.Records {
    [TestFixture]
    public class SshfpResourceRecordTests {

        [Test]
        public void ParseTest() {
            const string str = @"
; zone fragment example.com
; mail servers in the same zone
; will support incoming email with addresses of the format 
; user@example.com
$TTL 2d ; zone default = 2 days or 172800 seconds
$ORIGIN example.com.
sil.example.com. IN SSHFP 2 1 450c7d19d5da9a3a5b7c18992d1fbde15d8dad34";
            var zone = DnsZoneFile.Parse(str);
            Assert.That(zone.Records.Count, Is.EqualTo(1));

            Assert.That(zone.Records.First(), Is.AssignableTo<SSHFPResourceRecord>());

            var record = (SSHFPResourceRecord)zone.Records.First();
            Assert.That(record.Name, Is.EqualTo("sil.example.com"));
            Assert.That(record.Class, Is.EqualTo("IN"));
            Assert.That(record.Type, Is.EqualTo(ResourceRecordType.SSHFP));
            Assert.That(record.AlgorithmNumber, Is.EqualTo(2));
            Assert.That(record.FingerprintType, Is.EqualTo(1));
            Assert.That(record.Fingerprint, Is.EqualTo("450c7d19d5da9a3a5b7c18992d1fbde15d8dad34"));
        }

        [Test]
        public void OutputTest() {
            var zone = new DnsZoneFile();

            var record = new SSHFPResourceRecord {
                Name = "sil.example.com",
                Class = "IN",
                AlgorithmNumber = 3,
                FingerprintType = 1,
                Fingerprint = @"450c7d19d5da9a3a5b7c19992d1fbde15d8dad34"
            };
            zone.Records.Add(record);
            var sOutput = zone.ToString();
            Assert.That(sOutput, Is.EqualTo(";SSHFP records\nsil.example.com.\tIN\t\tSSHFP\t3\t1\t450c7d19d5da9a3a5b7c19992d1fbde15d8dad34\t\n\n"));
        }
    }
}
