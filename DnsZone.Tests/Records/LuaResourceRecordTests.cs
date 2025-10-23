using System.Linq;
using DnsZone.Records;
using NUnit.Framework;

namespace DnsZone.Tests.Records {
    [TestFixture]
    public class LuaResourceRecordTests {

        [Test]
        public void ParseTest() {
            const string str = @"
; zone fragment example.com
; mail servers in the same zone
; will support incoming email with addresses of the format 
; user@example.com
$TTL 2d ; zone default = 2 days or 172800 seconds
$ORIGIN example.com.
www IN	LUA A ""ifportup(443, {'192.0.2.1', '192.0.2.2'})""";
            var zone = DnsZoneFile.Parse(str);
            Assert.That(zone.Records.Count, Is.EqualTo(1));

            Assert.That(zone.Records.First(), Is.AssignableTo<LuaResourceRecord>());

            var record = (LuaResourceRecord)zone.Records.First();
            Assert.That(record.Name, Is.EqualTo("www.example.com"));
            Assert.That(record.Class, Is.EqualTo("IN"));
            Assert.That(record.Type, Is.EqualTo(ResourceRecordType.LUA));
            Assert.That(record.TargetType, Is.EqualTo("A"));
            Assert.That(record.Script, Is.EqualTo("ifportup(443, {'192.0.2.1', '192.0.2.2'})"));
        }
        
        [Test]
        public void OutputTest() {
            var zone = new DnsZoneFile();

            var record = new LuaResourceRecord {
                Name = "example.com",
                Class = "IN",
                TargetType = "A",
                Script = "ifportup(443, {'192.0.2.1', '192.0.2.2'})",
            };
            
            zone.Records.Add(record);
            var sOutput = zone.ToString();
            Assert.That(sOutput, Is.EqualTo(";LUA records\nexample.com.\tIN\t\tLUA\tA\t\"ifportup(443, {'192.0.2.1', '192.0.2.2'})\"\t\n\n"));
        }
    }
}
