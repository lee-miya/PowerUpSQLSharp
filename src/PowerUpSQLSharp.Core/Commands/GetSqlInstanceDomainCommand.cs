using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;

namespace PowerUpSQLSharp.Core.Commands
{
    public sealed class GetSqlInstanceDomainCommand : ICommand
    {
        private readonly IInstanceDiscoveryService _discoveryService;

        public GetSqlInstanceDomainCommand(IInstanceDiscoveryService discoveryService)
        {
            _discoveryService = discoveryService;
        }

        public string Name => "Get-SQLInstanceDomain";

        public string Description =>
            "Discover SQL Server instances by querying domain LDAP for MSSQL service principal names (SPNs).";

        public Task<CommandResult> ExecuteAsync(
            CommandContext context,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var query = BuildQuery(context);
            var discovery = _discoveryService.GetDomainInstances(query, cancellationToken);

            var rows = discovery.SummaryOnly
                ? CommandResultFormatter.ToDomainSummaryRows(discovery.Instances)
                : CommandResultFormatter.ToDomainRows(discovery.Instances, query.IncludeIp);

            var result = CommandResultFormatter.FromRows(rows, context?.Global, discovery.Warning);
            return Task.FromResult(result);
        }

        internal static DomainInstanceQuery BuildQuery(CommandContext context)
        {
            var query = new DomainInstanceQuery
            {
                Ldap = new DomainLdapOptions(),
                Threads = context?.Global?.Threads ?? 10
            };

            if (CommandArgumentHelper.TryGet(context?.Arguments, "Username", out var username))
            {
                query.Ldap.Username = username;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "Password", out var password))
            {
                query.Ldap.Password = password;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "DomainController", out var domainController))
            {
                query.Ldap.DomainController = domainController;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "ComputerName", out var computerName))
            {
                query.Ldap.ComputerName = computerName;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "DomainAccount", out var domainAccount))
            {
                query.Ldap.DomainAccount = domainAccount;
            }

            if (CommandArgumentHelper.TryGetBool(context?.Arguments, "CheckMgmt", out var checkMgmt))
            {
                query.CheckMgmt = checkMgmt;
            }

            if (CommandArgumentHelper.TryGetBool(context?.Arguments, "IncludeIP", out var includeIp))
            {
                query.IncludeIp = includeIp;
            }

            if (CommandArgumentHelper.TryGetInt(context?.Arguments, "UDPTimeOut", out var udpTimeout))
            {
                query.UdpTimeoutSeconds = udpTimeout;
            }

            if (CommandArgumentHelper.TryGetInt(context?.Arguments, "Threads", out var threads))
            {
                query.Threads = threads;
            }

            if (context?.Global != null)
            {
                query.Ldap.TimeoutSeconds = context.Global.TimeoutSeconds;
            }

            return query;
        }
    }
}
