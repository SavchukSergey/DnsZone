namespace DnsZone.Records {
    public enum ResourceRecordType {
        A,
        AAAA,
        NS,
        MD,
        MF,
        CNAME,
        SOA,
        MB,
        MG,
        MR,
        NULL,
        WKS,
        PTR,
        SRV,
        HINFO,
        MX,
        TXT,
        CAA,
        TLSA,
        SSHFP,
        ALIAS,
        DNAME,
        DS,
        LUA,
        NAPTR,
        DNSKEY,
        RRSIG,
        NSEC,
        NSEC3,
        NSEC3PARAM,
        CDS,
        CDNSKEY,
        /// <summary>
        /// Type not known to the library (RFC 3597), see <see cref="UnknownResourceRecord"/>
        /// </summary>
        Unknown
    }
}
