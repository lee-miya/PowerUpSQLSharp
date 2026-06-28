using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;

namespace PowerUpSQLSharp.Core.Commands
{
    public sealed class GetSqlSysadminCheckCommand : ICommand
    {
        private readonly IPrivilegeInspectionService _privilegeInspectionService;

        public GetSqlSysadminCheckCommand(IPrivilegeInspectionService privilegeInspectionService)
        {
            _privilegeInspectionService = privilegeInspectionService;
        }

        public string Name => "Get-SQLSysadminCheck";

        public string Description =>
            "Checks whether the authenticated login has sysadmin privileges on target SQL Server instances.";

        public Task<CommandResult> ExecuteAsync(
            CommandContext context,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var instances = ConnectionTestCommandHelper.ResolveInstances(context);
            var results = new List<SqlSysadminStatus>(instances.Count);

            foreach (var instance in instances)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var options = ConnectionTestCommandHelper.ResolveConnectionOptions(context, instance);
                var status = _privilegeInspectionService.GetSysadminStatus(options, cancellationToken);
                if (status != null)
                {
                    results.Add(status);
                }
            }

            return Task.FromResult(SysadminCommandHelper.BuildSysadminResult(
                results,
                instances.Count,
                context?.Global));
        }
    }
}
