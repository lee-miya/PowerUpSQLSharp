using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;

namespace PowerUpSQLSharp.Core.Commands
{
    public sealed class GetSqlServerLoginDefaultPwCommand : ICommand
    {
        private readonly IConnectionTestService _connectionTestService;

        public GetSqlServerLoginDefaultPwCommand(IConnectionTestService connectionTestService)
        {
            _connectionTestService = connectionTestService;
        }

        public string Name => "Get-SQLServerLoginDefaultPw";

        public string Description =>
            "Tests whether a named SQL Server instance matches known default credentials from product-specific databases.";

        public Task<CommandResult> ExecuteAsync(
            CommandContext context,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var instances = ConnectionTestCommandHelper.ResolveInstances(context);
            var timeout = ResolveTimeout(context);
            var matches = new List<SqlDefaultPasswordMatch>();

            foreach (var instance in instances)
            {
                cancellationToken.ThrowIfCancellationRequested();
                matches.AddRange(_connectionTestService.TestDefaultPasswords(
                    instance,
                    timeout,
                    cancellationToken));
            }

            var result = CommandResultFormatter.FromRows(
                CommandResultFormatter.ToDefaultPasswordRows(matches),
                context?.Global);

            return Task.FromResult(result);
        }

        private static int ResolveTimeout(CommandContext context)
        {
            if (CommandArgumentHelper.TryGetInt(context?.Arguments, "TimeOut", out var timeout))
            {
                return timeout;
            }

            return context?.Global?.TimeoutSeconds ?? 30;
        }
    }
}
