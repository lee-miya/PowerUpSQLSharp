namespace PowerUpSQLSharp.Core.Models
{
    public sealed class SqlConnectionTestResult
    {
        public string ComputerName { get; set; }
        public string Instance { get; set; }
        public string Status { get; set; }
        public bool Success { get; set; }
    }
}
