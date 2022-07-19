using System.Linq;
using DnsZone.Records;
using NUnit.Framework;

namespace DnsZone.Tests {
    [TestFixture]
    public class TxtResourceRecordTests {

        [Test]
        public void ParseTest() {
            const string str = @"
$ORIGIN example.com.
; multiple quotes strings on a single line
; generates a single text string of 
; Located in a black hole somewhere
$TTL 1h                  ; default expiration time of all resource records without their own TTL value
joe        IN      TXT    ""Located in a black hole"" "" somewhere""
; multiple quoted strings on multiple lines
joe IN      TXT (""Located in a black hole""
                    "" somewhere over the rainbow"")
; generates a single text string of
; Located in a black hole somewhere over the rainbow";
            var zone = DnsZoneFile.Parse(str);
            Assert.AreEqual(2, zone.Records.Count);

            Assert.IsAssignableFrom<TxtResourceRecord>(zone.Records.First());

            var record = (TxtResourceRecord)zone.Records.First();
            Assert.AreEqual("joe.example.com", record.Name);
            Assert.AreEqual("IN", record.Class);
            Assert.AreEqual(ResourceRecordType.TXT, record.Type);
            Assert.AreEqual("Located in a black hole somewhere", record.Content);
        }

        [Test]
        public void NoQuotesParseTest() {
            const string str = @"
$ORIGIN example.com.
; multiple quotes strings on a single line
; generates a single text string of 
; Located in a black hole somewhere
$TTL 1h                  ; default expiration time of all resource records without their own TTL value
joe        IN      TXT    LocatedInABlackHole
; multiple quoted strings on multiple lines
joe IN      TXT (""Located in a black hole""
                    "" somewhere over the rainbow"")
; generates a single text string of
; Located in a black hole somewhere over the rainbow";
            var zone = DnsZoneFile.Parse(str);
            Assert.AreEqual(2, zone.Records.Count);

            Assert.IsAssignableFrom<TxtResourceRecord>(zone.Records.First());

            var record = (TxtResourceRecord)zone.Records.First();
            Assert.AreEqual("joe.example.com", record.Name);
            Assert.AreEqual("IN", record.Class);
            Assert.AreEqual(ResourceRecordType.TXT, record.Type);
            Assert.AreEqual("LocatedInABlackHole", record.Content);
        }

    }
}
