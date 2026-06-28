using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Commands;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;
using Xunit;

namespace PowerUpSQLSharp.Tests.Commands
{
    public class GetSqlInstanceLocalCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_ReturnsTableOutput()
        {
            var service = new FakeDiscoveryService(new[]
            {
                new SqlInstance
                {
                    ComputerName = "HOST-01",
                    InstanceName = "MSSQL2019",
                    ServerInstance = @"HOST-01\MSSQL2019",
                    Port = 1433,
                    Version = "15.0.2000.5"
                }
            });

            var command = new GetSqlInstanceLocalCommand(service);
            var result = await command.ExecuteAsync(new CommandContext()).ConfigureAwait(false);

            Assert.Equal(ExitCodes.Success, result.ExitCode);
            Assert.Contains("HOST-01", result.Output);
            Assert.Contains("MSSQL2019", result.Output);
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsJsonOutput()
        {
            var service = new FakeDiscoveryService(new[]
            {
                new SqlInstance
                {
                    ComputerName = "HOST-01",
                    InstanceName = string.Empty,
                    ServerInstance = "HOST-01",
                    Port = 1433
                }
            });

            var command = new GetSqlInstanceLocalCommand(service);
            var context = new CommandContext { Global = new GlobalOptions { Format = "json" } };
            var result = await command.ExecuteAsync(context).ConfigureAwait(false);

            Assert.Equal(ExitCodes.Success, result.ExitCode);
            Assert.StartsWith("[{", result.Output);
            Assert.Contains("\"ServerInstance\":\"HOST-01\"", result.Output);
        }

        [Fact]
        public async Task ExecuteAsync_NoInstances_ReturnsSuccessWithEmptyOutput()
        {
            var command = new GetSqlInstanceLocalCommand(new FakeDiscoveryService(new SqlInstance[0]));
            var result = await command.ExecuteAsync(new CommandContext()).ConfigureAwait(false);

            Assert.Equal(ExitCodes.Success, result.ExitCode);
            Assert.True(string.IsNullOrEmpty(result.Output));
        }

        private sealed class FakeDiscoveryService : IInstanceDiscoveryService
        {
            private readonly IReadOnlyList<SqlInstance> _instances;

            public FakeDiscoveryService(IReadOnlyList<SqlInstance> instances)
            {
                _instances = instances;
            }

            public IReadOnlyList<SqlInstance> GetLocalInstances()
            {
                return _instances;
            }

            public IReadOnlyList<SqlInstance> GetBroadcastInstances()
            {
                return _instances;
            }

            public IReadOnlyList<SqlInstance> GetInstancesFromFile(string filePath)
            {
                return _instances;
            }

            public IReadOnlyList<SqlInstance> ScanUdp(
                string computerName,
                int timeoutSeconds,
                CancellationToken cancellationToken = default)
            {
                return _instances;
            }

            public IReadOnlyList<SqlInstance> ScanUdpThreaded(
                IEnumerable<string> computerNames,
                int timeoutSeconds,
                int maxThreads,
                CancellationToken cancellationToken = default)
            {
                return _instances;
            }

            public DomainDiscoveryResult GetDomainInstances(
                DomainInstanceQuery query,
                CancellationToken cancellationToken = default)
            {
                return new DomainDiscoveryResult();
            }
        }
    }
}
