using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Commands;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;
using PowerUpSQLSharp.Core.Utils;
using Xunit;

namespace PowerUpSQLSharp.Tests.Commands
{
    public class GetSqlServerLoginDefaultPwCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_DefaultInstance_ReturnsEmptyResult()
        {
            var command = new GetSqlServerLoginDefaultPwCommand(new ConnectionTestService(new SqlConnectionFactory()));
            var context = new CommandContext
            {
                Arguments = new Dictionary<string, string> { { "Instance", "SQL01" } }
            };

            var result = await command.ExecuteAsync(context).ConfigureAwait(false);

            Assert.Equal(ExitCodes.Success, result.ExitCode);
            Assert.True(string.IsNullOrEmpty(result.Output));
        }

        [Fact]
        public async Task ExecuteAsync_UnknownNamedInstance_ReturnsEmptyResult()
        {
            var command = new GetSqlServerLoginDefaultPwCommand(new ConnectionTestService(new SqlConnectionFactory()));
            var context = new CommandContext
            {
                Arguments = new Dictionary<string, string> { { "Instance", @"SQL01\UNKNOWNINSTANCE" } }
            };

            var result = await command.ExecuteAsync(context).ConfigureAwait(false);

            Assert.Equal(ExitCodes.Success, result.ExitCode);
            Assert.True(string.IsNullOrEmpty(result.Output));
        }

        [Fact]
        public async Task ExecuteAsync_KnownInstanceWithoutSql_ReturnsEmptyResult()
        {
            var service = new FakeConnectionTestService(new SqlDefaultPasswordMatch[0]);
            var command = new GetSqlServerLoginDefaultPwCommand(service);
            var context = new CommandContext
            {
                Arguments = new Dictionary<string, string> { { "Instance", @"SQL01\STANDARDDEV2014" } }
            };

            var result = await command.ExecuteAsync(context).ConfigureAwait(false);

            Assert.Equal(ExitCodes.Success, result.ExitCode);
            Assert.True(string.IsNullOrEmpty(result.Output));
        }

        [Fact]
        public async Task ExecuteAsync_MatchedCredentials_ReturnsDefaultPasswordColumns()
        {
            var service = new FakeConnectionTestService(new[]
            {
                new SqlDefaultPasswordMatch
                {
                    Computer = "SQL01",
                    Instance = @"SQL01\STANDARDDEV2014",
                    Username = "test",
                    Password = "test",
                    IsSysadmin = "No"
                }
            });

            var command = new GetSqlServerLoginDefaultPwCommand(service);
            var context = new CommandContext
            {
                Arguments = new Dictionary<string, string> { { "Instance", @"SQL01\STANDARDDEV2014" } }
            };

            var result = await command.ExecuteAsync(context).ConfigureAwait(false);

            Assert.Equal(ExitCodes.Success, result.ExitCode);
            Assert.Contains("STANDARDDEV2014", result.Output);
            Assert.Contains("test", result.Output);
            Assert.Contains("IsSysadmin", result.Output);
        }

        [Fact]
        public void DefaultPasswordDatabase_KnownInstance_HasEntries()
        {
            var entries = SqlDefaultPasswordDatabase.GetEntriesForInstanceName("STANDARDDEV2014");

            Assert.Single(entries);
            Assert.Equal("test", entries[0].Username);
            Assert.Equal("test", entries[0].Password);
        }

        private sealed class FakeConnectionTestService : IConnectionTestService
        {
            private readonly IReadOnlyList<SqlDefaultPasswordMatch> _matches;

            public FakeConnectionTestService(IReadOnlyList<SqlDefaultPasswordMatch> matches)
            {
                _matches = matches;
            }

            public SqlConnectionTestResult TestConnection(
                SqlConnectionOptions options,
                CancellationToken cancellationToken = default)
            {
                return new SqlConnectionTestResult();
            }

            public IReadOnlyList<SqlConnectionTestResult> TestConnectionsThreaded(
                IEnumerable<string> instances,
                SqlConnectionOptions templateOptions,
                int maxThreads,
                CancellationToken cancellationToken = default)
            {
                return new SqlConnectionTestResult[0];
            }

            public IReadOnlyList<SqlDefaultPasswordMatch> TestDefaultPasswords(
                string instance,
                int connectTimeoutSeconds,
                CancellationToken cancellationToken = default)
            {
                return _matches;
            }
        }
    }
}
