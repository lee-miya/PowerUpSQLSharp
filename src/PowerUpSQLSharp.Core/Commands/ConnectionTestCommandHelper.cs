using System;
using System.Collections.Generic;
using System.Linq;
using PowerUpSQLSharp.Core.Models;

namespace PowerUpSQLSharp.Core.Commands
{
    internal static class ConnectionTestCommandHelper
    {
        internal static List<string> ResolveInstances(CommandContext context)
        {
            if (CommandArgumentHelper.TryGet(context?.Arguments, "Instance", out var instance)
                && !string.IsNullOrWhiteSpace(instance))
            {
                return instance
                    .Split(',')
                    .Select(value => value.Trim())
                    .Where(value => !string.IsNullOrEmpty(value))
                    .ToList();
            }

            return new List<string> { Environment.MachineName };
        }

        internal static SqlConnectionOptions ResolveConnectionOptions(CommandContext context, string instance)
        {
            var options = new SqlConnectionOptions { Instance = instance };

            if (CommandArgumentHelper.TryGet(context?.Arguments, "Username", out var username))
            {
                options.Username = username;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "Password", out var password))
            {
                options.Password = password;
            }

            if (CommandArgumentHelper.TryGetBool(context?.Arguments, "DAC", out var dac))
            {
                options.Dac = dac;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "Database", out var database))
            {
                options.Database = database;
            }

            if (CommandArgumentHelper.TryGetInt(context?.Arguments, "TimeOut", out var timeout))
            {
                options.ConnectTimeoutSeconds = timeout;
            }
            else
            {
                options.ConnectTimeoutSeconds = context?.Global?.TimeoutSeconds ?? 30;
            }

            return options;
        }

        internal static int ResolveExitCode(IReadOnlyList<SqlConnectionTestResult> results)
        {
            if (results == null || results.Count == 0)
            {
                return ExitCodes.Success;
            }

            var accessibleCount = results.Count(result => result.Success);
            var failedCount = results.Count(result => !result.Success);

            if (accessibleCount == 0 && failedCount > 0)
            {
                return ExitCodes.ConnectionFailed;
            }

            if (failedCount > 0 && accessibleCount > 0)
            {
                return ExitCodes.PartialFailure;
            }

            return ExitCodes.Success;
        }

        internal static CommandResult BuildConnectionTestResult(
            IReadOnlyList<SqlConnectionTestResult> results,
            GlobalOptions options)
        {
            var formatted = CommandResultFormatter.FromRows(
                CommandResultFormatter.ToConnectionTestRows(results),
                options);

            formatted.ExitCode = ResolveExitCode(results);
            return formatted;
        }
    }
}
