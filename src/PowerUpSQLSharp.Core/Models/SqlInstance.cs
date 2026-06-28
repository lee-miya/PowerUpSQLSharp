namespace PowerUpSQLSharp.Core.Models
{
    public sealed class SqlInstance
    {
        public string ComputerName { get; set; }
        public string InstanceName { get; set; }
        public string ServerInstance { get; set; }
        public int? Port { get; set; }
        public string Version { get; set; }
        public bool IsClustered { get; set; }
        public string ServerIp { get; set; }
    }
}
