using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Commands;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;
using Xunit;

namespace PowerUpSQLSharp.Tests.Commands
{
    public class GetSqlInstanceBroadcastCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_ReturnsBroadcastColumns()
        {
            var service = new FakeDiscoveryService(new[]
            {
                new SqlInstance
                {
                    ComputerName = "SQL01",
                    InstanceName = "MSSQL2019",
                    ServerInstance = @"SQL01\MSSQL2019",
                    Version = "15.0.2000.5",
                    IsClustered = false
                }
            });

            var command = new GetSqlInstanceBroadcastCommand(service);
            var result = await command.ExecuteAsync(new CommandContext()).ConfigureAwait(false);

            Assert.Equal(ExitCodes.Success, result.ExitCode);
            Assert.Contains("SQL01", result.Output);
            Assert.Contains("MSSQL2019", result.Output);
            Assert.Contains("15.0.2000.5", result.Output);
        }

        [Fact]
        public async Task ExecuteAsync_UdpPing_UsesUdpColumns()
        {
            var service = new FakeDiscoveryService(new[]
            {
                new SqlInstance
                {
                    ComputerName = "SQL01",
                    InstanceName = "MSSQL2019",
                    ServerInstance = @"SQL01\MSSQL2019",
                    ServerIp = "10.0.0.1",
                    Port = 1433,
                    Version = "15.0.2000.5"
                }
            });

            var command = new GetSqlInstanceBroadcastCommand(service);
            var context = new CommandContext
            {
                Arguments = new Dictionary<string, string> { { "UDPPing", "true" } }
            };

            var result = await command.ExecuteAsync(context).ConfigureAwait(false);

            Assert.Equal(ExitCodes.Success, result.ExitCode);
            Assert.Contains("TCPPort", result.Output);
            Assert.Contains("10.0.0.1", result.Output);
        }

        private sealed class FakeDiscoveryService : IInstanceDiscoveryService
        {
            private readonly IReadOnlyList<SqlInstance> _instances;

            public FakeDiscoveryService(IReadOnlyList<SqlInstance> instances)
            {
                _instances = instances;
            }

            public IReadOnlyList<SqlInstance> GetLocalInstances() => _instances;

            public IReadOnlyList<SqlInstance> GetBroadcastInstances() => _instances;

            public IReadOnlyList<SqlInstance> GetInstancesFromFile(string filePath) => _instances;

            public IReadOnlyList<SqlInstance> ScanUdp(
                string computerName,
                int timeoutSeconds,
                CancellationToken cancellationToken = default) => _instances;

            public IReadOnlyList<SqlInstance> ScanUdpThreaded(
                IEnumerable<string> computerNames,
                int timeoutSeconds,
                int maxThreads,
                CancellationToken cancellationToken = default) => _instances;

            public DomainDiscoveryResult GetDomainInstances(
                DomainInstanceQuery query,
                CancellationToken cancellationToken = default) => new DomainDiscoveryResult();
        }
    }
}
