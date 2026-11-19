using System;
using System.Linq;
using DnsZone.Records;
using DnsZone.Tokens;
using NUnit.Framework;

namespace DnsZone.Tests.Records {
    [TestFixture]
    public class DnssecResourceRecordTests {

        // Examples from RFC 4034 sections 2.3, 3.3, 4.3, 5.4 and RFC 5155 appendix A
        private const string SIGNED_ZONE = @"
$TTL 1h
$ORIGIN example.com.
example.com. 86400 IN DNSKEY 256 3 5 ( AQPSKmynfzW4kyBv015MUG2DeIQ3
                                      Cbl+BBZH4b/0PY1kxkmvHjcZc8no
                                      kfzj31GajIQKY+5CptLr3buXA10h
                                      WqTkF7H6RfoRqXQeogmMHfpftf6z
                                      Mv1LyBUgia7za6ZEzOJBOztyvhjL
                                      742iU/TpPSEDhm2SNKLijfUppn1U
                                      aNvv4w==  )
host.example.com. 86400 IN RRSIG A 5 3 86400 20030322173103 (
                              20030220173103 2642 example.com.
                              oJB1W6WNGv+ldvQ3WDG0MQkg5IEhjRip8WTr
                              PYGv07h108dUKGMeDPKijVCHX3DDKdfb+v6o
                              B9wfuh3DTJXUAfI/M0zmO/zz8bW0Rznl8O3t
                              GNazPwQKkRN20XPXV6nwwfoXmJQbsLNrLfkG
                              J5D6fwFm8nN+6pBzeDQfsS3Ap3o= )
alfa.example.com. 86400 IN NSEC host.example.com. (
                                A MX RRSIG NSEC TYPE1234 )
dskey.example.com. 86400 IN DS 60485 5 1 ( 2BB183AF5F22588179A53B0A
                                           98631FAD1A292118 )
0p9mhaveqvm6t7vbl5lop2u3t2rp3tom IN NSEC3 1 1 12 aabbccdd (
                                2t7b4g4vsa5smi47k61mv5bv1a22bojr MX DNSKEY NS
                                SOA NSEC3PARAM RRSIG ) ; comment
@ IN NSEC3PARAM 1 0 12 aabbccdd
example.com. IN CDS 60485 5 1 2BB183AF5F22588179A53B0A98631FAD1A292118
example.com. IN CDNSKEY 257 3 13 mdsswUyr3DPW132mOi8V9xESWE8jTo0dxCjjnopKl+GqJxpVXckHAeF+KkxLbxILfDLUT0rAK9iUzy1L53eKGQ==
www IN A 192.0.2.1
";

        [Test]
        public void ParseDnsKeyTest() {
            var zone = DnsZoneFile.Parse(SIGNED_ZONE);
            var record = zone.Records.OfType<DnsKeyResourceRecord>().Single();
            Assert.That(record.Name, Is.EqualTo("example.com"));
            Assert.That(record.Type, Is.EqualTo(ResourceRecordType.DNSKEY));
            Assert.That(record.Ttl, Is.EqualTo(TimeSpan.FromDays(1)));
            Assert.That(record.Flags, Is.EqualTo(256));
            Assert.That(record.Protocol, Is.EqualTo(3));
            Assert.That(record.Algorithm, Is.EqualTo(5));
            Assert.That(record.PublicKey, Is.EqualTo("AQPSKmynfzW4kyBv015MUG2DeIQ3Cbl+BBZH4b/0PY1kxkmvHjcZc8nokfzj31GajIQKY+5CptLr3buXA10hWqTkF7H6RfoRqXQeogmMHfpftf6zMv1LyBUgia7za6ZEzOJBOztyvhjL742iU/TpPSEDhm2SNKLijfUppn1UaNvv4w=="));
        }

        [Test]
        public void ParseRrsigTest() {
            var zone = DnsZoneFile.Parse(SIGNED_ZONE);
            var record = zone.Records.OfType<RrsigResourceRecord>().Single();
            Assert.That(record.Name, Is.EqualTo("host.example.com"));
            Assert.That(record.Type, Is.EqualTo(ResourceRecordType.RRSIG));
            Assert.That(record.TypeCovered, Is.EqualTo("A"));
            Assert.That(record.Algorithm, Is.EqualTo(5));
            Assert.That(record.Labels, Is.EqualTo(3));
            Assert.That(record.OriginalTtl, Is.EqualTo(TimeSpan.FromDays(1)));
            Assert.That(record.SignatureExpiration, Is.EqualTo(new DateTime(2003, 3, 22, 17, 31, 3, DateTimeKind.Utc)));
            Assert.That(record.SignatureExpiration.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(record.SignatureInception, Is.EqualTo(new DateTime(2003, 2, 20, 17, 31, 3, DateTimeKind.Utc)));
            Assert.That(record.KeyTag, Is.EqualTo(2642));
            Assert.That(record.SignerName, Is.EqualTo("example.com"));
            Assert.That(record.Signature, Does.StartWith("oJB1W6WNGv+ldvQ3WDG0MQkg5IEhjRip8WTrPYGv07h1"));
            Assert.That(record.Signature, Does.EndWith("J5D6fwFm8nN+6pBzeDQfsS3Ap3o="));
        }

        [Test]
        public void ParseNsecTest() {
            var zone = DnsZoneFile.Parse(SIGNED_ZONE);
            var record = zone.Records.OfType<NsecResourceRecord>().Single();
            Assert.That(record.Name, Is.EqualTo("alfa.example.com"));
            Assert.That(record.NextDomainName, Is.EqualTo("host.example.com"));
            Assert.That(record.Types, Is.EqualTo(new[] { "A", "MX", "RRSIG", "NSEC", "TYPE1234" }));
        }

        [Test]
        public void ParseSplitDsDigestTest() {
            var zone = DnsZoneFile.Parse(SIGNED_ZONE);
            var record = zone.Records.OfType<DsResourceRecord>().Single();
            Assert.That(record.Name, Is.EqualTo("dskey.example.com"));
            Assert.That(record.KeyTag, Is.EqualTo("60485"));
            Assert.That(record.Algorithm, Is.EqualTo(5));
            Assert.That(record.HashType, Is.EqualTo(1));
            Assert.That(record.Hash, Is.EqualTo("2BB183AF5F22588179A53B0A98631FAD1A292118"));
        }

        [Test]
        public void ParseNsec3Test() {
            var zone = DnsZoneFile.Parse(SIGNED_ZONE);
            var record = zone.Records.OfType<Nsec3ResourceRecord>().Single();
            Assert.That(record.Name, Is.EqualTo("0p9mhaveqvm6t7vbl5lop2u3t2rp3tom.example.com"));
            Assert.That(record.Ttl, Is.EqualTo(TimeSpan.FromHours(1)));
            Assert.That(record.HashAlgorithm, Is.EqualTo(1));
            Assert.That(record.Flags, Is.EqualTo(1));
            Assert.That(record.Iterations, Is.EqualTo(12));
            Assert.That(record.Salt, Is.EqualTo("aabbccdd"));
            Assert.That(record.NextHashedOwnerName, Is.EqualTo("2t7b4g4vsa5smi47k61mv5bv1a22bojr"));
            Assert.That(record.Types, Is.EqualTo(new[] { "MX", "DNSKEY", "NS", "SOA", "NSEC3PARAM", "RRSIG" }));
        }

        [Test]
        public void ParseNsec3ParamTest() {
            var zone = DnsZoneFile.Parse(SIGNED_ZONE);
            var record = zone.Records.OfType<Nsec3ParamResourceRecord>().Single();
            Assert.That(record.Name, Is.EqualTo("example.com"));
            Assert.That(record.HashAlgorithm, Is.EqualTo(1));
            Assert.That(record.Flags, Is.EqualTo(0));
            Assert.That(record.Iterations, Is.EqualTo(12));
            Assert.That(record.Salt, Is.EqualTo("aabbccdd"));
        }

        [Test]
        public void ParseCdsAndCDnsKeyTest() {
            var zone = DnsZoneFile.Parse(SIGNED_ZONE);
            var cds = zone.Records.OfType<CdsResourceRecord>().Single();
            Assert.That(cds.Type, Is.EqualTo(ResourceRecordType.CDS));
            Assert.That(cds.KeyTag, Is.EqualTo(60485));
            Assert.That(cds.Algorithm, Is.EqualTo(5));
            Assert.That(cds.HashType, Is.EqualTo(1));
            Assert.That(cds.Hash, Is.EqualTo("2BB183AF5F22588179A53B0A98631FAD1A292118"));

            var cdnskey = zone.Records.OfType<CDnsKeyResourceRecord>().Single();
            Assert.That(cdnskey.Type, Is.EqualTo(ResourceRecordType.CDNSKEY));
            Assert.That(cdnskey.Flags, Is.EqualTo(257));
            Assert.That(cdnskey.Protocol, Is.EqualTo(3));
            Assert.That(cdnskey.Algorithm, Is.EqualTo(13));
            Assert.That(cdnskey.PublicKey, Does.StartWith("mdsswUyr3DPW"));
        }

        [Test]
        public void SignedZoneDoesNotSwallowFollowingRecordsTest() {
            var zone = DnsZoneFile.Parse(SIGNED_ZONE);
            Assert.That(zone.Records.Count, Is.EqualTo(9));
            Assert.That(zone.Records.OfType<AResourceRecord>().Single().Name, Is.EqualTo("www.example.com"));
        }

        [Test]
        public void RoundTripTest() {
            var zone = DnsZoneFile.Parse(SIGNED_ZONE);
            var text = zone.ToString("example.com");
            var reparsed = DnsZoneFile.Parse(text);
            Assert.That(reparsed.ToString("example.com"), Is.EqualTo(text));
            Assert.That(reparsed.Records.Count, Is.EqualTo(zone.Records.Count));
        }

        [Test]
        public void RrsigEpochTimeTest() {
            var zone = DnsZoneFile.Parse("example.com. 3600 IN RRSIG SOA 13 2 3600 1700000000 1690000000 12345 example.com. c2ln");
            var record = zone.Records.OfType<RrsigResourceRecord>().Single();
            Assert.That(record.SignatureExpiration, Is.EqualTo(DateTimeOffset.FromUnixTimeSeconds(1700000000).UtcDateTime));
            Assert.That(record.SignatureInception, Is.EqualTo(DateTimeOffset.FromUnixTimeSeconds(1690000000).UtcDateTime));
        }

        [Test]
        public void OutputRrsigTest() {
            var zone = new DnsZoneFile();
            zone.Records.Add(new RrsigResourceRecord {
                Name = "host.example.com",
                Class = "IN",
                Ttl = TimeSpan.FromHours(1),
                TypeCovered = "A",
                Algorithm = 13,
                Labels = 3,
                OriginalTtl = TimeSpan.FromHours(1),
                SignatureExpiration = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                SignatureInception = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc),
                KeyTag = 2642,
                SignerName = "example.com",
                Signature = "c2lnbmF0dXJl"
            });
            Assert.That(zone.ToString(), Is.EqualTo(";RRSIG records\nhost.example.com.\tIN\t1h\tRRSIG\tA\t13\t3\t3600\t20261201000000\t20261101000000\t2642\texample.com.\tc2lnbmF0dXJl\t\n\n"));
        }

        [Test]
        public void OutputNsec3EmptySaltTest() {
            var zone = new DnsZoneFile();
            zone.Records.Add(new Nsec3ResourceRecord {
                Name = "abc.example.com",
                Class = "IN",
                HashAlgorithm = 1,
                Iterations = 0,
                NextHashedOwnerName = "2t7b4g4vsa5smi47k61mv5bv1a22bojr",
                Types = ["A", "RRSIG"]
            });
            zone.Records.Add(new Nsec3ParamResourceRecord {
                Name = "example.com",
                Class = "IN",
                HashAlgorithm = 1
            });
            Assert.That(zone.ToString(), Is.EqualTo(
                ";NSEC3 records\nabc.example.com.\tIN\t0\tNSEC3\t1\t0\t0\t-\t2t7b4g4vsa5smi47k61mv5bv1a22bojr\tA\tRRSIG\t\n\n" +
                ";NSEC3PARAM records\nexample.com.\tIN\t0\tNSEC3PARAM\t1\t0\t0\t-\t\n\n"));

            var nsec3 = DnsZoneFile.Parse(zone.ToString()).Records.OfType<Nsec3ResourceRecord>().Single();
            Assert.That(nsec3.Salt, Is.EqualTo(""));
        }

        [Test]
        public void NsecWithoutTypesTest() {
            var zone = DnsZoneFile.Parse("a.example.com. 3600 IN NSEC b.example.com.\nwww.example.com. 3600 IN A 192.0.2.1");
            Assert.That(zone.Records.OfType<NsecResourceRecord>().Single().Types, Is.Empty);
            Assert.That(zone.Records.Count, Is.EqualTo(2));
        }

        [Test]
        public void InvalidSignatureTimeTest() {
            Assert.Throws<TokenException>(() => DnsZoneFile.Parse("example.com. 3600 IN RRSIG SOA 13 2 3600 2026-12-01 1690000000 12345 example.com. c2ln"));
        }

        [Test]
        public void MissingPublicKeyTest() {
            Assert.Throws<TokenException>(() => DnsZoneFile.Parse("example.com. 3600 IN DNSKEY 256 3 13\nwww.example.com. 3600 IN A 192.0.2.1"));
        }

        [Test]
        public void InvalidSaltTest() {
            Assert.Throws<TokenException>(() => DnsZoneFile.Parse("example.com. 3600 IN NSEC3PARAM 1 0 0 xyz"));
        }

        [Test]
        public void U8OverflowTest() {
            var exc = Assert.Throws<TokenException>(() => DnsZoneFile.Parse("example.com. 3600 IN DNSKEY 256 3 256 AwEAAQ=="));
            Assert.That(exc!.Message, Does.Contain("U8 value expected, but '256' found"));
        }

        [Test]
        public void PublicKeyAtEndOfFileTest() {
            var exc = Assert.Throws<TokenException>(() => DnsZoneFile.Parse("example.com. 3600 IN DNSKEY 256 3 13"));
            Assert.That(exc!.Message, Does.Contain("public key expected, but end of file reached"));
        }

        [Test]
        public void InvalidTypeInBitmapTest() {
            var exc = Assert.Throws<TokenException>(() => DnsZoneFile.Parse("a.example.com. 3600 IN NSEC b.example.com. A 123"));
            Assert.That(exc!.Message, Does.Contain("resource record type expected, but '123' found"));
        }

        [TestCase("example.com. 3600 IN DNSKEY 256 3 13 !!not-base64!!")]
        [TestCase("example.com. 3600 IN DNSKEY 256 3 13 AwEAAQ=A")]
        [TestCase("example.com. 3600 IN DNSKEY 256 3 13 AwEAAQ== oops")]
        [TestCase("example.com. 3600 IN CDNSKEY 257 3 13 AwEAAQ== oops")]
        [TestCase("example.com. 3600 IN RRSIG SOA 13 2 3600 1700000000 1690000000 12345 example.com. c2ln oop")]
        public void InvalidBase64Test(string zone) {
            var exc = Assert.Throws<TokenException>(() => DnsZoneFile.Parse(zone));
            Assert.That(exc!.Message, Does.Contain("invalid Base64"));
        }

        [TestCase("example.com. 3600 IN DS 60485 5 1 zzzz")]
        [TestCase("example.com. 3600 IN DS 60485 5 1 ABC")]
        [TestCase("example.com. 3600 IN CDS 60485 5 1 2BB1 oops")]
        [TestCase(@"example.com. 3600 IN CDS 0 0 0 \# 0")]
        public void InvalidDigestTest(string zone) {
            var exc = Assert.Throws<TokenException>(() => DnsZoneFile.Parse(zone));
            Assert.That(exc!.Message, Does.Contain("invalid hex"));
        }

        [TestCase("zzzz!!zzzz")]
        [TestCase("2t7b4g4vsa5smi47k61mv5bv1a22bojrx")]
        [TestCase("2t7b4g4vsa5smi47k61mv5bv1a22bojw")]
        public void InvalidNextHashedOwnerNameTest(string hash) {
            var exc = Assert.Throws<TokenException>(() => DnsZoneFile.Parse($"example.com. 3600 IN NSEC3 1 0 12 - {hash} A"));
            Assert.That(exc!.Message, Does.Contain("invalid Base32hex"));
        }

        [Test]
        public void UpperCaseNextHashedOwnerNameTest() {
            var zone = DnsZoneFile.Parse("example.com. 3600 IN NSEC3 1 0 12 - 2T7B4G4VSA5SMI47K61MV5BV1A22BOJR A");
            Assert.That(zone.Records.OfType<Nsec3ResourceRecord>().Single().NextHashedOwnerName, Is.EqualTo("2T7B4G4VSA5SMI47K61MV5BV1A22BOJR"));
        }

        [Test]
        public void DeleteSignalsTest() {
            // RFC 8078 section 4
            var zone = DnsZoneFile.Parse("example.com. 3600 IN CDS 0 0 0 00\nexample.com. 3600 IN CDNSKEY 0 3 0 AA==");
            var cds = zone.Records.OfType<CdsResourceRecord>().Single();
            Assert.That(cds.KeyTag, Is.EqualTo(0));
            Assert.That(cds.Hash, Is.EqualTo("00"));
            var cdnskey = zone.Records.OfType<CDnsKeyResourceRecord>().Single();
            Assert.That(cdnskey.Flags, Is.EqualTo(0));
            Assert.That(cdnskey.PublicKey, Is.EqualTo("AA=="));
            Assert.That(DnsZoneFile.Parse(zone.ToString()).ToString(), Is.EqualTo(zone.ToString()));
        }

        [TestCase("example.com. 3600 IN CDS 65536 5 1 2BB1")]
        [TestCase("example.com. 3600 IN CDS 60485 256 1 2BB1")]
        [TestCase("example.com. 3600 IN CDS 60485 5 256 2BB1")]
        public void CdsFieldRangeTest(string zone) {
            Assert.Throws<TokenException>(() => DnsZoneFile.Parse(zone));
        }

        [Test]
        public void OutputCdsTest() {
            var zone = new DnsZoneFile();
            zone.Records.Add(new CdsResourceRecord {
                Name = "example.com",
                Class = "IN",
                KeyTag = 60485,
                Algorithm = 5,
                HashType = 1,
                Hash = "2BB183AF5F22588179A53B0A98631FAD1A292118"
            });
            Assert.That(zone.ToString(), Is.EqualTo(";CDS records\nexample.com.\tIN\t0\tCDS\t60485\t5\t1\t2BB183AF5F22588179A53B0A98631FAD1A292118\t\n\n"));
        }

        private static readonly ResourceRecord[] EmptyEncodedRecords = [
            new DnsKeyResourceRecord { Name = "example.com", Class = "IN", Flags = 256, Algorithm = 13 },
            new CDnsKeyResourceRecord { Name = "example.com", Class = "IN", Flags = 257, Algorithm = 13 },
            new RrsigResourceRecord { Name = "example.com", Class = "IN", TypeCovered = "A", SignerName = "example.com" },
            new DsResourceRecord { Name = "example.com", Class = "IN", KeyTag = "1" },
            new CdsResourceRecord { Name = "example.com", Class = "IN", KeyTag = 1 }
        ];

        [TestCaseSource(nameof(EmptyEncodedRecords))]
        public void OutputEmptyEncodedDataTest(ResourceRecord record) {
            var zone = new DnsZoneFile();
            zone.Records.Add(record);
            var exc = Assert.Throws<InvalidOperationException>(() => zone.ToString());
            Assert.That(exc!.Message, Does.Contain("record 'example.com' is empty"));
        }
    }
}
