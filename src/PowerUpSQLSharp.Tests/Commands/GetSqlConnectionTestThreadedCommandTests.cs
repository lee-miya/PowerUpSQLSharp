using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Commands;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;
using Xunit;

namespace PowerUpSQLSharp.Tests.Commands
{
    public class GetSqlConnectionTestThreadedCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_PartialFailure_ReturnsPartialFailureExitCode()
        {
            var service = new FakeConnectionTestService(new[]
            {
                new SqlConnectionTestResult
                {
                    ComputerName = "SQL01",
                    Instance = "SQL01",
                    Status = "Accessible",
                    Success = true
                },
                new SqlConnectionTestResult
                {
                    ComputerName = "SQL02",
                    Instance = "SQL02",
                    Status = "Not Accessible",
                    Success = false
                }
            });

            var command = new GetSqlConnectionTestThreadedCommand(service);
            var context = new CommandContext
            {
                Arguments = new Dictionary<string, string>
                {
                    { "Instance", "SQL01,SQL02" },
                    { "Threads", "2" }
                }
            };

            var result = await command.ExecuteAsync(context).ConfigureAwait(false);

            Assert.Equal(ExitCodes.PartialFailure, result.ExitCode);
            Assert.Contains("Accessible", result.Output);
            Assert.Contains("Not Accessible", result.Output);
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
