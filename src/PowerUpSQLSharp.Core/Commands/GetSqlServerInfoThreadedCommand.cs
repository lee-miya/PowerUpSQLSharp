using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;

namespace PowerUpSQLSharp.Core.Commands
{
    public sealed class GetSqlServerInfoThreadedCommand : ICommand
    {
        private readonly IReconnaissanceService _reconnaissanceService;

        public GetSqlServerInfoThreadedCommand(IReconnaissanceService reconnaissanceService)
        {
            _reconnaissanceService = reconnaissanceService;
        }

        public string Name => "Get-SQLServerInfoThreaded";

        public string Description =>
            "Returns basic server and user information from multiple SQL Server instances using concurrent queries.";

        public Task<CommandResult> ExecuteAsync(
            CommandContext context,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var instances = ConnectionTestCommandHelper.ResolveInstances(context);
            var templateOptions = ConnectionTestCommandHelper.ResolveConnectionOptions(context, instances[0]);
            var threads = ResolveThreads(context);

            var results = _reconnaissanceService.GetServerInfoThreaded(
                instances,
                templateOptions,
                threads,
                cancellationToken);

            return Task.FromResult(ServerInfoCommandHelper.BuildServerInfoResult(
                results,
                instances.Count,
                context?.Global));
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
