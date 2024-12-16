using System.Linq;
using DnsZone.Records;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace DnsZone.Tests.Records {
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
            ClassicAssert.AreEqual(2, zone.Records.Count);

            ClassicAssert.IsAssignableFrom<TxtResourceRecord>(zone.Records.First());

            var record = (TxtResourceRecord)zone.Records.First();
            ClassicAssert.AreEqual("joe.example.com", record.Name);
            ClassicAssert.AreEqual("IN", record.Class);
            ClassicAssert.AreEqual(ResourceRecordType.TXT, record.Type);
            ClassicAssert.AreEqual("Located in a black hole somewhere", record.Content);
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
            ClassicAssert.That(zone.Records.Count, Is.EqualTo(2));

            ClassicAssert.IsAssignableFrom<TxtResourceRecord>(zone.Records.First());

            var record = (TxtResourceRecord)zone.Records.First();
            ClassicAssert.That(record.Name, Is.EqualTo("joe.example.com"));
            ClassicAssert.That(record.Class, Is.EqualTo("IN"));
            ClassicAssert.That(record.Type, Is.EqualTo(ResourceRecordType.TXT));
            ClassicAssert.That(record.Content, Is.EqualTo("LocatedInABlackHole"));
        }

    }
}
