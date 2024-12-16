using System.Linq;
using DnsZone.Records;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            ClassicAssert.AreEqual(1, zone.Records.Count);

            ClassicAssert.IsAssignableFrom<LuaResourceRecord>(zone.Records.First());

            var record = (LuaResourceRecord)zone.Records.First();
            ClassicAssert.AreEqual("www.example.com", record.Name);
            ClassicAssert.AreEqual("IN", record.Class);
            ClassicAssert.AreEqual(ResourceRecordType.LUA, record.Type);
            ClassicAssert.AreEqual("A", record.TargetType);
            ClassicAssert.AreEqual("ifportup(443, {'192.0.2.1', '192.0.2.2'})", record.Script);
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
            ClassicAssert.AreEqual(";LUA records\nexample.com.\tIN\t\tLUA\tA\t\"ifportup(443, {'192.0.2.1', '192.0.2.2'})\"\t\n\n", sOutput);
        }
    }
}
