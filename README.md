# About

DnsZone is a tool for parsing and building dns zone file.

Available at [nuget.org](https://www.nuget.org/packages/DnsZone/)

Donations: ```bitcoin:12aM1SmiG6QmirQK3huSmPRhyPPQQvqkUv```

# Parsing zone file

Parse dns zone from existing string:

```C#
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
var zone = DnsZoneFile.Parse(content);
```

Alternatively your can load zone from external uri:

```C#
var zone = await DnsZoneFile.LoadAsync(uri)
```

# Generating zone file

You can use regular `ToString()` method to produce zone file from `DnsZoneFile` object

```C#
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
Console.Write(zone.ToString());
```

This will generate following content:

```
;A records
www.example.com.	IN	15m	A	127.0.0.1	
ftp.example.com.	IN	15m	A	127.0.0.1	
```
You can give a hint to the formatter which domain should be used as an origin to make output cleaner

```C#
Console.Write(zone.ToString("example.com"));
```

This will generate following content

```
$ORIGIN example.com.
;A records
www	IN	15m	A	127.0.0.1	
ftp	IN	15m	A	127.0.0.1	
```

# References

[RFC 1035 - Domain names - implementation and specification](https://tools.ietf.org/html/rfc1035)

[RFC 1034 - Domain names - concepts and facilities](https://tools.ietf.org/html/rfc1034)

[RFC 2782 - A DNS RR for specifying the location of services (DNS SRV)](https://tools.ietf.org/html/rfc2782)
