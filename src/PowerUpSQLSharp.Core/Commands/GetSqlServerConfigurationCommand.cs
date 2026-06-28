using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;

namespace PowerUpSQLSharp.Core.Commands
{
    public sealed class GetSqlServerConfigurationCommand : ICommand
    {
        private readonly IReconnaissanceService _reconnaissanceService;

        public GetSqlServerConfigurationCommand(IReconnaissanceService reconnaissanceService)
        {
            _reconnaissanceService = reconnaissanceService;
        }

        public string Name => "Get-SQLServerConfiguration";

        public string Description =>
            "Returns SQL Server configuration settings from sp_configure on target instances.";

        public Task<CommandResult> ExecuteAsync(
            CommandContext context,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var instances = ConnectionTestCommandHelper.ResolveInstances(context);
            var templateOptions = ConnectionTestCommandHelper.ResolveConnectionOptions(context, instances[0]);
            var threads = ResolveThreads(context);

            var entries = _reconnaissanceService.GetServerConfigurationThreaded(
                instances,
                templateOptions,
                threads,
                cancellationToken);

            var successfulInstances = entries
                .Select(entry => entry.Instance)
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .Count();

            return Task.FromResult(ServerConfigurationCommandHelper.BuildConfigurationResult(
                entries.ToList(),
                instances.Count,
                successfulInstances,
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
