using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PowerUpSQLSharp.Core.Models;

namespace PowerUpSQLSharp.Core.Commands
{
    internal static class CommandArgumentHelper
    {
        public static bool TryGetBool(IDictionary<string, string> arguments, string name, out bool value)
        {
            value = false;
            if (arguments == null || !TryGet(arguments, name, out var raw))
            {
                return false;
            }

            if (string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(raw, "1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(raw, name, StringComparison.OrdinalIgnoreCase))
            {
                value = true;
                return true;
            }

            if (string.Equals(raw, "false", StringComparison.OrdinalIgnoreCase)
                || string.Equals(raw, "0", StringComparison.OrdinalIgnoreCase))
            {
                value = true;
                return false;
            }

            value = true;
            return true;
        }

        public static bool TryGetInt(IDictionary<string, string> arguments, string name, out int value)
        {
            value = 0;
            return TryGet(arguments, name, out var raw) && int.TryParse(raw, out value);
        }

        public static bool TryGet(IDictionary<string, string> arguments, string name, out string value)
        {
            value = null;
            if (arguments == null)
            {
                return false;
            }

            if (arguments.TryGetValue(name, out value))
            {
                return !string.IsNullOrWhiteSpace(value);
            }

            var alt = name.Replace("-", string.Empty);
            return arguments.TryGetValue(alt, out value) && !string.IsNullOrWhiteSpace(value);
        }
    }

    internal static class CommandResultFormatter
    {
        public static CommandResult FromRows(
            IEnumerable<IDictionary<string, object>> rows,
            GlobalOptions options,
            string warning = null)
        {
            var materialized = rows?.ToList() ?? new List<IDictionary<string, object>>();
            if (materialized.Count == 0)
            {
                return CommandResult.Success(warning: warning);
            }

            var output = IsJsonFormat(options)
                ? FormatJson(materialized)
                : Utils.OutputFormatter.FormatTable(materialized, options);

            return CommandResult.Success(output, warning);
        }

        public static IEnumerable<IDictionary<string, object>> ToRows(IEnumerable<SqlInstance> instances)
        {
            foreach (var instance in instances ?? Enumerable.Empty<SqlInstance>())
            {
                yield return new Dictionary<string, object>
                {
                    { "ComputerName", instance.ComputerName ?? string.Empty },
                    { "Instance", instance.InstanceName ?? string.Empty },
                    { "ServerInstance", instance.ServerInstance ?? string.Empty },
                    { "Port", instance.Port.HasValue ? (object)instance.Port.Value : string.Empty },
                    { "Version", instance.Version ?? string.Empty },
                    { "IsClustered", instance.IsClustered }
                };
            }
        }

        public static IEnumerable<IDictionary<string, object>> ToBroadcastRows(IEnumerable<SqlInstance> instances)
        {
            foreach (var instance in instances ?? Enumerable.Empty<SqlInstance>())
            {
                yield return new Dictionary<string, object>
                {
                    { "ComputerName", instance.ComputerName ?? string.Empty },
                    { "Instance", instance.ServerInstance ?? string.Empty },
                    { "IsClustered", instance.IsClustered ? "Yes" : "No" },
                    { "Version", instance.Version ?? string.Empty }
                };
            }
        }

        public static IEnumerable<IDictionary<string, object>> ToFileRows(IEnumerable<SqlInstance> instances)
        {
            foreach (var instance in instances ?? Enumerable.Empty<SqlInstance>())
            {
                yield return new Dictionary<string, object>
                {
                    { "ComputerName", instance.ComputerName ?? string.Empty },
                    { "Instance", instance.ServerInstance ?? string.Empty }
                };
            }
        }

        public static IEnumerable<IDictionary<string, object>> ToDomainRows(
            IEnumerable<SqlDomainInstance> instances,
            bool includeIp)
        {
            foreach (var instance in instances ?? Enumerable.Empty<SqlDomainInstance>())
            {
                var row = new Dictionary<string, object>
                {
                    { "ComputerName", instance.ComputerName ?? string.Empty },
                    { "Instance", instance.ServerInstance ?? string.Empty },
                    { "DomainAccountSid", instance.DomainAccountSid ?? string.Empty },
                    { "DomainAccount", instance.DomainAccount ?? string.Empty },
                    { "DomainAccountCn", instance.DomainAccountCn ?? string.Empty },
                    { "Service", instance.Service ?? string.Empty },
                    { "Spn", instance.Spn ?? string.Empty },
                    { "LastLogon", instance.LastLogon ?? string.Empty },
                    { "Description", instance.Description ?? string.Empty }
                };

                if (includeIp)
                {
                    row["IPAddress"] = instance.IpAddress ?? string.Empty;
                }

                yield return row;
            }
        }

        public static IEnumerable<IDictionary<string, object>> ToDomainSummaryRows(IEnumerable<SqlDomainInstance> instances)
        {
            foreach (var instance in instances ?? Enumerable.Empty<SqlDomainInstance>())
            {
                yield return new Dictionary<string, object>
                {
                    { "ComputerName", instance.ComputerName ?? string.Empty },
                    { "Instance", instance.ServerInstance ?? string.Empty }
                };
            }
        }

        public static IEnumerable<IDictionary<string, object>> ToConnectionTestRows(
            IEnumerable<SqlConnectionTestResult> results)
        {
            foreach (var result in results ?? Enumerable.Empty<SqlConnectionTestResult>())
            {
                yield return new Dictionary<string, object>
                {
                    { "ComputerName", result.ComputerName ?? string.Empty },
                    { "Instance", result.Instance ?? string.Empty },
                    { "Status", result.Status ?? string.Empty },
                    { "Success", result.Success }
                };
            }
        }

        public static IEnumerable<IDictionary<string, object>> ToServerInfoRows(IEnumerable<SqlServerInfo> infos)
        {
            foreach (var info in infos ?? Enumerable.Empty<SqlServerInfo>())
            {
                if (info == null)
                {
                    continue;
                }

                yield return new Dictionary<string, object>
                {
                    { "ComputerName", info.ComputerName ?? string.Empty },
                    { "Instance", info.Instance ?? string.Empty },
                    { "DomainName", info.DomainName ?? string.Empty },
                    { "ServiceName", info.ServiceName ?? string.Empty },
                    { "ServiceAccount", info.ServiceAccount ?? string.Empty },
                    { "AuthenticationMode", info.AuthenticationMode ?? string.Empty },
                    { "Clustered", info.Clustered ?? string.Empty },
                    { "SQLServerVersionNumber", info.SqlServerVersionNumber ?? string.Empty },
                    { "SQLServerMajorVersion", info.SqlServerMajorVersion ?? string.Empty },
                    { "SQLServerEdition", info.SqlServerEdition ?? string.Empty },
                    { "SQLServerServicePack", info.SqlServerServicePack ?? string.Empty },
                    { "OSArchitecture", info.OsArchitecture ?? string.Empty },
                    { "OsMachineType", info.OsMachineType ?? string.Empty },
                    { "OSVersionName", info.OsVersionName ?? string.Empty },
                    { "OsVersionNumber", info.OsVersionNumber ?? string.Empty },
                    { "Currentlogin", info.CurrentLogin ?? string.Empty },
                    { "IsSysadmin", info.IsSysadmin ?? string.Empty },
                    { "XpCmdShellEnabled", info.XpCmdShellEnabled ?? string.Empty },
                    { "XpDirtreeEnabled", info.XpDirtreeEnabled ?? string.Empty },
                    { "XpFileexistEnabled", info.XpFileexistEnabled ?? string.Empty },
                    { "XpSubdirsEnabled", info.XpSubdirsEnabled ?? string.Empty },
                    { "ActiveSessions", info.ActiveSessions ?? string.Empty }
                };
            }
        }

        public static IEnumerable<IDictionary<string, object>> ToDefaultPasswordRows(
            IEnumerable<SqlDefaultPasswordMatch> matches)
        {
            foreach (var match in matches ?? Enumerable.Empty<SqlDefaultPasswordMatch>())
            {
                yield return new Dictionary<string, object>
                {
                    { "Computer", match.Computer ?? string.Empty },
                    { "Instance", match.Instance ?? string.Empty },
                    { "Username", match.Username ?? string.Empty },
                    { "Password", match.Password ?? string.Empty },
                    { "IsSysadmin", match.IsSysadmin ?? string.Empty }
                };
            }
        }

        public static IEnumerable<IDictionary<string, object>> ToLocalServiceRows(
            IEnumerable<SqlLocalService> services)
        {
            foreach (var service in services ?? Enumerable.Empty<SqlLocalService>())
            {
                yield return new Dictionary<string, object>
                {
                    { "ComputerName", service.ComputerName ?? string.Empty },
                    { "Instance", service.Instance ?? string.Empty },
                    { "ServiceDisplayName", service.ServiceDisplayName ?? string.Empty },
                    { "ServiceName", service.ServiceName ?? string.Empty },
                    { "ServicePath", service.ServicePath ?? string.Empty },
                    { "ServiceAccount", service.ServiceAccount ?? string.Empty },
                    { "ServiceState", service.ServiceState ?? string.Empty },
                    { "ServiceProcessId", service.ServiceProcessId ?? string.Empty }
                };
            }
        }

        public static IEnumerable<IDictionary<string, object>> ToSysadminRows(
            IEnumerable<SqlSysadminStatus> statuses)
        {
            foreach (var status in statuses ?? Enumerable.Empty<SqlSysadminStatus>())
            {
                if (status == null)
                {
                    continue;
                }

                yield return new Dictionary<string, object>
                {
                    { "ComputerName", status.ComputerName ?? string.Empty },
                    { "Instance", status.Instance ?? string.Empty },
                    { "IsSysadmin", status.IsSysadmin ?? string.Empty }
                };
            }
        }

        public static IEnumerable<IDictionary<string, object>> ToLocalAdminRows(SqlLocalAdminStatus status)
        {
            if (status == null)
            {
                yield break;
            }

            yield return new Dictionary<string, object>
            {
                { "CurrentUser", status.CurrentUser ?? string.Empty },
                { "IsLocalAdmin", status.IsLocalAdmin }
            };
        }

        public static IEnumerable<IDictionary<string, object>> ToServerConfigurationRows(
            IEnumerable<SqlServerConfigurationEntry> entries)
        {
            foreach (var entry in entries ?? Enumerable.Empty<SqlServerConfigurationEntry>())
            {
                if (entry == null)
                {
                    continue;
                }

                yield return new Dictionary<string, object>
                {
                    { "ComputerName", entry.ComputerName ?? string.Empty },
                    { "Instance", entry.Instance ?? string.Empty },
                    { "Name", entry.Name ?? string.Empty },
                    { "Minimum", entry.Minimum ?? string.Empty },
                    { "Maximum", entry.Maximum ?? string.Empty },
                    { "config_value", entry.ConfigValue ?? string.Empty },
                    { "run_value", entry.RunValue ?? string.Empty }
                };
            }
        }

        public static IEnumerable<IDictionary<string, object>> ToUdpRows(IEnumerable<SqlInstance> instances)
        {
            foreach (var instance in instances ?? Enumerable.Empty<SqlInstance>())
            {
                yield return new Dictionary<string, object>
                {
                    { "ComputerName", instance.ComputerName ?? string.Empty },
                    { "Instance", instance.ServerInstance ?? string.Empty },
                    { "InstanceName", instance.InstanceName ?? string.Empty },
                    { "ServerIP", instance.ServerIp ?? string.Empty },
                    { "TCPPort", instance.Port.HasValue ? (object)instance.Port.Value : string.Empty },
                    { "BaseVersion", instance.Version ?? string.Empty },
                    { "IsClustered", instance.IsClustered ? "Yes" : "No" }
                };
            }
        }

        private static bool IsJsonFormat(GlobalOptions options)
        {
            return options != null
                && string.Equals(options.Format, "json", StringComparison.OrdinalIgnoreCase);
        }

        private static string FormatJson(IReadOnlyList<IDictionary<string, object>> rows)
        {
            var sb = new StringBuilder();
            sb.Append('[');

            for (var i = 0; i < rows.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(',');
                }

                sb.Append('{');
                var first = true;
                foreach (var pair in rows[i])
                {
                    if (!first)
                    {
                        sb.Append(',');
                    }

                    first = false;
                    sb.Append('"').Append(EscapeJson(pair.Key)).Append("\":");
                    AppendJsonValue(sb, pair.Value);
                }

                sb.Append('}');
            }

            sb.Append(']');
            return sb.ToString();
        }

        private static void AppendJsonValue(StringBuilder sb, object value)
        {
            if (value == null)
            {
                sb.Append("null");
                return;
            }

            switch (value)
            {
                case bool boolValue:
                    sb.Append(boolValue ? "true" : "false");
                    break;
                case int intValue:
                    sb.Append(intValue);
                    break;
                case long longValue:
                    sb.Append(longValue);
                    break;
                default:
                    sb.Append('"').Append(EscapeJson(value.ToString())).Append('"');
                    break;
            }
        }

        private static string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
        }
    }
}
