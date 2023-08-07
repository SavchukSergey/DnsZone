using System.Linq;
using DnsZone.Records;
using NUnit.Framework;

namespace DnsZone.Tests.Records
{
    [TestFixture]
    public class SshfpResourceRecordTests
    {

        [Test]
        public void ParseTest()
        {
            const string str = @"
; zone fragment example.com
; mail servers in the same zone
; will support incoming email with addresses of the format 
; user@example.com
$TTL 2d ; zone default = 2 days or 172800 seconds
$ORIGIN example.com.
sil.example.com. IN SSHFP 2 1 450c7d19d5da9a3a5b7c18992d1fbde15d8dad34";
            var zone = DnsZoneFile.Parse(str);
            Assert.AreEqual(1, zone.Records.Count);

            Assert.IsAssignableFrom<SSHFPResourceRecord>(zone.Records.First());

            var record = (SSHFPResourceRecord)zone.Records.First();
            Assert.AreEqual("sil.example.com", record.Name);
            Assert.AreEqual("IN", record.Class);
            Assert.AreEqual(ResourceRecordType.SSHFP, record.Type);
            Assert.AreEqual(2, record.AlgorithmNumber);
            Assert.AreEqual(1, record.FingerprintType);
            Assert.AreEqual("450c7d19d5da9a3a5b7c18992d1fbde15d8dad34", record.Fingerprint);
            Assert.AreEqual("2 1 450c7d19d5da9a3a5b7c18992d1fbde15d8dad34", record.ToString());
        }

        [Test]
        public void OutputTest()
        {
            var zone = new DnsZoneFile();

            var record = new SSHFPResourceRecord
            {
                Name = "sil.example.com",
                Class = "IN",
                AlgorithmNumber = 3,
                FingerprintType = 1,
                Fingerprint = @"450c7d19d5da9a3a5b7c19992d1fbde15d8dad34"
            };
            zone.Records.Add(record);
            var sOutput = zone.ToString();
            Assert.AreEqual(";SSHFP records\r\nsil.example.com.\tIN\t\tSSHFP\t3\t1\t450c7d19d5da9a3a5b7c19992d1fbde15d8dad34\t\r\n\r\n", sOutput);
            Assert.AreEqual("3 1 450c7d19d5da9a3a5b7c19992d1fbde15d8dad34", record.ToString());
        }
    }
}
