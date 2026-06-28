using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;

namespace PowerUpSQLSharp.Core.Commands
{
    public sealed class GetSqlServerInfoCommand : ICommand
    {
        private readonly IReconnaissanceService _reconnaissanceService;

        public GetSqlServerInfoCommand(IReconnaissanceService reconnaissanceService)
        {
            _reconnaissanceService = reconnaissanceService;
        }

        public string Name => "Get-SQLServerInfo";

        public string Description =>
            "Returns basic server and user information from target SQL Server instances.";

        public Task<CommandResult> ExecuteAsync(
            CommandContext context,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var instances = ConnectionTestCommandHelper.ResolveInstances(context);
            var results = new List<SqlServerInfo>(instances.Count);

            foreach (var instance in instances)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var options = ConnectionTestCommandHelper.ResolveConnectionOptions(context, instance);
                var info = _reconnaissanceService.GetServerInfo(options, cancellationToken);
                if (info != null)
                {
                    results.Add(info);
                }
            }

            return Task.FromResult(ServerInfoCommandHelper.BuildServerInfoResult(
                results,
                instances.Count,
                context?.Global));
        }
    }
}
