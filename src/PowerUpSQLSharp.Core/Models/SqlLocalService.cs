namespace PowerUpSQLSharp.Core.Models
{
    public sealed class SqlLocalService
    {
        public string ComputerName { get; set; }
        public string Instance { get; set; }
        public string ServiceDisplayName { get; set; }
        public string ServiceName { get; set; }
        public string ServicePath { get; set; }
        public string ServiceAccount { get; set; }
        public string ServiceState { get; set; }
        public string ServiceProcessId { get; set; }
    }
}
