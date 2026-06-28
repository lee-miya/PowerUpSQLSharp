using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Utils;

namespace PowerUpSQLSharp.Core.Commands
{
    public sealed class GetSqlServiceLocalCommand : ICommand
    {
        public string Name => "Get-SQLServiceLocal";

        public string Description =>
            "Returns local SQL Server services via WMI (Win32_Service). Local machine only.";

        public Task<CommandResult> ExecuteAsync(
            CommandContext context,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            CommandArgumentHelper.TryGet(context?.Arguments, "Instance", out var instanceFilter);
            var runOnly = CommandArgumentHelper.TryGetBool(context?.Arguments, "RunOnly", out var runOnlyValue)
                && runOnlyValue;

            var services = SqlLocalServiceReader.ReadLocalServices(instanceFilter, runOnly);
            return Task.FromResult(CommandResultFormatter.FromRows(
                CommandResultFormatter.ToLocalServiceRows(services),
                context?.Global));
        }
    }
}
