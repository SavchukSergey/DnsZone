using System.Linq;
using DnsZone.Records;
using NUnit.Framework;

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
            Assert.That(zone.Records.Count, Is.EqualTo(2));

            Assert.That(zone.Records.First(), Is.AssignableTo<TxtResourceRecord>());

            var record = (TxtResourceRecord)zone.Records.First();
            Assert.That(record.Name, Is.EqualTo("joe.example.com"));
            Assert.That(record.Class, Is.EqualTo("IN"));
            Assert.That(record.Type, Is.EqualTo(ResourceRecordType.TXT));
            Assert.That(record.Content, Is.EqualTo("Located in a black hole somewhere"));
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
            Assert.That(zone.Records.Count, Is.EqualTo(2));

            Assert.That(zone.Records.First(), Is.AssignableTo<TxtResourceRecord>());

            var record = (TxtResourceRecord)zone.Records.First();
            Assert.That(record.Name, Is.EqualTo("joe.example.com"));
            Assert.That(record.Class, Is.EqualTo("IN"));
            Assert.That(record.Type, Is.EqualTo(ResourceRecordType.TXT));
            Assert.That(record.Content, Is.EqualTo("LocatedInABlackHole"));
        }

    }
}
