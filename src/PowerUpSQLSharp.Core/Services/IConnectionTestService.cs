using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Models;

namespace PowerUpSQLSharp.Core.Services
{
    public interface IConnectionTestService
    {
        SqlConnectionTestResult TestConnection(
            SqlConnectionOptions options,
            CancellationToken cancellationToken = default);

        IReadOnlyList<SqlConnectionTestResult> TestConnectionsThreaded(
            IEnumerable<string> instances,
            SqlConnectionOptions templateOptions,
            int maxThreads,
            CancellationToken cancellationToken = default);

        IReadOnlyList<SqlDefaultPasswordMatch> TestDefaultPasswords(
            string instance,
            int connectTimeoutSeconds,
            CancellationToken cancellationToken = default);
    }
}
