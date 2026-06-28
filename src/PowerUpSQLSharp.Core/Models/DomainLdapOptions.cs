namespace PowerUpSQLSharp.Core.Models
{
    public sealed class DomainLdapOptions
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string DomainController { get; set; }
        public string ComputerName { get; set; }
        public string DomainAccount { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
    }
}
