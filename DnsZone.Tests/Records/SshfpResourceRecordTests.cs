using System.Linq;
using DnsZone.Records;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            ClassicAssert.AreEqual(1, zone.Records.Count);

            ClassicAssert.IsAssignableFrom<SSHFPResourceRecord>(zone.Records.First());

            var record = (SSHFPResourceRecord)zone.Records.First();
            ClassicAssert.AreEqual("sil.example.com", record.Name);
            ClassicAssert.AreEqual("IN", record.Class);
            ClassicAssert.AreEqual(ResourceRecordType.SSHFP, record.Type);
            ClassicAssert.AreEqual(2, record.AlgorithmNumber);
            ClassicAssert.AreEqual(1, record.FingerprintType);
            ClassicAssert.AreEqual("450c7d19d5da9a3a5b7c18992d1fbde15d8dad34", record.Fingerprint);
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
            ClassicAssert.AreEqual(";SSHFP records\r\nsil.example.com.\tIN\t\tSSHFP\t3\t1\t450c7d19d5da9a3a5b7c19992d1fbde15d8dad34\t\r\n\r\n", sOutput);
        }
    }
}
