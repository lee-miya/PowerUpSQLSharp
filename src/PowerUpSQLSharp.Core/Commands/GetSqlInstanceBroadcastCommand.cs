using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;

namespace PowerUpSQLSharp.Core.Commands
{
    public sealed class GetSqlInstanceBroadcastCommand : ICommand
    {
        private readonly IInstanceDiscoveryService _discoveryService;

        public GetSqlInstanceBroadcastCommand(IInstanceDiscoveryService discoveryService)
        {
            _discoveryService = discoveryService;
        }

        public string Name => "Get-SQLInstanceBroadcast";

        public string Description =>
            "Discover SQL Server instances on the local network via SQL Browser broadcast (SqlDataSourceEnumerator).";

        public Task<CommandResult> ExecuteAsync(
            CommandContext context,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var instances = _discoveryService.GetBroadcastInstances();
            var udpPing = CommandArgumentHelper.TryGetBool(context?.Arguments, "UDPPing", out var ping) && ping;
            var timeout = ResolveUdpTimeout(context);

            if (udpPing && instances.Count > 0)
            {
                var computerNames = instances
                    .Select(i => i.ComputerName)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(System.StringComparer.OrdinalIgnoreCase);

                var udpResults = _discoveryService.ScanUdpThreaded(
                    computerNames,
                    timeout,
                    context?.Global?.Threads ?? 10,
                    cancellationToken);

                var result = CommandResultFormatter.FromRows(
                    CommandResultFormatter.ToUdpRows(udpResults),
                    context?.Global);

                return Task.FromResult(result);
            }

            var broadcastResult = CommandResultFormatter.FromRows(
                CommandResultFormatter.ToBroadcastRows(instances),
                context?.Global);

            return Task.FromResult(broadcastResult);
        }

        private static int ResolveUdpTimeout(CommandContext context)
        {
            if (CommandArgumentHelper.TryGetInt(context?.Arguments, "UDPTimeOut", out var udpTimeout))
            {
                return udpTimeout;
            }

            return context?.Global?.TimeoutSeconds ?? 30;
        }
    }
}
