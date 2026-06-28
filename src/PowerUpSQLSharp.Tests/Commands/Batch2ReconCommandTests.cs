using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Commands;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;
using PowerUpSQLSharp.Tests.TestDoubles;
using Xunit;

namespace PowerUpSQLSharp.Tests.Commands
{
    public class GetSqlDatabaseCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_ReturnsDatabaseRows()
        {
            var service = new FakeReconnaissanceService(queryResults: new List<IDictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "ComputerName", "SQL01" },
                    { "Instance", @"SQL01\MSSQL" },
                    { "DatabaseName", "testdb" },
                    { "DatabaseOwner", "sa" }
                }
            });

            var command = new GetSqlDatabaseCommand(service);
            var context = new CommandContext
            {
                Arguments = new Dictionary<string, string>
                {
                    { "Instance", @"SQL01\MSSQL" }
                }
            };

            var result = await command.ExecuteAsync(context).ConfigureAwait(false);

            Assert.Equal(ExitCodes.Success, result.ExitCode);
            Assert.Contains("testdb", result.Output);
            Assert.Contains("DatabaseName", result.Output);
        }

        [Fact]
        public async Task ExecuteAsync_NoAccessibleInstances_ReturnsConnectionFailedExitCode()
        {
            var service = new FakeReconnaissanceService(queryResults: new List<IDictionary<string, object>>());
            var command = new GetSqlDatabaseCommand(service);
            var context = new CommandContext
            {
                Arguments = new Dictionary<string, string> { { "Instance", @"SQL01\MSSQL" } }
            };

            var result = await command.ExecuteAsync(context).ConfigureAwait(false);

            Assert.Equal(ExitCodes.ConnectionFailed, result.ExitCode);
        }
    }

    public class GetSqlQueryCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_ReturnsQueryRows()
        {
            var service = new FakeReconnaissanceService(queryResults: new List<IDictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "ComputerName", "SQL01" },
                    { "Instance", @"SQL01\MSSQL" },
                    { "Column1", "value" }
                }
            });

            var command = new GetSqlQueryCommand(service);
            var context = new CommandContext
            {
                Arguments = new Dictionary<string, string>
                {
                    { "Instance", @"SQL01\MSSQL" },
                    { "Query", "SELECT 1 AS Column1" }
                }
            };

            var result = await command.ExecuteAsync(context).ConfigureAwait(false);

            Assert.Equal(ExitCodes.Success, result.ExitCode);
            Assert.Contains("value", result.Output);
        }
    }

    public class SqlReconQueryBuilderTests
    {
        [Fact]
        public void BuildDatabaseQuery_IncludesHasAccessFilter()
        {
            var query = PowerUpSQLSharp.Core.Utils.SqlReconQueryBuilder.BuildDatabaseQuery(
                "SQL01",
                @"SQL01\MSSQL",
                new ReconQueryFilters { HasAccess = true, NoDefaults = true },
                15);

            Assert.Contains("HAS_DBACCESS(a.name)=1", query);
            Assert.Contains("master", query);
        }
    }
}
