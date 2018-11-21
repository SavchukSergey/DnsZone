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
        public async Task Parse2Test() {
            var zone = await DnsZoneFile.LoadFromFileAsync(@"Samples/domain.com.zone", "domain.com");
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
                Assert.AreEqual(1, zone.Records.OfType<SoaResourceRecord>().Count());
                Assert.AreEqual(2, zone.Records.OfType<NsResourceRecord>().Count());
                Assert.AreEqual(3, zone.Records.OfType<MxResourceRecord>().Count());
                Assert.AreEqual(5, zone.Records.OfType<AResourceRecord>().Count());
                Assert.AreEqual(2, zone.Records.OfType<CNameResourceRecord>().Count());
            } catch (TokenException exc) {
                Console.WriteLine(exc.Token.Position.GetLine());
                throw;
            }
        }

        [Test]
        public void TxtRecordParseTest() {
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
        public void TxtRecordNoQuotesParseTest()
        {
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
            Assert.AreEqual(1, zone.Records.Count);

            Assert.IsAssignableFrom<SoaResourceRecord>(zone.Records.First());

            var record = (SoaResourceRecord)zone.Records.First();
            Assert.AreEqual("example.com", record.Name);
            Assert.AreEqual("IN", record.Class);
            Assert.AreEqual(ResourceRecordType.SOA, record.Type);
            Assert.AreEqual("ns.example.com", record.NameServer);
            Assert.AreEqual("2003080800", record.SerialNumber);
            Assert.AreEqual(TimeSpan.FromDays(2), record.Refresh);
            Assert.AreEqual(TimeSpan.FromMinutes(15), record.Retry);
            Assert.AreEqual(TimeSpan.FromDays(14), record.Expiry);
            Assert.AreEqual(TimeSpan.FromHours(1), record.Minimum);
        }

        [Test]
        public void AaaaRecordParseTest() {
            const string str = @"
; zone fragment for example.com
$TTL 2d ; zone default = 2 days or 172800 seconds
$ORIGIN example.com.
joe        IN      AAAA      2001:db8::3  ; joe & www = same ip
www        IN      AAAA      2001:db8::3
; functionally the same as the record above
www.example.com.   AAAA      2001:db8::3
fred  3600 IN      AAAA      2001:db8::4  ; ttl =3600 overrides $TTL default
ftp        IN      AAAA      2001:db8::5 ; round robin with next
           IN      AAAA      2001:db8::6
mail       IN      AAAA      2001:db8::7  ; mail = round robin
mail       IN      AAAA      2001:db8::32
mail       IN      AAAA      2001:db8::33
squat      IN      AAAA      2001:db8:0:0:1::13  ; address in another subnet";
            var zone = DnsZoneFile.Parse(str);
            Assert.AreEqual(10, zone.Records.Count);

            Assert.IsAssignableFrom<AaaaResourceRecord>(zone.Records.First());

            var record = (AaaaResourceRecord)zone.Records.First();
            Assert.AreEqual("joe.example.com", record.Name);
            Assert.AreEqual("IN", record.Class);
            Assert.AreEqual(ResourceRecordType.AAAA, record.Type);
            Assert.AreEqual(IPAddress.Parse("2001:db8::3"), record.Address);
        }

        [Test]
        public void CNameRecordParseTest() {
            const string str = @"
; zone fragment for example.com
$TTL 2d ; zone default = 2 days or 172800 seconds
$ORIGIN example.com.
www        IN      CNAME  server1
ftp        IN      CNAME  server1";
            var zone = DnsZoneFile.Parse(str);
            Assert.AreEqual(2, zone.Records.Count);

            Assert.IsAssignableFrom<CNameResourceRecord>(zone.Records.First());

            var record = (CNameResourceRecord)zone.Records.First();
            Assert.AreEqual("www.example.com", record.Name);
            Assert.AreEqual("IN", record.Class);
            Assert.AreEqual(ResourceRecordType.CNAME, record.Type);
            Assert.AreEqual("server1.example.com", record.CanonicalName);
        }

        [Test]
        public void MxRecordParseTest() {
            const string str = @"
; zone fragment example.com
; mail servers in the same zone
; will support incoming email with addresses of the format 
; user@example.com
$TTL 2d ; zone default = 2 days or 172800 seconds
$ORIGIN example.com.
@               IN     MX     10  mail.foo.com.
@               IN     MX     20  mail2.foo.com.";
            var zone = DnsZoneFile.Parse(str);
            Assert.AreEqual(2, zone.Records.Count);

            Assert.IsAssignableFrom<MxResourceRecord>(zone.Records.First());

            var record = (MxResourceRecord)zone.Records.First();
            Assert.AreEqual("example.com", record.Name);
            Assert.AreEqual("IN", record.Class);
            Assert.AreEqual(ResourceRecordType.MX, record.Type);
            Assert.AreEqual(10, record.Preference);
            Assert.AreEqual("mail.foo.com", record.Exchange);
        }

        [Test]
        public void NsRecordParseTest() {
            const string str = @"
; zone fragment example.com
; mail servers in the same zone
; will support incoming email with addresses of the format 
; user@example.com
$TTL 2d ; zone default = 2 days or 172800 seconds
$ORIGIN example.com.
@               IN      NS     ns1.example.net.
@               IN      NS     ns1.example.org.";
            var zone = DnsZoneFile.Parse(str);
            Assert.AreEqual(2, zone.Records.Count);

            Assert.IsAssignableFrom<NsResourceRecord>(zone.Records.First());

            var record = (NsResourceRecord)zone.Records.First();
            Assert.AreEqual("example.com", record.Name);
            Assert.AreEqual("IN", record.Class);
            Assert.AreEqual(ResourceRecordType.NS, record.Type);
            Assert.AreEqual("ns1.example.net", record.NameServer);
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
            Assert.AreEqual(4, zone.Records.Count);

            Assert.IsAssignableFrom<PtrResourceRecord>(zone.Records.First());

            var record = (PtrResourceRecord)zone.Records.First();
            Assert.AreEqual("2.23.168.192.IN-ADDR.ARPA", record.Name);
            Assert.AreEqual("IN", record.Class);
            Assert.AreEqual(ResourceRecordType.PTR, record.Type);
            Assert.AreEqual("joe.example.com", record.HostName);
        }

        [Test]
        public void ARecordParseTest() {
            const string str = @"
; zone file snippet
$ORIGIN example.com.
$TTL 2d ; 172800 secs
alice  IN  A   192.168.2.1 ; real host name
ns1    IN   A  192.168.2.1 ; service name
alice  IN   A  192.168.2.1 ; host name (same IPv4)";
            var zone = DnsZoneFile.Parse(str);
            Assert.AreEqual(3, zone.Records.Count);

            Assert.IsAssignableFrom<AResourceRecord>(zone.Records.First());

            var record = (AResourceRecord)zone.Records.First();
            Assert.AreEqual("alice.example.com", record.Name);
            Assert.AreEqual("IN", record.Class);
            Assert.AreEqual(ResourceRecordType.A, record.Type);
            Assert.AreEqual(IPAddress.Parse("192.168.2.1"), record.Address);
        }

        [Test]
        public void SrvRecordParseTest() {
            const string str = @"
$ORIGIN example.com.
$TTL 2d ; 172800 secs
; foobar - use old-slow-box or new-fast-box if either is
; available, make three quarters of the logins go to
; new-fast-box.
_foobar._tcp    SRV 0 1 9 old-slow-box.example.com.
                SRV 0 3 9 new-fast-box.example.com.
; if neither old-slow-box or new-fast-box is up, switch to
; using the sysdmin's box and the server
                SRV 1 0 9 sysadmins-box.example.com.
                SRV 1 0 9 server.example.com.
*._tcp          SRV  0 0 0 .
*._udp          SRV  0 0 0 .";
            var zone = DnsZoneFile.Parse(str);
            Assert.AreEqual(6, zone.Records.Count);

            Assert.IsAssignableFrom<SrvResourceRecord>(zone.Records.First());

            var record = (SrvResourceRecord)zone.Records.First();
            Assert.AreEqual("_foobar._tcp.example.com", record.Name);
            Assert.AreEqual(null, record.Class);
            Assert.AreEqual(ResourceRecordType.SRV, record.Type);
            Assert.AreEqual(0, record.Priority);
        }

        [Test]
        public void ExplicitOriginTest() {
            const string str = @"
; A Records
@	600	IN	A	184.168.221.14
sub	600	IN	A	184.168.221.15
";
            var zone = DnsZoneFile.Parse(str, "test.com");
            Assert.AreEqual(2, zone.Records.Count);

            Assert.IsAssignableFrom<AResourceRecord>(zone.Records.First());

            var rootRecord = (AResourceRecord)zone.Records.First();
            Assert.AreEqual("test.com", rootRecord.Name);
            var subRecord = (AResourceRecord)zone.Records.Last();
            Assert.AreEqual("sub.test.com", subRecord.Name);
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
            Assert.IsNotNull(zone.ToString());
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
            Assert.AreEqual(3, filtered.Records.Count);
            Assert.IsTrue(filtered.Records.OfType<AResourceRecord>().Any(item => Equals(item.Address, IPAddress.Parse("127.0.0.1"))));
            Assert.IsTrue(filtered.Records.OfType<AResourceRecord>().Any(item => Equals(item.Address, IPAddress.Parse("127.0.0.2"))));
            Assert.IsTrue(filtered.Records.OfType<AResourceRecord>().Any(item => Equals(item.Address, IPAddress.Parse("127.0.0.3"))));


            filtered = zone.Filter("test-example.com");
            Assert.AreEqual(1, filtered.Records.Count);
            Assert.IsTrue(filtered.Records.OfType<AResourceRecord>().Any(item => Equals(item.Address, IPAddress.Parse("127.0.0.4"))));

            filtered = zone.Filter("example-test-example.com");
            Assert.AreEqual(1, filtered.Records.Count);
            Assert.IsTrue(filtered.Records.OfType<AResourceRecord>().Any(item => Equals(item.Address, IPAddress.Parse("127.0.0.5"))));
        }

        [Test]
        public void IncludeTest() {
            var embedded = new EmbeddedDnsSource(GetType().Assembly, GetType().Namespace + ".Samples", "root_com.zone");
            var zone = DnsZoneFile.Parse(embedded);
            Assert.AreEqual(2, zone.Records.Count);

            var rootA = zone.Single<AResourceRecord>("root.com");
            Assert.AreEqual(IPAddress.Parse("192.168.0.1"), rootA.Address);

            var wwwA = zone.Single<AResourceRecord>("www.root.com");
            Assert.AreEqual(IPAddress.Parse("192.168.0.2"), wwwA.Address);
        }
    }
}
