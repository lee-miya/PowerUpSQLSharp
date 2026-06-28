using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Commands;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;
using Xunit;

namespace PowerUpSQLSharp.Tests.Commands
{
    public class GetSqlInstanceDomainCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_ReturnsDomainColumns()
        {
            var service = new FakeDiscoveryService(new DomainDiscoveryResult
            {
                Instances = new[]
                {
                    new SqlDomainInstance
                    {
                        ComputerName = "SQL01.example.com",
                        ServerInstance = "SQL01.example.com",
                        DomainAccount = "SQL01$",
                        DomainAccountCn = "SQL01",
                        Service = "MSSQLSvc",
                        Spn = "MSSQLSvc/SQL01.example.com"
                    }
                }
            });

            var command = new GetSqlInstanceDomainCommand(service);
            var result = await command.ExecuteAsync(new CommandContext()).ConfigureAwait(false);

            Assert.Equal(ExitCodes.Success, result.ExitCode);
            Assert.Contains("SQL01.example.com", result.Output);
            Assert.Contains("MSSQLSvc", result.Output);
            Assert.Contains("DomainAccount", result.Output);
        }

        [Fact]
        public async Task ExecuteAsync_NonDomainWarning_StillReturnsSuccess()
        {
            var service = new FakeDiscoveryService(new DomainDiscoveryResult
            {
                Warning = "Domain LDAP query failed: not joined to a domain"
            });

            var command = new GetSqlInstanceDomainCommand(service);
            var result = await command.ExecuteAsync(new CommandContext()).ConfigureAwait(false);

            Assert.Equal(ExitCodes.Success, result.ExitCode);
            Assert.Contains("Domain LDAP query failed", result.Warning);
        }

        [Fact]
        public async Task ExecuteAsync_CheckMgmt_ReturnsSummaryColumnsOnly()
        {
            var service = new FakeDiscoveryService(new DomainDiscoveryResult
            {
                SummaryOnly = true,
                Instances = new[]
                {
                    new SqlDomainInstance
                    {
                        ComputerName = "SQL01.example.com",
                        ServerInstance = @"SQL01.example.com\SQLEXPRESS"
                    }
                }
            });

            var command = new GetSqlInstanceDomainCommand(service);
            var context = new CommandContext
            {
                Arguments = new Dictionary<string, string> { { "CheckMgmt", "true" } }
            };

            var result = await command.ExecuteAsync(context).ConfigureAwait(false);

            Assert.Equal(ExitCodes.Success, result.ExitCode);
            Assert.Contains("SQLEXPRESS", result.Output);
            Assert.DoesNotContain("DomainAccount", result.Output);
        }

        [Fact]
        public void BuildQuery_MapsArguments()
        {
            var context = new CommandContext
            {
                Global = new GlobalOptions { Threads = 12, TimeoutSeconds = 45 },
                Arguments = new Dictionary<string, string>
                {
                    { "DomainController", "dc01.example.com" },
                    { "Username", @"DOMAIN\user" },
                    { "Password", "secret" },
                    { "IncludeIP", "true" },
                    { "UDPTimeOut", "4" }
                }
            };

            var query = GetSqlInstanceDomainCommand.BuildQuery(context);

            Assert.Equal("dc01.example.com", query.Ldap.DomainController);
            Assert.Equal(@"DOMAIN\user", query.Ldap.Username);
            Assert.Equal("secret", query.Ldap.Password);
            Assert.True(query.IncludeIp);
            Assert.Equal(4, query.UdpTimeoutSeconds);
            Assert.Equal(45, query.Ldap.TimeoutSeconds);
        }

        private sealed class FakeDiscoveryService : IInstanceDiscoveryService
        {
            private readonly DomainDiscoveryResult _domainResult;

            public FakeDiscoveryService(DomainDiscoveryResult domainResult)
            {
                _domainResult = domainResult;
            }

            public IReadOnlyList<SqlInstance> GetLocalInstances() => new SqlInstance[0];

            public IReadOnlyList<SqlInstance> GetBroadcastInstances() => new SqlInstance[0];

            public IReadOnlyList<SqlInstance> GetInstancesFromFile(string filePath) => new SqlInstance[0];

            public IReadOnlyList<SqlInstance> ScanUdp(
                string computerName,
                int timeoutSeconds,
                CancellationToken cancellationToken = default) => new SqlInstance[0];

            public IReadOnlyList<SqlInstance> ScanUdpThreaded(
                IEnumerable<string> computerNames,
                int timeoutSeconds,
                int maxThreads,
                CancellationToken cancellationToken = default) => new SqlInstance[0];

            public DomainDiscoveryResult GetDomainInstances(
                DomainInstanceQuery query,
                CancellationToken cancellationToken = default)
            {
                return _domainResult;
            }
        }
    }
}
