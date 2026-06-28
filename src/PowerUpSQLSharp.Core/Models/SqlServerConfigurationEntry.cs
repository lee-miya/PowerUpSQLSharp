namespace PowerUpSQLSharp.Core.Models
{
    public sealed class SqlServerConfigurationEntry
    {
        public string ComputerName { get; set; }
        public string Instance { get; set; }
        public string Name { get; set; }
        public string Minimum { get; set; }
        public string Maximum { get; set; }
        public string ConfigValue { get; set; }
        public string RunValue { get; set; }
    }
}
