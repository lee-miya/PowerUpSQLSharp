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

    public class GetSqlServerInfoThreadedCommandTests

    {

        [Fact]

        public async Task ExecuteAsync_PartialSuccess_ReturnsPartialFailureExitCode()

        {

            var service = new FakeReconnaissanceService(new[]

            {

                new SqlServerInfo

                {

                    ComputerName = "SQL01",

                    Instance = @"SQL01\MSSQL",

                    SqlServerVersionNumber = "15.0.2000.5",

                    IsSysadmin = "Yes"

                }

            });



            var command = new GetSqlServerInfoThreadedCommand(service);

            var context = new CommandContext

            {

                Arguments = new Dictionary<string, string>

                {

                    { "Instance", @"SQL01\MSSQL,SQL02\MSSQL" },

                    { "Threads", "2" }

                }

            };



            var result = await command.ExecuteAsync(context).ConfigureAwait(false);



            Assert.Equal(ExitCodes.PartialFailure, result.ExitCode);

            Assert.Contains("SQL01", result.Output);

        }



        [Fact]

        public async Task ExecuteAsync_AllInstancesFail_ReturnsConnectionFailedExitCode()

        {

            var service = new FakeReconnaissanceService(new SqlServerInfo[0]);



            var command = new GetSqlServerInfoThreadedCommand(service);

            var context = new CommandContext

            {

                Arguments = new Dictionary<string, string>

                {

                    { "Instance", @"SQL01\MSSQL,SQL02\MSSQL" }

                }

            };



            var result = await command.ExecuteAsync(context).ConfigureAwait(false);



            Assert.Equal(ExitCodes.ConnectionFailed, result.ExitCode);

        }



        private sealed class FakeReconnaissanceService : IReconnaissanceService

        {

            private readonly IReadOnlyList<SqlServerInfo> _results;



            public FakeReconnaissanceService(IReadOnlyList<SqlServerInfo> results)

            {

                _results = results ?? new SqlServerInfo[0];

            }



            public SqlServerInfo GetServerInfo(

                SqlConnectionOptions options,

                CancellationToken cancellationToken = default)

            {

                return _results.Count > 0 ? _results[0] : null;

            }



            public IReadOnlyList<SqlServerInfo> GetServerInfoThreaded(

                IEnumerable<string> instances,

                SqlConnectionOptions templateOptions,

                int maxThreads,

                CancellationToken cancellationToken = default)

            {

                return _results;

            }

            public IReadOnlyList<SqlServerConfigurationEntry> GetServerConfiguration(
                SqlConnectionOptions options,
                CancellationToken cancellationToken = default)
            {
                return Array.Empty<SqlServerConfigurationEntry>();
            }

            public IReadOnlyList<SqlServerConfigurationEntry> GetServerConfigurationThreaded(
                IEnumerable<string> instances,
                SqlConnectionOptions templateOptions,
                int maxThreads,
                CancellationToken cancellationToken = default)
            {
                return Array.Empty<SqlServerConfigurationEntry>();
            }

        }

    }

}

