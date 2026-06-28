using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;

namespace PowerUpSQLSharp.Core.Commands
{
    public sealed class GetSqlInstanceLocalCommand : ICommand
    {
        private readonly IInstanceDiscoveryService _discoveryService;

        public GetSqlInstanceLocalCommand(IInstanceDiscoveryService discoveryService)
        {
            _discoveryService = discoveryService;
        }

        public string Name => "Get-SQLInstanceLocal";

        public string Description =>
            "Discover SQL Server instances installed on the local machine via registry enumeration.";

        public Task<CommandResult> ExecuteAsync(
            CommandContext context,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var instances = _discoveryService.GetLocalInstances();
            var result = CommandResultFormatter.FromRows(
                CommandResultFormatter.ToRows(instances),
                context?.Global);

            return Task.FromResult(result);
        }
    }
}
