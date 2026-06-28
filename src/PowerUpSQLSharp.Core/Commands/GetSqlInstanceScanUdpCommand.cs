using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;
using PowerUpSQLSharp.Core.Utils;

namespace PowerUpSQLSharp.Core.Commands
{
    public sealed class GetSqlInstanceScanUdpCommand : ICommand
    {
        private readonly IInstanceDiscoveryService _discoveryService;

        public GetSqlInstanceScanUdpCommand(IInstanceDiscoveryService discoveryService)
        {
            _discoveryService = discoveryService;
        }

        public string Name => "Get-SQLInstanceScanUDP";

        public string Description =>
            "Discover SQL Server instances on a remote host via SQL Browser UDP (port 1434).";

        public Task<CommandResult> ExecuteAsync(
            CommandContext context,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var computerNames = ResolveComputerNames(context);
            if (computerNames.Count == 0)
            {
                return Task.FromResult(CommandResult.Failure(
                    ExitCodes.ArgumentError,
                    "ComputerName is required. Example: -ComputerName SQL01"));
            }

            var timeout = ResolveUdpTimeout(context);
            var instances = new List<SqlInstance>();

            foreach (var computerName in computerNames)
            {
                cancellationToken.ThrowIfCancellationRequested();
                OutputFormatter.WriteLine($"Scanning UDP 1434 on {computerName}...", context?.Global);
                instances.AddRange(_discoveryService.ScanUdp(computerName, timeout, cancellationToken));
            }

            var result = CommandResultFormatter.FromRows(
                CommandResultFormatter.ToUdpRows(instances),
                context?.Global);

            return Task.FromResult(result);
        }

        internal static List<string> ResolveComputerNames(CommandContext context)
        {
            if (CommandArgumentHelper.TryGet(context?.Arguments, "ComputerName", out var value)
                && !string.IsNullOrWhiteSpace(value))
            {
                return value
                    .Split(',')
                    .Select(name => name.Trim())
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList();
            }

            return new List<string>();
        }

        internal static int ResolveUdpTimeout(CommandContext context)
        {
            if (CommandArgumentHelper.TryGetInt(context?.Arguments, "UDPTimeOut", out var udpTimeout))
            {
                return udpTimeout;
            }

            return 2;
        }
    }
}
