using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;

namespace PowerUpSQLSharp.Core.Commands
{
    public sealed class GetSqlConnectionTestThreadedCommand : ICommand
    {
        private readonly IConnectionTestService _connectionTestService;

        public GetSqlConnectionTestThreadedCommand(IConnectionTestService connectionTestService)
        {
            _connectionTestService = connectionTestService;
        }

        public string Name => "Get-SQLConnectionTestThreaded";

        public string Description =>
            "Tests SQL Server connectivity for multiple instances using concurrent connection attempts.";

        public Task<CommandResult> ExecuteAsync(
            CommandContext context,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var instances = ConnectionTestCommandHelper.ResolveInstances(context);
            var templateOptions = ConnectionTestCommandHelper.ResolveConnectionOptions(context, instances[0]);
            var threads = ResolveThreads(context);

            var results = _connectionTestService.TestConnectionsThreaded(
                instances,
                templateOptions,
                threads,
                cancellationToken);

            return Task.FromResult(ConnectionTestCommandHelper.BuildConnectionTestResult(
                results,
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
