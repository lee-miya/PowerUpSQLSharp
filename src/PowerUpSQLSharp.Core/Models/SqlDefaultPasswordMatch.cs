namespace PowerUpSQLSharp.Core.Models
{
    public sealed class SqlDefaultPasswordMatch
    {
        public string Computer { get; set; }
        public string Instance { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string IsSysadmin { get; set; }
    }
}
