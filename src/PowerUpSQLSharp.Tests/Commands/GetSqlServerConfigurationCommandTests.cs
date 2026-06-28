using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Commands;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;
using Xunit;

namespace PowerUpSQLSharp.Tests.Commands
{
    public class GetSqlServerConfigurationCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_ReturnsConfigurationRows()
        {
            var service = new FakeReconnaissanceService(new[]
            {
                new SqlServerConfigurationEntry
                {
                    ComputerName = "SQL01",
                    Instance = @"SQL01\MSSQL",
                    Name = "xp_cmdshell",
                    Minimum = "0",
                    Maximum = "1",
                    ConfigValue = "0",
                    RunValue = "0"
                }
            });

            var command = new GetSqlServerConfigurationCommand(service);
            var context = new CommandContext
            {
                Arguments = new Dictionary<string, string>
                {
                    { "Instance", @"SQL01\MSSQL" },
                    { "Username", "sa" },
                    { "Password", "secret" }
                }
            };

            var result = await command.ExecuteAsync(context).ConfigureAwait(false);

            Assert.Equal(ExitCodes.Success, result.ExitCode);
            Assert.Contains("xp_cmdshell", result.Output);
            Assert.Contains("config_value", result.Output);
        }

        [Fact]
        public async Task ExecuteAsync_NoAccessibleInstances_ReturnsConnectionFailedExitCode()
        {
            var service = new FakeReconnaissanceService(new SqlServerConfigurationEntry[0]);
            var command = new GetSqlServerConfigurationCommand(service);
            var context = new CommandContext
            {
                Arguments = new Dictionary<string, string> { { "Instance", @"SQL01\MSSQL" } }
            };

            var result = await command.ExecuteAsync(context).ConfigureAwait(false);

            Assert.Equal(ExitCodes.ConnectionFailed, result.ExitCode);
        }

        private sealed class FakeReconnaissanceService : IReconnaissanceService
        {
            private readonly IReadOnlyList<SqlServerConfigurationEntry> _entries;

            public FakeReconnaissanceService(IReadOnlyList<SqlServerConfigurationEntry> entries)
            {
                _entries = entries ?? new SqlServerConfigurationEntry[0];
            }

            public SqlServerInfo GetServerInfo(
                SqlConnectionOptions options,
                CancellationToken cancellationToken = default)
            {
                return null;
            }

            public IReadOnlyList<SqlServerInfo> GetServerInfoThreaded(
                IEnumerable<string> instances,
                SqlConnectionOptions templateOptions,
                int maxThreads,
                CancellationToken cancellationToken = default)
            {
                return new SqlServerInfo[0];
            }

            public IReadOnlyList<SqlServerConfigurationEntry> GetServerConfiguration(
                SqlConnectionOptions options,
                CancellationToken cancellationToken = default)
            {
                return _entries;
            }

            public IReadOnlyList<SqlServerConfigurationEntry> GetServerConfigurationThreaded(
                IEnumerable<string> instances,
                SqlConnectionOptions templateOptions,
                int maxThreads,
                CancellationToken cancellationToken = default)
            {
                return _entries;
            }

            public IReadOnlyList<IDictionary<string, object>> Query(
                SqlReconOperation operation,
                SqlConnectionOptions options,
                ReconQueryFilters filters,
                CancellationToken cancellationToken = default)
            {
                return Array.Empty<IDictionary<string, object>>();
            }

            public IReadOnlyList<IDictionary<string, object>> QueryThreaded(
                SqlReconOperation operation,
                IEnumerable<string> instances,
                SqlConnectionOptions templateOptions,
                ReconQueryFilters filters,
                int maxThreads,
                CancellationToken cancellationToken = default)
            {
                return Array.Empty<IDictionary<string, object>>();
            }
        }
    }
}
