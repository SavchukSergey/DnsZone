using System.Linq;
using DnsZone.Records;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            ClassicAssert.AreEqual(1, zone.Records.Count);

            ClassicAssert.IsAssignableFrom<DsResourceRecord>(zone.Records.First());

            var record = (DsResourceRecord)zone.Records.First();
            ClassicAssert.AreEqual("example.com", record.Name);
            ClassicAssert.AreEqual("IN", record.Class);
            ClassicAssert.AreEqual(ResourceRecordType.DS, record.Type);
            ClassicAssert.AreEqual("62910", record.KeyTag);
            ClassicAssert.AreEqual(7, record.Algorithm);
            ClassicAssert.AreEqual(1, record.HashType);
            ClassicAssert.AreEqual("1D6AC75083F3CEC31861993E325E0EEC7E97D1DD", record.Hash);
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
            ClassicAssert.AreEqual(";DS records\nexample.com.\tIN\t\tDS\t52037\t1\t1\t378929E92D7DA04267EE87E802D75C5CA1B5D280\t\n\n", sOutput);
        }
    }
}
