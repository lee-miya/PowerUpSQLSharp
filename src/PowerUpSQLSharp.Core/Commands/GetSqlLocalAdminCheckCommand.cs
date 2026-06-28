using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;

namespace PowerUpSQLSharp.Core.Commands
{
    public sealed class GetSqlLocalAdminCheckCommand : ICommand
    {
        private readonly IPrivilegeInspectionService _privilegeInspectionService;

        public GetSqlLocalAdminCheckCommand(IPrivilegeInspectionService privilegeInspectionService)
        {
            _privilegeInspectionService = privilegeInspectionService;
        }

        public string Name => "Get-SQLLocalAdminCheck";

        public string Description =>
            "Checks whether the current Windows user is running in a local administrator context.";

        public Task<CommandResult> ExecuteAsync(
            CommandContext context,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var status = _privilegeInspectionService.CheckLocalAdministrator();
            return Task.FromResult(CommandResultFormatter.FromRows(
                CommandResultFormatter.ToLocalAdminRows(status),
                context?.Global));
        }
    }
}
