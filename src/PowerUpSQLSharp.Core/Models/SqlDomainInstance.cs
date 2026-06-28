namespace PowerUpSQLSharp.Core.Models
{
    public sealed class SqlDomainInstance
    {
        public string ComputerName { get; set; }
        public string ServerInstance { get; set; }
        public string DomainAccountSid { get; set; }
        public string DomainAccount { get; set; }
        public string DomainAccountCn { get; set; }
        public string Service { get; set; }
        public string Spn { get; set; }
        public string LastLogon { get; set; }
        public string Description { get; set; }
        public string IpAddress { get; set; }
    }
}
