namespace PowerUpSQLSharp.Core.Models
{
    public sealed class GlobalOptions
    {
        public bool Quiet { get; set; }
        public bool NoColor { get; set; }
        public bool Verbose { get; set; }
        public string Format { get; set; } = "table";
        public int TimeoutSeconds { get; set; } = 30;
        public int Threads { get; set; } = 10;
    }
}
