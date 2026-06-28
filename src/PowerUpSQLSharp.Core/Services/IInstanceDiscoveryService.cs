using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Utils;

namespace PowerUpSQLSharp.Core.Services
{
    public interface IInstanceDiscoveryService
    {
        IReadOnlyList<SqlInstance> GetLocalInstances();

        IReadOnlyList<SqlInstance> GetBroadcastInstances();

        IReadOnlyList<SqlInstance> GetInstancesFromFile(string filePath);

        IReadOnlyList<SqlInstance> ScanUdp(
            string computerName,
            int timeoutSeconds,
            CancellationToken cancellationToken = default);

        IReadOnlyList<SqlInstance> ScanUdpThreaded(
            IEnumerable<string> computerNames,
            int timeoutSeconds,
            int maxThreads,
            CancellationToken cancellationToken = default);

        DomainDiscoveryResult GetDomainInstances(
            DomainInstanceQuery query,
            CancellationToken cancellationToken = default);
    }
}
