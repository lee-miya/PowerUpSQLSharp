using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Commands;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;
using Xunit;

namespace PowerUpSQLSharp.Tests.Commands
{
    public class GetSqlConnectionTestCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_SuccessfulConnection_ReturnsAccessibleStatus()
        {
            var service = new FakeConnectionTestService(new[]
            {
                new SqlConnectionTestResult
                {
                    ComputerName = "SQL01",
                    Instance = @"SQL01\MSSQL",
                    Status = "Accessible",
                    Success = true
                }
            });

            var command = new GetSqlConnectionTestCommand(service);
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
            Assert.Contains("Accessible", result.Output);
            Assert.Contains("true", result.Output);
        }

        [Fact]
        public async Task ExecuteAsync_FailedConnection_ReturnsConnectionFailedExitCode()
        {
            var service = new FakeConnectionTestService(new[]
            {
                new SqlConnectionTestResult
                {
                    ComputerName = "SQL01",
                    Instance = @"SQL01\MSSQL",
                    Status = "Not Accessible",
                    Success = false
                }
            });

            var command = new GetSqlConnectionTestCommand(service);
            var context = new CommandContext
            {
                Arguments = new Dictionary<string, string>
                {
                    { "Instance", @"SQL01\MSSQL" },
                    { "Username", "sa" },
                    { "Password", "wrong" }
                }
            };

            var result = await command.ExecuteAsync(context).ConfigureAwait(false);

            Assert.Equal(ExitCodes.ConnectionFailed, result.ExitCode);
            Assert.Contains("Not Accessible", result.Output);
        }

        [Fact]
        public async Task ExecuteAsync_JsonFormat_IncludesSuccessField()
        {
            var service = new FakeConnectionTestService(new[]
            {
                new SqlConnectionTestResult
                {
                    ComputerName = "SQL01",
                    Instance = @"SQL01\MSSQL",
                    Status = "Accessible",
                    Success = true
                }
            });

            var command = new GetSqlConnectionTestCommand(service);
            var context = new CommandContext
            {
                Global = new GlobalOptions { Format = "json" },
                Arguments = new Dictionary<string, string> { { "Instance", @"SQL01\MSSQL" } }
            };

            var result = await command.ExecuteAsync(context).ConfigureAwait(false);

            Assert.Contains("\"Success\":true", result.Output);
            Assert.Contains("\"Status\":\"Accessible\"", result.Output);
        }

        private sealed class FakeConnectionTestService : IConnectionTestService
        {
            private readonly IReadOnlyList<SqlConnectionTestResult> _results;

            public FakeConnectionTestService(IReadOnlyList<SqlConnectionTestResult> results)
            {
                _results = results;
            }

            public SqlConnectionTestResult TestConnection(
                SqlConnectionOptions options,
                CancellationToken cancellationToken = default)
            {
                return _results[0];
            }

            public IReadOnlyList<SqlConnectionTestResult> TestConnectionsThreaded(
                IEnumerable<string> instances,
                SqlConnectionOptions templateOptions,
                int maxThreads,
                CancellationToken cancellationToken = default)
            {
                return _results;
            }

            public IReadOnlyList<SqlDefaultPasswordMatch> TestDefaultPasswords(
                string instance,
                int connectTimeoutSeconds,
                CancellationToken cancellationToken = default)
            {
                return new SqlDefaultPasswordMatch[0];
            }
        }
    }
}
