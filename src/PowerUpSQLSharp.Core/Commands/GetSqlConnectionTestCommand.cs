using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;

namespace PowerUpSQLSharp.Core.Commands
{
    public sealed class GetSqlConnectionTestCommand : ICommand
    {
        private readonly IConnectionTestService _connectionTestService;

        public GetSqlConnectionTestCommand(IConnectionTestService connectionTestService)
        {
            _connectionTestService = connectionTestService;
        }

        public string Name => "Get-SQLConnectionTest";

        public string Description =>
            "Tests if the current Windows account or provided SQL Server login can connect to an instance.";

        public Task<CommandResult> ExecuteAsync(
            CommandContext context,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var instances = ConnectionTestCommandHelper.ResolveInstances(context);
            var results = new List<SqlConnectionTestResult>(instances.Count);

            foreach (var instance in instances)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var options = ConnectionTestCommandHelper.ResolveConnectionOptions(context, instance);
                results.Add(_connectionTestService.TestConnection(options, cancellationToken));
            }

            return Task.FromResult(ConnectionTestCommandHelper.BuildConnectionTestResult(
                results,
                context?.Global));
        }
    }
}
