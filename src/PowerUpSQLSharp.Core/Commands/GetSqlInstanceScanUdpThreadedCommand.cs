using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;

namespace PowerUpSQLSharp.Core.Commands
{
    public sealed class GetSqlInstanceScanUdpThreadedCommand : ICommand
    {
        private readonly IInstanceDiscoveryService _discoveryService;

        public GetSqlInstanceScanUdpThreadedCommand(IInstanceDiscoveryService discoveryService)
        {
            _discoveryService = discoveryService;
        }

        public string Name => "Get-SQLInstanceScanUDPThreaded";

        public string Description =>
            "Discover SQL Server instances on multiple hosts via concurrent SQL Browser UDP scans.";

        public Task<CommandResult> ExecuteAsync(
            CommandContext context,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var computerNames = GetSqlInstanceScanUdpCommand.ResolveComputerNames(context);
            if (computerNames.Count == 0)
            {
                return Task.FromResult(CommandResult.Failure(
                    ExitCodes.ArgumentError,
                    "ComputerName is required. Example: -ComputerName SQL01,SQL02"));
            }

            var timeout = GetSqlInstanceScanUdpCommand.ResolveUdpTimeout(context);
            var threads = ResolveThreads(context);

            var instances = _discoveryService.ScanUdpThreaded(
                computerNames,
                timeout,
                threads,
                cancellationToken);

            var result = CommandResultFormatter.FromRows(
                CommandResultFormatter.ToUdpRows(instances),
                context?.Global);

            return Task.FromResult(result);
        }

        private static int ResolveThreads(CommandContext context)
        {
            if (CommandArgumentHelper.TryGetInt(context?.Arguments, "Threads", out var threads))
            {
                return threads;
            }

            return context?.Global?.Threads ?? 10;
        }
    }
}
