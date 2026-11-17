using System;
using System.IO;
using System.Linq;
using System.Net;
using DnsZone.IO;
using DnsZone.Records;
using DnsZone.Tokens;
using NUnit.Framework;

namespace DnsZone.Tests {
    [TestFixture]
    public class RegressionTests {

        private static DnsZoneFile RoundTrip(DnsZoneFile zone, string? origin = null) {
            return DnsZoneFile.Parse(zone.ToString(origin));
        }

        private EmbeddedDnsSource Embedded(string startFile) {
            return new EmbeddedDnsSource(GetType().Assembly, GetType().Namespace + ".Samples", startFile);
        }

        #region #13 serializer round-trip

        [Test]
        public void NaptrReplacementRoundTrip() {
            var zone = DnsZoneFile.Parse("$ORIGIN example.com.\n$TTL 1h\n@ IN NAPTR 100 10 \"S\" \"SIP+D2U\" \"\" _sip._udp.example.com.\n");
            var record = RoundTrip(zone).Records.OfType<NaptrResourceRecord>().Single();
            Assert.That(record.Replacement, Is.EqualTo("_sip._udp.example.com"));
        }

        [Test]
        public void NaptrRootReplacementRoundTrip() {
            var zone = DnsZoneFile.Parse("$ORIGIN example.com.\n$TTL 1h\n@ IN NAPTR 100 10 \"U\" \"E2U+sip\" \"!^.*$!sip:info@example.com!\" .\n");
            Assert.That(zone.Records.OfType<NaptrResourceRecord>().Single().Replacement, Is.EqualTo("."));
            var record = RoundTrip(zone).Records.OfType<NaptrResourceRecord>().Single();
            Assert.That(record.Replacement, Is.EqualTo("."));
        }

        [Test]
        public void ZeroTtlRoundTrip() {
            var zone = DnsZoneFile.Parse("example.com. 0 IN A 1.2.3.4\n");
            Assert.That(RoundTrip(zone).Records.Single().Ttl, Is.EqualTo(TimeSpan.Zero));
            Assert.That(zone.ToString(null, TimeSpan.Zero), Does.StartWith("$TTL 0\n"));
        }

        [Test]
        public void OriginWithTrailingDotIsNotDuplicated() {
            var zone = DnsZoneFile.Parse("www.example.com. 1h IN A 1.2.3.4\n");
            var text = zone.ToString("example.com.");
            Assert.That(text, Does.StartWith("$ORIGIN example.com.\n"));
            Assert.That(DnsZoneFile.Parse(text).Records.Single().Name, Is.EqualTo("www.example.com"));
        }

        [Test]
        public void ControlCharactersAreEscaped() {
            var zone = DnsZoneFile.Parse("example.com. 1h IN TXT \"line1\\010line2\"\n");
            Assert.That(zone.Records.OfType<TxtResourceRecord>().Single().Content, Is.EqualTo("line1\nline2"));
            var text = zone.ToString();
            Assert.That(text, Does.Contain("\"line1\\010line2\""));
            Assert.That(DnsZoneFile.Parse(text).Records.OfType<TxtResourceRecord>().Single().Content, Is.EqualTo("line1\nline2"));
        }

        [Test]
        public void LongTxtIsSplitInto255OctetStrings() {
            var content = new string('a', 600);
            var zone = new DnsZoneFile();
            zone.Records.Add(new TxtResourceRecord { Name = "example.com", Class = "IN", Ttl = TimeSpan.FromHours(1), Content = content });
            var text = zone.ToString();
            Assert.That(text, Does.Contain("\"" + new string('a', 255) + "\"\t\"" + new string('a', 255) + "\"\t\"" + new string('a', 90) + "\""));
            Assert.That(DnsZoneFile.Parse(text).Records.OfType<TxtResourceRecord>().Single().Content, Is.EqualTo(content));
        }

        #endregion

        #region #14 $INCLUDE

        [Test]
        public void IncludeInheritsTtlAndOwner() {
            var zone = DnsZoneFile.Parse(Embedded("inherit_root.zone"));
            Assert.That(zone.Records.Count, Is.EqualTo(3));
            var aaaa = zone.Records.OfType<AaaaResourceRecord>().Single();
            Assert.That(aaaa.Name, Is.EqualTo("host.inherit.com"));
            Assert.That(aaaa.Ttl, Is.EqualTo(TimeSpan.FromHours(1)));
            Assert.That(aaaa.Class, Is.EqualTo("IN"));
            Assert.That(zone.Single<AResourceRecord>("sub.inherit.com").Ttl, Is.EqualTo(TimeSpan.FromHours(1)));
        }

        [Test]
        public void MissingIncludeThrowsTokenException() {
            var exc = Assert.Throws<TokenException>(() => DnsZoneFile.Parse(Embedded("missing_root.zone")));
            Assert.That(exc!.Message, Does.Contain("does_not_exist.zone"));
        }

        [Test]
        public void SelfIncludeDoesNotOverflowStack() {
            Assert.Throws<TokenException>(() => DnsZoneFile.Parse(Embedded("self_include.zone")));
        }

        [Test]
        public void IncludeWithOriginAtEof() {
            var zone = DnsZoneFile.Parse(Embedded("eof_root.zone"));
            Assert.That(zone.Single<AResourceRecord>("sub.eof.com").Address, Is.EqualTo(IPAddress.Parse("10.0.0.3")));
        }

        [Test]
        public void FileSourceResolvesIncludesRelativeToStartFile() {
            var dir = Path.Combine(Path.GetTempPath(), "dnszone-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try {
                var root = Path.Combine(dir, "root.zone");
                File.WriteAllText(root, "$ORIGIN files.com.\n$TTL 1h\n$INCLUDE child.zone\n");
                File.WriteAllText(Path.Combine(dir, "child.zone"), "www IN A 10.1.1.1\n");
                var zone = DnsZoneFile.Parse(new FileDnsSource(root));
                Assert.That(zone.Single<AResourceRecord>("www.files.com").Address, Is.EqualTo(IPAddress.Parse("10.1.1.1")));
                zone = DnsZoneFile.LoadFromFileAsync(root).GetAwaiter().GetResult();
                Assert.That(zone.Single<AResourceRecord>("www.files.com").Address, Is.EqualTo(IPAddress.Parse("10.1.1.1")));
            } finally {
                Directory.Delete(dir, true);
            }
        }

        #endregion

        #region #15 positioned errors

        [Test]
        public void TrailingWhitespaceAtEof() {
            var zone = DnsZoneFile.Parse("example.com. 3600 IN A 1.2.3.4\n   ");
            Assert.That(zone.Records.Count, Is.EqualTo(1));
        }

        [TestCase("$ORIGIN example.com.\n$TTL 1h\n@ IN A")]
        [TestCase("$ORIGIN example.com.\n$TTL 1h\n@ IN SOA ns admin 1 1d\n")]
        [TestCase("$ORIGIN example.com.\n$TTL 1h\n     IN A 1.2.3.4\n")]
        [TestCase("$ORIGIN example.com.\n$TTL 1h\n@ IN MX x mail\n")]
        [TestCase("$ORIGIN example.com.\n$TTL 1h\n@ IN A not-an-ip\n")]
        [TestCase("$ORIGIN example.com.\n@ IN A 1.2.3.4\n")]
        [TestCase("$ORIGIN example.com.\n$TTL 1h\n@ IN CAA 0\n")]
        [TestCase("$ORIGIN example.com.\n$TTL 1h\n@ IN FOO bar\n")]
        [TestCase("$INCLUDE")]
        [TestCase("$UNKNOWN value\n")]
        public void MalformedInputThrowsTokenException(string content) {
            var exc = Assert.Throws<TokenException>(() => DnsZoneFile.Parse(content));
            Assert.That(exc!.Token.Position.Line, Is.GreaterThan(0));
        }

        #endregion

        #region #16 tokenizer

        [Test]
        public void ParenthesesGluedToTokens() {
            var zone = DnsZoneFile.Parse("$ORIGIN ex.com.\n@ 1h IN SOA ns host (1 2 3 4 5)\n");
            var soa = zone.Records.OfType<SoaResourceRecord>().Single();
            Assert.That(soa.SerialNumber, Is.EqualTo("1"));
            Assert.That(soa.Minimum, Is.EqualTo(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void CommentGluedToToken() {
            var zone = DnsZoneFile.Parse("$ORIGIN ex.com.\n@ 1h IN NS ns.ex.com.;comment\n");
            Assert.That(zone.Records.OfType<NsResourceRecord>().Single().NameServer, Is.EqualTo("ns.ex.com"));
        }

        [Test]
        public void DollarInsideRecordIsNotDirective() {
            var zone = DnsZoneFile.Parse("$ORIGIN ex.com.\n@ 1h IN TXT $5.00\n");
            Assert.That(zone.Records.OfType<TxtResourceRecord>().Single().Content, Is.EqualTo("$5.00"));
        }

        [TestCase("hello\\ world", "hello world")]
        [TestCase("a\\;b", "a;b")]
        [TestCase("a\\065b", "aAb")]
        public void EscapesInUnquotedText(string raw, string expected) {
            var zone = DnsZoneFile.Parse("$ORIGIN ex.com.\n@ 1h IN TXT " + raw + "\n");
            Assert.That(zone.Records.OfType<TxtResourceRecord>().Single().Content, Is.EqualTo(expected));
        }

        [Test]
        public void EscapeCodeOutOfRange() {
            Assert.Throws<TokenException>(() => DnsZoneFile.Parse("$ORIGIN ex.com.\n@ 1h IN TXT \"x\\999y\"\n"));
        }

        [Test]
        public void EscapedDotIsNotAbsoluteName() {
            var zone = DnsZoneFile.Parse("$ORIGIN ex.com.\n@ 1h IN CNAME odd\\.\n");
            Assert.That(zone.Records.OfType<CNameResourceRecord>().Single().CanonicalName, Is.EqualTo("odd\\..ex.com"));
        }

        #endregion

        #region #17 TTL

        [Test]
        public void TtlOverflowIsRejected() {
            Assert.That(DnsZoneUtils.TryParseTimeSpan("99999999999", out _), Is.False);
            Assert.That(DnsZoneUtils.TryParseTimeSpan("2147483647", out var max), Is.True);
            Assert.That(max, Is.EqualTo(DnsZoneUtils.MaxTtl));
            Assert.That(DnsZoneUtils.TryParseTimeSpan("10000w", out _), Is.False);
            Assert.Throws<FormatException>(() => DnsZoneUtils.ParseTimeSpan("99999999999"));
        }

        [Test]
        public void TtlFallsBackToPreviousRecord() {
            var zone = DnsZoneFile.Parse("$ORIGIN ex.com.\n@ 1h IN A 1.2.3.4\nwww IN A 1.2.3.5\n");
            Assert.That(zone.Single<AResourceRecord>("www.ex.com").Ttl, Is.EqualTo(TimeSpan.FromHours(1)));
        }

        [Test]
        public void DefaultTtlTakesPrecedenceOverPreviousRecord() {
            var zone = DnsZoneFile.Parse("$ORIGIN ex.com.\n$TTL 5m\n@ 1h IN A 1.2.3.4\nwww IN A 1.2.3.5\n");
            Assert.That(zone.Single<AResourceRecord>("www.ex.com").Ttl, Is.EqualTo(TimeSpan.FromMinutes(5)));
        }

        #endregion

        #region #18 domain names

        [Test]
        public void ExplicitOriginWithTrailingDot() {
            var zone = DnsZoneFile.Parse("sub 600 IN A 1.2.3.4\n", "example.com.");
            Assert.That(zone.Records.Single().Name, Is.EqualTo("sub.example.com"));
            Assert.That(zone.Filter("example.com").Records.Count, Is.EqualTo(1));
            Assert.That(zone.Filter("example.com.").Records.Count, Is.EqualTo(1));
        }

        [Test]
        public void FilterIsCaseInsensitive() {
            var zone = DnsZoneFile.Parse("$ORIGIN Ex.COM.\nwww 1h IN A 1.2.3.4\n");
            Assert.That(zone.Filter("ex.com").Records.Count, Is.EqualTo(1));
        }

        [Test]
        public void CompressionIsCaseInsensitive() {
            var zone = DnsZoneFile.Parse("$ORIGIN Ex.COM.\nwww 1h IN A 1.2.3.4\n@ 1h IN NS ns\n");
            var text = zone.ToString("ex.com");
            Assert.That(text, Does.Contain("\nwww\tIN"));
            Assert.That(text, Does.Contain("\n@\tIN"));
        }

        [Test]
        public void RootDomainName() {
            var zone = DnsZoneFile.Parse("$ORIGIN ex.com.\n@ 1h IN NS .\n");
            Assert.That(zone.Records.OfType<NsResourceRecord>().Single().NameServer, Is.EqualTo("."));
            Assert.That(RoundTrip(zone).Records.OfType<NsResourceRecord>().Single().NameServer, Is.EqualTo("."));
        }

        [Test]
        public void RootOrigin() {
            var zone = DnsZoneFile.Parse("$ORIGIN .\n$TTL 1h\n@ IN NS a.root-servers.net.\ncom IN NS a.gtld-servers.net.\n");
            Assert.That(zone.Records[0].Name, Is.EqualTo("."));
            Assert.That(zone.Records[1].Name, Is.EqualTo("com"));
        }

        #endregion

        #region #19 SRV/TLSA names

        [Test]
        public void TlsaWithoutPrefix() {
            var zone = DnsZoneFile.Parse("host.example.com. 1h IN TLSA 3 1 1 ABCDEF\n");
            var record = zone.Records.OfType<TLSAResourceRecord>().Single();
            Assert.That(record.Name, Is.EqualTo("host.example.com"));
            Assert.That(record.Port, Is.Null);
            Assert.That(RoundTrip(zone).Records.Single().Name, Is.EqualTo("host.example.com"));
        }

        [TestCase("_tcp")]
        [TestCase("_x._tcp.host")]
        [TestCase("_tcp.host")]
        [TestCase("plain.host")]
        public void TlsaNamePreserved(string name) {
            var record = new TLSAResourceRecord { Name = name };
            Assert.That(record.Name, Is.EqualTo(name));
        }

        [TestCase("_tcp")]
        [TestCase("_sip.host")]
        [TestCase("plain.host")]
        [TestCase("_sip._tcp.host")]
        public void SrvNamePreserved(string name) {
            var record = new SrvResourceRecord { Name = name };
            Assert.That(record.Name, Is.EqualTo(name));
        }

        #endregion

        #region #20 unquoted character-strings

        [Test]
        public void UnquotedHinfo() {
            var zone = DnsZoneFile.Parse("$ORIGIN ex.com.\n@ 1h IN HINFO PC Unix\n");
            var record = zone.Records.OfType<HInfoResourceRecord>().Single();
            Assert.That(record.Cpu, Is.EqualTo("PC"));
            Assert.That(record.Os, Is.EqualTo("Unix"));
        }

        [Test]
        public void UnquotedNaptr() {
            var zone = DnsZoneFile.Parse("$ORIGIN ex.com.\n@ 1h IN NAPTR 100 10 S SIP+D2U \"\" _sip._udp\n");
            var record = zone.Records.OfType<NaptrResourceRecord>().Single();
            Assert.That(record.Flags, Is.EqualTo("S"));
            Assert.That(record.Services, Is.EqualTo("SIP+D2U"));
            Assert.That(record.Replacement, Is.EqualTo("_sip._udp.ex.com"));
        }

        [Test]
        public void UnquotedLua() {
            var zone = DnsZoneFile.Parse("$ORIGIN ex.com.\n@ 1h IN LUA A ifportup\n");
            Assert.That(zone.Records.OfType<LuaResourceRecord>().Single().Script, Is.EqualTo("ifportup"));
        }

        [Test]
        public void QuotedCaaTag() {
            var zone = DnsZoneFile.Parse("$ORIGIN ex.com.\n@ 1h IN CAA 0 \"issue\" \"letsencrypt.org\"\n");
            var record = zone.Records.OfType<CAAResourceRecord>().Single();
            Assert.That(record.Tag, Is.EqualTo("issue"));
            Assert.That(record.Value, Is.EqualTo("letsencrypt.org"));
        }

        #endregion

        #region review of fixes

        [TestCase("www.example.com.. 1h IN A 1.2.3.4\n")]
        [TestCase("www..example.com. 1h IN A 1.2.3.4\n")]
        [TestCase(".www 1h IN A 1.2.3.4\n")]
        public void EmptyLabelIsRejected(string content) {
            Assert.Throws<TokenException>(() => DnsZoneFile.Parse(content, "example.com"));
        }

        [Test]
        public void ProgrammaticNameWithTrailingDot() {
            var zone = new DnsZoneFile();
            zone.Records.Add(new AResourceRecord { Name = "example.com.", Class = "IN", Ttl = TimeSpan.FromHours(1), Address = IPAddress.Parse("1.2.3.4") });
            zone.Records.Add(new AResourceRecord { Name = "www.example.com.", Class = "IN", Ttl = TimeSpan.FromHours(1), Address = IPAddress.Parse("1.2.3.5") });
            Assert.That(zone.ToString(), Does.Contain("\nexample.com.\tIN").And.Not.Contain(".."));
            var text = zone.ToString("example.com");
            Assert.That(text, Does.Contain("\n@\tIN").And.Contain("\nwww\tIN"));
            Assert.That(zone.Filter("example.com").Records.Count, Is.EqualTo(2));
        }

        [TestCase("MD")]
        [TestCase("NULL")]
        [TestCase("WKS")]
        public void UnimplementedTypeThrowsTokenException(string type) {
            var exc = Assert.Throws<TokenException>(() => DnsZoneFile.Parse($"$ORIGIN ex.com.\n\n@ 1h IN {type} foo\n"));
            Assert.That(exc!.Token.Position.Line, Is.EqualTo(3));
        }

        [TestCase("100000w")]
        [TestCase("99999999999")]
        [TestCase("5q")]
        public void InvalidTtlDiagnostic(string ttl) {
            var exc = Assert.Throws<TokenException>(() => DnsZoneFile.Parse($"ex.com. {ttl} IN A 1.2.3.4\n"));
            Assert.That(exc!.Message, Does.Contain("ttl"));
        }

        [Test]
        public void RootOriginRoundTrip() {
            var zone = DnsZoneFile.Parse("$ORIGIN .\n$TTL 1h\n@ IN NS a.root-servers.net.\ncom IN NS a.gtld-servers.net.\n");
            foreach (var origin in new string?[] { null, "." }) {
                var reparsed = DnsZoneFile.Parse(zone.ToString(origin));
                Assert.That(reparsed.Records.Select(r => r.Name), Is.EqualTo(new[] { ".", "com" }));
                Assert.That(reparsed.Records.OfType<NsResourceRecord>().Select(r => r.NameServer), Is.EqualTo(new[] { "a.root-servers.net", "a.gtld-servers.net" }));
            }
        }

        [Test]
        public void CrLfInput() {
            var zone = DnsZoneFile.Parse("$ORIGIN ex.com.\r\n$TTL 1h\r\n@ IN SOA ns host (\r\n 1 2 3 4 5 )\r\nwww IN A 1.2.3.4\r\n");
            Assert.That(zone.Records.Count, Is.EqualTo(2));
            Assert.That(zone.Single<AResourceRecord>("www.ex.com").Address, Is.EqualTo(IPAddress.Parse("1.2.3.4")));
        }

        #endregion
    }
}
