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
            Assert.That(zone.Records.Count, Is.EqualTo(1));

            Assert.That(zone.Records.First(), Is.AssignableTo<DsResourceRecord>());

            var record = (DsResourceRecord)zone.Records.First();
            Assert.That(record.Name, Is.EqualTo("example.com"));
            Assert.That(record.Class, Is.EqualTo("IN"));
            Assert.That(record.Type, Is.EqualTo(ResourceRecordType.DS));
            Assert.That(record.KeyTag, Is.EqualTo("62910"));
            Assert.That(record.Algorithm, Is.EqualTo(7));
            Assert.That(record.HashType, Is.EqualTo(1));
            Assert.That(record.Hash, Is.EqualTo("1D6AC75083F3CEC31861993E325E0EEC7E97D1DD"));
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
            Assert.That(sOutput, Is.EqualTo(";DS records\nexample.com.\tIN\t\tDS\t52037\t1\t1\t378929E92D7DA04267EE87E802D75C5CA1B5D280\t\n\n"));
        }
    }
}
