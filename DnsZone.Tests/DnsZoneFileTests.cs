using System;
using System.Linq;
using System.Net;
using DnsZone.IO;
using DnsZone.Records;
using DnsZone.Tokens;
using NUnit.Framework;
using System.Threading.Tasks;

namespace DnsZone.Tests {
    [TestFixture]
    public class DnsZoneFileTests {

        [Test]
        public async Task ParseWhitespace() {
            var zone = await DnsZoneFile.LoadFromFileAsync(@"Samples/whitespace.com.zone", "whitespace.com");
            Assert.That(zone, Is.Not.Null);
        }

        [Test]
        public async Task Parse2Test() {
            var zone = await DnsZoneFile.LoadFromFileAsync(@"Samples/domain.com.zone", "domain.com");
            Assert.That(zone, Is.Not.Null);
        }

        [Test]
        public void EmptyTest() {
            const string str = @"
;just single comment
    ;white space and comments

";
            try {
                var zone = DnsZoneFile.Parse(str);
                Assert.That(zone.Records.Count, Is.EqualTo(0));
            } catch (TokenException exc) {
                Console.WriteLine(exc.Token.Position.GetLine());
                throw;
            }
        }

        [Test]
        public void ParseTest() {
            const string str = @"
$ORIGIN example.com.     ; designates the start of this zone file in the namespace
$TTL 1h                  ; default expiration time of all resource records without their own TTL value
example.com.  IN  SOA   ns.example.com. username.example.com. ( 2007120710 1d 2h 4w 1h )
example.com.  IN  NS    ns                    ; ns.example.com is a nameserver for example.com
example.com.  IN  NS    ns.somewhere.example. ; ns.somewhere.example is a backup nameserver for example.com
example.com.  IN  MX    10 mail.example.com.  ; mail.example.com is the mailserver for example.com
@             IN  MX    20 mail2.example.com. ; equivalent to above line, ""@"" represents zone origin
@             IN  MX    50 mail3              ; equivalent to above line, but using a relative host name
example.com.  IN  A     192.0.2.1             ; IPv4 address for example.com
              IN  AAAA  2001:db8:10::1        ; IPv6 address for example.com
ns            IN  A     192.0.2.2             ; IPv4 address for ns.example.com
              IN  AAAA  2001:db8:10::2        ; IPv6 address for ns.example.com
www           IN  CNAME example.com.          ; www.example.com is an alias for example.com
wwwtest       IN  CNAME www                   ; wwwtest.example.com is another alias for www.example.com
mail          IN  A     192.0.2.3             ; IPv4 address for mail.example.com
mail2         IN  A     192.0.2.4             ; IPv4 address for mail2.example.com
mail3         IN  A     192.0.2.5             ; IPv4 address for mail3.example.com";
            try {
                var zone = DnsZoneFile.Parse(str);
                Assert.That(zone.Records.OfType<SoaResourceRecord>().Count(), Is.EqualTo(1));
                Assert.That(zone.Records.OfType<NsResourceRecord>().Count(), Is.EqualTo(2));
                Assert.That(zone.Records.OfType<MxResourceRecord>().Count(), Is.EqualTo(3));
                Assert.That(zone.Records.OfType<AResourceRecord>().Count(), Is.EqualTo(5));
                Assert.That(zone.Records.OfType<CNameResourceRecord>().Count(), Is.EqualTo(2));
            } catch (TokenException exc) {
                Console.WriteLine(exc.Token.Position.GetLine());
                throw;
            }
        }

        [Test]
        public void SoaRecordParseTest() {
            const string str = @"
$TTL 1h                  ; default expiration time of all resource records without their own TTL value
example.com.    IN    SOA   ns.example.com. hostmaster.example.com. (
                              2003080800 ; sn = serial number
                              172800     ; ref = refresh = 2d
                              900        ; ret = update retry = 15m
                              1209600    ; ex = expiry = 2w
                              3600       ; nx = nxdomain ttl = 1h
                              )";
            var zone = DnsZoneFile.Parse(str);
            Assert.That(zone.Records.Count, Is.EqualTo(1));

            Assert.That(zone.Records.First(), Is.AssignableTo<SoaResourceRecord>());

            var record = (SoaResourceRecord)zone.Records.First();
            Assert.That(record.Name, Is.EqualTo("example.com"));
            Assert.That(record.Class, Is.EqualTo("IN"));
            Assert.That(record.Type, Is.EqualTo(ResourceRecordType.SOA));
            Assert.That(record.NameServer, Is.EqualTo("ns.example.com"));
            Assert.That(record.SerialNumber, Is.EqualTo("2003080800"));
            Assert.That(record.Refresh, Is.EqualTo(TimeSpan.FromDays(2)));
            Assert.That(record.Retry, Is.EqualTo(TimeSpan.FromMinutes(15)));
            Assert.That(record.Expiry, Is.EqualTo(TimeSpan.FromDays(14)));
            Assert.That(record.Minimum, Is.EqualTo(TimeSpan.FromHours(1)));
        }

        [Test]
        public void PtrRecordParseTest() {
            const string str = @"
$TTL 2d ; 172800 secs
$ORIGIN 23.168.192.IN-ADDR.ARPA.
2             IN      PTR     joe.example.com. ; FDQN
15            IN      PTR     www.example.com.
17            IN      PTR     bill.example.com.
74            IN      PTR     fred.example.com.";
            var zone = DnsZoneFile.Parse(str);
            Assert.That(zone.Records.Count, Is.EqualTo(4));

            Assert.That(zone.Records.First(), Is.AssignableTo<PtrResourceRecord>());

            var record = (PtrResourceRecord)zone.Records.First();
            Assert.That(record.Name, Is.EqualTo("2.23.168.192.IN-ADDR.ARPA"));
            Assert.That(record.Class, Is.EqualTo("IN"));
            Assert.That(record.Type, Is.EqualTo(ResourceRecordType.PTR));
            Assert.That(record.HostName, Is.EqualTo("joe.example.com"));
        }

        [Test]
        public void ExplicitOriginTest() {
            const string str = @"
; A Records
@	600	IN	A	184.168.221.14
sub	600	IN	A	184.168.221.15
";
            var zone = DnsZoneFile.Parse(str, "test.com");
            Assert.That(zone.Records.Count, Is.EqualTo(2));

            Assert.That(zone.Records.First(), Is.AssignableTo<AResourceRecord>());

            var rootRecord = (AResourceRecord)zone.Records.First();
            Assert.That(rootRecord.Name, Is.EqualTo("test.com"));
            var subRecord = (AResourceRecord)zone.Records.Last();
            Assert.That(subRecord.Name, Is.EqualTo("sub.test.com"));
        }

        [Test]
        public void MissingOriginTest() {
            const string str = @"
; A Records
@	600	IN	A	184.168.221.14
sub	600	IN	A	184.168.221.15
";
            Assert.Throws<TokenException>(() => DnsZoneFile.Parse(str));
        }


        [Test]
        public void FormatTest() {
            var zone = new DnsZoneFile();
            zone.Records.Add(new AResourceRecord {
                Name = "www.example.com",
                Class = "IN",
                Ttl = TimeSpan.FromMinutes(15),
                Address = IPAddress.Parse("127.0.0.1")
            });
            zone.Records.Add(new AResourceRecord {
                Name = "ftp.example.com",
                Class = "IN",
                Ttl = TimeSpan.FromMinutes(15),
                Address = IPAddress.Parse("127.0.0.1")
            });
            var val = zone.ToString(origin: null, defaultTtl: TimeSpan.FromSeconds(5));
            var TAB_CHAR = '\t';
            Assert.That(val, Is.EqualTo($@"$TTL 5
;A records
www.example.com.{TAB_CHAR}IN{TAB_CHAR}15m{TAB_CHAR}A{TAB_CHAR}127.0.0.1
ftp.example.com.{TAB_CHAR}IN{TAB_CHAR}15m{TAB_CHAR}A{TAB_CHAR}127.0.0.1

"));
        }

        [Test]
        public void FitlerTest() {
            var zone = new DnsZoneFile();
            zone.Records.Add(new AResourceRecord {
                Name = "www.example.com",
                Class = "IN",
                Ttl = TimeSpan.FromMinutes(15),
                Address = IPAddress.Parse("127.0.0.1")
            });
            zone.Records.Add(new AResourceRecord {
                Name = "ftp.example.com",
                Class = "IN",
                Ttl = TimeSpan.FromMinutes(15),
                Address = IPAddress.Parse("127.0.0.2")
            });
            zone.Records.Add(new AResourceRecord {
                Name = "example.com",
                Class = "IN",
                Ttl = TimeSpan.FromMinutes(15),
                Address = IPAddress.Parse("127.0.0.3")
            });
            zone.Records.Add(new AResourceRecord {
                Name = "test-example.com",
                Class = "IN",
                Ttl = TimeSpan.FromMinutes(15),
                Address = IPAddress.Parse("127.0.0.4")
            });
            zone.Records.Add(new AResourceRecord {
                Name = "example-test-example.com",
                Class = "IN",
                Ttl = TimeSpan.FromMinutes(15),
                Address = IPAddress.Parse("127.0.0.5")
            });

            var filtered = zone.Filter("example.com");
            Assert.That(filtered.Records.Count, Is.EqualTo(3));
            Assert.That(filtered.Records.OfType<AResourceRecord>().Any(item => Equals(item.Address, IPAddress.Parse("127.0.0.1"))), Is.True);
            Assert.That(filtered.Records.OfType<AResourceRecord>().Any(item => Equals(item.Address, IPAddress.Parse("127.0.0.2"))), Is.True);
            Assert.That(filtered.Records.OfType<AResourceRecord>().Any(item => Equals(item.Address, IPAddress.Parse("127.0.0.3"))), Is.True);


            filtered = zone.Filter("test-example.com");
            Assert.That(filtered.Records.Count, Is.EqualTo(1));
            Assert.That(filtered.Records.OfType<AResourceRecord>().Any(item => Equals(item.Address, IPAddress.Parse("127.0.0.4"))), Is.True);

            filtered = zone.Filter("example-test-example.com");
            Assert.That(filtered.Records.Count, Is.EqualTo(1));
            Assert.That(filtered.Records.OfType<AResourceRecord>().Any(item => Equals(item.Address, IPAddress.Parse("127.0.0.5"))), Is.True);
        }

        [Test]
        public void IncludeTest() {
            var embedded = new EmbeddedDnsSource(GetType().Assembly, GetType().Namespace + ".Samples", "root_com.zone");
            var zone = DnsZoneFile.Parse(embedded);
            Assert.That(zone.Records.Count, Is.EqualTo(2));

            var rootA = zone.Single<AResourceRecord>("root.com");
            Assert.That(rootA.Address, Is.EqualTo(IPAddress.Parse("192.168.0.1")));

            var wwwA = zone.Single<AResourceRecord>("www.root.com");
            Assert.That(wwwA.Address, Is.EqualTo(IPAddress.Parse("192.168.0.2")));
        }
    }
}
