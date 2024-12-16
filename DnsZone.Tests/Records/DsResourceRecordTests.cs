using System.Linq;
using DnsZone.Records;
using NUnit.Framework;

namespace DnsZone.Tests.Records {
    [TestFixture]
    public class DsResourceRecordTests {

        [Test]
        public void ParseTest() {
            const string str = @"
; zone fragment example.com
; mail servers in the same zone
; will support incoming email with addresses of the format 
; user@example.com
$TTL 2d ; zone default = 2 days or 172800 seconds
$ORIGIN example.com.
example.com.        IN DS 62910 7 1 1D6AC75083F3CEC31861993E325E0EEC7E97D1DD";
            var zone = DnsZoneFile.Parse(str);
            Assert.AreEqual(1, zone.Records.Count);

            Assert.IsAssignableFrom<DsResourceRecord>(zone.Records.First());

            var record = (DsResourceRecord)zone.Records.First();
            Assert.AreEqual("example.com", record.Name);
            Assert.AreEqual("IN", record.Class);
            Assert.AreEqual(ResourceRecordType.DS, record.Type);
            Assert.AreEqual("62910", record.KeyTag);
            Assert.AreEqual(7, record.Algorithm);
            Assert.AreEqual(1, record.HashType);
            Assert.AreEqual("1D6AC75083F3CEC31861993E325E0EEC7E97D1DD", record.Hash);
        }
        
        [Test]
        public void OutputTest() {
            var zone = new DnsZoneFile();

            var record = new DsResourceRecord {
                Name = "example.com",
                Class = "IN",
                KeyTag = "52037",
                Algorithm = 1,
                HashType = 1,
                Hash = "378929E92D7DA04267EE87E802D75C5CA1B5D280"
            };
            
            zone.Records.Add(record);
            var sOutput = zone.ToString();
            Assert.AreEqual(";DS records\r\nexample.com.\tIN\t\tDS\t52037\t1\t1\t378929E92D7DA04267EE87E802D75C5CA1B5D280\t\r\n\r\n", sOutput);
        }
    }
}
