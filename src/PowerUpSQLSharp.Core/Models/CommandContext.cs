using System.Collections.Generic;

namespace PowerUpSQLSharp.Core.Models
{
    public sealed class CommandContext
    {
        public GlobalOptions Global { get; set; } = new GlobalOptions();
        public string CommandName { get; set; }
        public IDictionary<string, string> Arguments { get; set; } = new Dictionary<string, string>();
    }
}
