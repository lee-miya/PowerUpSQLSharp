using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;

namespace PowerUpSQLSharp.Core.Commands
{
    public sealed class GetSqlInstanceFileCommand : ICommand
    {
        private readonly IInstanceDiscoveryService _discoveryService;

        public GetSqlInstanceFileCommand(IInstanceDiscoveryService discoveryService)
        {
            _discoveryService = discoveryService;
        }

        public string Name => "Get-SQLInstanceFile";

        public string Description =>
            "Load SQL Server instances from a text file (one per line: computer, computer\\instance, or computer,port).";

        public Task<CommandResult> ExecuteAsync(
            CommandContext context,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!CommandArgumentHelper.TryGet(context?.Arguments, "FilePath", out var filePath)
                || string.IsNullOrWhiteSpace(filePath))
            {
                return Task.FromResult(CommandResult.Failure(
                    ExitCodes.ArgumentError,
                    "FilePath is required. Example: -FilePath servers.txt"));
            }

            if (!File.Exists(filePath))
            {
                return Task.FromResult(CommandResult.Failure(
                    ExitCodes.ArgumentError,
                    "File path does not appear to be valid."));
            }

            var instances = _discoveryService.GetInstancesFromFile(filePath);
            var result = CommandResultFormatter.FromRows(
                CommandResultFormatter.ToFileRows(instances),
                context?.Global);

            return Task.FromResult(result);
        }
    }
}
