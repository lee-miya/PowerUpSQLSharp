using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Commands;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;
using Xunit;

namespace PowerUpSQLSharp.Tests.Commands
{
    public class GetSqlLocalAdminCheckCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_ReturnsLocalAdminStatus()
        {
            var service = new FakePrivilegeInspectionService(new SqlLocalAdminStatus
            {
                CurrentUser = @"DOMAIN\operator",
                IsLocalAdmin = true
            });

            var command = new GetSqlLocalAdminCheckCommand(service);
            var result = await command.ExecuteAsync(new CommandContext()).ConfigureAwait(false);

            Assert.Equal(ExitCodes.Success, result.ExitCode);
            Assert.Contains("CurrentUser", result.Output);
            Assert.Contains("IsLocalAdmin", result.Output);
            Assert.Contains("DOMAIN\\operator", result.Output);
            Assert.Contains("True", result.Output);
        }

        private sealed class FakePrivilegeInspectionService : IPrivilegeInspectionService
        {
            private readonly SqlLocalAdminStatus _status;

            public FakePrivilegeInspectionService(SqlLocalAdminStatus status)
            {
                _status = status;
            }

            public SqlSysadminStatus GetSysadminStatus(
                SqlConnectionOptions options,
                CancellationToken cancellationToken = default)
            {
                return null;
            }

            public SqlLocalAdminStatus CheckLocalAdministrator()
            {
                return _status;
            }

            public IReadOnlyList<IDictionary<string, object>> GetDatabasePriv(
                SqlConnectionOptions options,
                ReconQueryFilters filters,
                CancellationToken cancellationToken = default)
            {
                return System.Array.Empty<IDictionary<string, object>>();
            }

            public IReadOnlyList<IDictionary<string, object>> GetServerPriv(
                SqlConnectionOptions options,
                ReconQueryFilters filters,
                CancellationToken cancellationToken = default)
            {
                return System.Array.Empty<IDictionary<string, object>>();
            }
        }
    }
}
