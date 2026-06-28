namespace PowerUpSQLSharp.Core.Models
{
    public sealed class DomainSpnEntry
    {
        public string UserSid { get; set; }
        public string User { get; set; }
        public string UserCn { get; set; }
        public string Service { get; set; }
        public string ComputerName { get; set; }
        public string Spn { get; set; }
        public string LastLogon { get; set; }
        public string Description { get; set; }
    }
}
