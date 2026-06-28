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

    public class GetSqlServerInfoCommandTests

    {

        [Fact]

        public async Task ExecuteAsync_SuccessfulQuery_ReturnsServerInfoFields()

        {

            var service = new FakeReconnaissanceService(new[]

            {

                new SqlServerInfo

                {

                    ComputerName = "SQL01",

                    Instance = @"SQL01\MSSQL",

                    DomainName = "DOMAIN",

                    SqlServerVersionNumber = "15.0.2000.5",

                    CurrentLogin = @"DOMAIN\user",

                    IsSysadmin = "Yes",

                    XpCmdShellEnabled = "No",

                    XpDirtreeEnabled = "Yes",

                    XpFileexistEnabled = "Yes",

                    XpSubdirsEnabled = "No",

                    ActiveSessions = "3"

                }

            });



            var command = new GetSqlServerInfoCommand(service);

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

            Assert.Contains("SQLServerVersionNumber", result.Output);

            Assert.Contains("15.0.2000.5", result.Output);

            Assert.Contains("Currentlogin", result.Output);

            Assert.Contains("IsSysadmin", result.Output);

            Assert.Contains("Yes", result.Output);

            Assert.Contains("XpCmdShellEnabled", result.Output);

            Assert.Contains("XpDirtreeEnabled", result.Output);

            Assert.Contains("XpFileexistEnabled", result.Output);

            Assert.Contains("XpSubdirsEnabled", result.Output);

            Assert.Contains("ActiveSessions", result.Output);

        }



        [Fact]

        public async Task ExecuteAsync_ConnectionFailed_ReturnsConnectionFailedExitCode()

        {

            var service = new FakeReconnaissanceService(new SqlServerInfo[0]);



            var command = new GetSqlServerInfoCommand(service);

            var context = new CommandContext

            {

                Arguments = new Dictionary<string, string> { { "Instance", @"SQL01\MSSQL" } }

            };



            var result = await command.ExecuteAsync(context).ConfigureAwait(false);



            Assert.Equal(ExitCodes.ConnectionFailed, result.ExitCode);

        }



        [Fact]

        public async Task ExecuteAsync_JsonFormat_IncludesVersionField()

        {

            var service = new FakeReconnaissanceService(new[]

            {

                new SqlServerInfo

                {

                    ComputerName = "SQL01",

                    Instance = @"SQL01\MSSQL",

                    SqlServerVersionNumber = "15.0.2000.5",

                    CurrentLogin = "sa",

                    IsSysadmin = "No",

                    XpCmdShellEnabled = "Yes",

                    XpDirtreeEnabled = "No",

                    XpFileexistEnabled = "No",

                    XpSubdirsEnabled = "Yes"

                }

            });



            var command = new GetSqlServerInfoCommand(service);

            var context = new CommandContext

            {

                Global = new GlobalOptions { Format = "json" },

                Arguments = new Dictionary<string, string> { { "Instance", @"SQL01\MSSQL" } }

            };



            var result = await command.ExecuteAsync(context).ConfigureAwait(false);



            Assert.Contains("\"SQLServerVersionNumber\":\"15.0.2000.5\"", result.Output);

            Assert.Contains("\"Currentlogin\":\"sa\"", result.Output);

            Assert.Contains("\"IsSysadmin\":\"No\"", result.Output);
            Assert.Contains("\"XpCmdShellEnabled\":\"Yes\"", result.Output);
            Assert.Contains("\"XpDirtreeEnabled\":\"No\"", result.Output);
            Assert.Contains("\"XpSubdirsEnabled\":\"Yes\"", result.Output);

        }



        private sealed class FakeReconnaissanceService : IReconnaissanceService

        {

            private readonly IReadOnlyList<SqlServerInfo> _results;

            private int _callIndex;



            public FakeReconnaissanceService(IReadOnlyList<SqlServerInfo> results)

            {

                _results = results ?? new SqlServerInfo[0];

            }



            public SqlServerInfo GetServerInfo(

                SqlConnectionOptions options,

                CancellationToken cancellationToken = default)

            {

                if (_results.Count == 0)

                {

                    return null;

                }



                return _results[Math.Min(_callIndex++, _results.Count - 1)];

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

