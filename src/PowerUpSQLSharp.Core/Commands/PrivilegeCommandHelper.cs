using System.Collections.Generic;
using System.Linq;
using PowerUpSQLSharp.Core.Models;

namespace PowerUpSQLSharp.Core.Commands
{
    internal static class SysadminCommandHelper
    {
        internal static CommandResult BuildSysadminResult(
            IReadOnlyList<SqlSysadminStatus> results,
            int requestedCount,
            GlobalOptions options)
        {
            var formatted = CommandResultFormatter.FromRows(
                CommandResultFormatter.ToSysadminRows(results),
                options);

            formatted.ExitCode = ServerInfoCommandHelper.ResolveExitCode(requestedCount, results?.Count ?? 0);
            return formatted;
        }
    }

    internal static class ServerConfigurationCommandHelper
    {
        internal static CommandResult BuildConfigurationResult(
            IReadOnlyList<SqlServerConfigurationEntry> entries,
            int requestedCount,
            int successfulInstanceCount,
            GlobalOptions options)
        {
            var formatted = CommandResultFormatter.FromRows(
                CommandResultFormatter.ToServerConfigurationRows(entries),
                options);

            formatted.ExitCode = ResolveExitCode(requestedCount, successfulInstanceCount);
            return formatted;
        }

        private static int ResolveExitCode(int requestedCount, int successfulInstanceCount)
        {
            if (requestedCount <= 0)
            {
                return ExitCodes.Success;
            }

            if (successfulInstanceCount == 0)
            {
                return ExitCodes.ConnectionFailed;
            }

            if (successfulInstanceCount < requestedCount)
            {
                return ExitCodes.PartialFailure;
            }

            return ExitCodes.Success;
        }
    }
}
