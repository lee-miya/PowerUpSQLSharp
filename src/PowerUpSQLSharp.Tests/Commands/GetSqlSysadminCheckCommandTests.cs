using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Commands;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;
using Xunit;

namespace PowerUpSQLSharp.Tests.Commands
{
    public class GetSqlSysadminCheckCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_SuccessfulCheck_ReturnsSysadminFields()
        {
            var service = new FakePrivilegeInspectionService(new[]
            {
                new SqlSysadminStatus
                {
                    ComputerName = "SQL01",
                    Instance = @"SQL01\MSSQL",
                    IsSysadmin = "Yes"
                }
            });

            var command = new GetSqlSysadminCheckCommand(service);
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
            Assert.Contains("IsSysadmin", result.Output);
            Assert.Contains("Yes", result.Output);
        }

        [Fact]
        public async Task ExecuteAsync_ConnectionFailed_ReturnsConnectionFailedExitCode()
        {
            var service = new FakePrivilegeInspectionService(new SqlSysadminStatus[0]);
            var command = new GetSqlSysadminCheckCommand(service);
            var context = new CommandContext
            {
                Arguments = new Dictionary<string, string> { { "Instance", @"SQL01\MSSQL" } }
            };

            var result = await command.ExecuteAsync(context).ConfigureAwait(false);

            Assert.Equal(ExitCodes.ConnectionFailed, result.ExitCode);
        }

        private sealed class FakePrivilegeInspectionService : IPrivilegeInspectionService
        {
            private readonly IReadOnlyList<SqlSysadminStatus> _results;
            private int _callIndex;

            public FakePrivilegeInspectionService(IReadOnlyList<SqlSysadminStatus> results)
            {
                _results = results ?? new SqlSysadminStatus[0];
            }

            public SqlSysadminStatus GetSysadminStatus(
                SqlConnectionOptions options,
                CancellationToken cancellationToken = default)
            {
                if (_results.Count == 0)
                {
                    return null;
                }

                return _results[System.Math.Min(_callIndex++, _results.Count - 1)];
            }

            public SqlLocalAdminStatus CheckLocalAdministrator()
            {
                return new SqlLocalAdminStatus { CurrentUser = "TEST\\user", IsLocalAdmin = false };
            }
        }
    }
}
