using System;
using System.Linq;
using System.Net;
using DnsZone.IO;
using DnsZone.Records;
using DnsZone.Tokens;
using NUnit.Framework;
using System.Threading.Tasks;
using NUnit.Framework.Legacy;

namespace DnsZone.Tests {
    [TestFixture]
    public class DnsZoneFileTests {

        [Test]
        public async Task ParseWhitespace() {
            var zone = await DnsZoneFile.LoadFromFileAsync(@"Samples/whitespace.com.zone", "whitespace.com");
            ClassicAssert.IsNotNull(zone);
        }

        [Test]
        public async Task Parse2Test() {
            var zone = await DnsZoneFile.LoadFromFileAsync(@"Samples/domain.com.zone", "domain.com");
            ClassicAssert.IsNotNull(zone);
        }

        [Test]
        public void EmptyTest() {
            const string str = @"
;just single comment
    ;white space and comments

";
            try {
                var zone = DnsZoneFile.Parse(str);
                ClassicAssert.AreEqual(0, zone.Records.Count);
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
                ClassicAssert.AreEqual(1, zone.Records.OfType<SoaResourceRecord>().Count());
                ClassicAssert.AreEqual(2, zone.Records.OfType<NsResourceRecord>().Count());
                ClassicAssert.AreEqual(3, zone.Records.OfType<MxResourceRecord>().Count());
                ClassicAssert.AreEqual(5, zone.Records.OfType<AResourceRecord>().Count());
                ClassicAssert.AreEqual(2, zone.Records.OfType<CNameResourceRecord>().Count());
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
            ClassicAssert.AreEqual(1, zone.Records.Count);

            ClassicAssert.IsAssignableFrom<SoaResourceRecord>(zone.Records.First());

            var record = (SoaResourceRecord)zone.Records.First();
            ClassicAssert.AreEqual("example.com", record.Name);
            ClassicAssert.AreEqual("IN", record.Class);
            ClassicAssert.AreEqual(ResourceRecordType.SOA, record.Type);
            ClassicAssert.AreEqual("ns.example.com", record.NameServer);
            ClassicAssert.AreEqual("2003080800", record.SerialNumber);
            ClassicAssert.AreEqual(TimeSpan.FromDays(2), record.Refresh);
            ClassicAssert.AreEqual(TimeSpan.FromMinutes(15), record.Retry);
            ClassicAssert.AreEqual(TimeSpan.FromDays(14), record.Expiry);
            ClassicAssert.AreEqual(TimeSpan.FromHours(1), record.Minimum);
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
            ClassicAssert.AreEqual(4, zone.Records.Count);

            ClassicAssert.IsAssignableFrom<PtrResourceRecord>(zone.Records.First());

            var record = (PtrResourceRecord)zone.Records.First();
            ClassicAssert.AreEqual("2.23.168.192.IN-ADDR.ARPA", record.Name);
            ClassicAssert.AreEqual("IN", record.Class);
            ClassicAssert.AreEqual(ResourceRecordType.PTR, record.Type);
            ClassicAssert.AreEqual("joe.example.com", record.HostName);
        }

        [Test]
        public void ExplicitOriginTest() {
            const string str = @"
; A Records
@	600	IN	A	184.168.221.14
sub	600	IN	A	184.168.221.15
";
            var zone = DnsZoneFile.Parse(str, "test.com");
            ClassicAssert.AreEqual(2, zone.Records.Count);

            ClassicAssert.IsAssignableFrom<AResourceRecord>(zone.Records.First());

            var rootRecord = (AResourceRecord)zone.Records.First();
            ClassicAssert.AreEqual("test.com", rootRecord.Name);
            var subRecord = (AResourceRecord)zone.Records.Last();
            ClassicAssert.AreEqual("sub.test.com", subRecord.Name);
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
            ClassicAssert.IsNotNull(zone.ToString());
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
            ClassicAssert.AreEqual(3, filtered.Records.Count);
            ClassicAssert.IsTrue(filtered.Records.OfType<AResourceRecord>().Any(item => Equals(item.Address, IPAddress.Parse("127.0.0.1"))));
            ClassicAssert.IsTrue(filtered.Records.OfType<AResourceRecord>().Any(item => Equals(item.Address, IPAddress.Parse("127.0.0.2"))));
            ClassicAssert.IsTrue(filtered.Records.OfType<AResourceRecord>().Any(item => Equals(item.Address, IPAddress.Parse("127.0.0.3"))));


            filtered = zone.Filter("test-example.com");
            ClassicAssert.AreEqual(1, filtered.Records.Count);
            ClassicAssert.IsTrue(filtered.Records.OfType<AResourceRecord>().Any(item => Equals(item.Address, IPAddress.Parse("127.0.0.4"))));

            filtered = zone.Filter("example-test-example.com");
            ClassicAssert.AreEqual(1, filtered.Records.Count);
            ClassicAssert.IsTrue(filtered.Records.OfType<AResourceRecord>().Any(item => Equals(item.Address, IPAddress.Parse("127.0.0.5"))));
        }

        [Test]
        public void IncludeTest() {
            var embedded = new EmbeddedDnsSource(GetType().Assembly, GetType().Namespace + ".Samples", "root_com.zone");
            var zone = DnsZoneFile.Parse(embedded);
            ClassicAssert.AreEqual(2, zone.Records.Count);

            var rootA = zone.Single<AResourceRecord>("root.com");
            ClassicAssert.AreEqual(IPAddress.Parse("192.168.0.1"), rootA.Address);

            var wwwA = zone.Single<AResourceRecord>("www.root.com");
            ClassicAssert.AreEqual(IPAddress.Parse("192.168.0.2"), wwwA.Address);
        }
    }
}
