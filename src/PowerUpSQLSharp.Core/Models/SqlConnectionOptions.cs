namespace PowerUpSQLSharp.Core.Models
{
    public sealed class SqlConnectionOptions
    {
        public string Instance { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Dac { get; set; }
        public string Database { get; set; } = "Master";
        public int ConnectTimeoutSeconds { get; set; } = 30;
    }
}
