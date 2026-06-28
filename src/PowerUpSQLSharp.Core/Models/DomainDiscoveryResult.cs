using System.Collections.Generic;

namespace PowerUpSQLSharp.Core.Models
{
    public sealed class DomainDiscoveryResult
    {
        public IReadOnlyList<SqlDomainInstance> Instances { get; set; } = new SqlDomainInstance[0];
        public string Warning { get; set; }
        public bool SummaryOnly { get; set; }
    }
}
