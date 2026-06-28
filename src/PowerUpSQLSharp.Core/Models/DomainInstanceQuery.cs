namespace PowerUpSQLSharp.Core.Models
{
    public sealed class DomainInstanceQuery
    {
        public DomainLdapOptions Ldap { get; set; } = new DomainLdapOptions();
        public bool CheckMgmt { get; set; }
        public bool IncludeIp { get; set; }
        public int UdpTimeoutSeconds { get; set; } = 3;
        public int Threads { get; set; } = 10;
    }
}
