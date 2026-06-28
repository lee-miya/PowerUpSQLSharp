using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using PowerUpSQLSharp.Core.Models;

namespace PowerUpSQLSharp.Core.Utils
{
    internal static class SqlLocalServiceReader
    {
        public static IReadOnlyList<SqlLocalService> ReadLocalServices(string instanceFilter, bool runOnly)
        {
            var results = new List<SqlLocalService>();

            using (var searcher = new ManagementObjectSearcher("SELECT DisplayName, PathName, Name, StartName, State, SystemName, ProcessId FROM Win32_Service"))
            {
                foreach (var managementObject in searcher.Get().Cast<ManagementObject>())
                {
                    using (managementObject)
                    {
                        var displayName = GetString(managementObject, "DisplayName");
                        if (string.IsNullOrEmpty(displayName)
                            || !displayName.StartsWith("SQL Server ", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var computerName = GetString(managementObject, "SystemName");
                        var state = GetString(managementObject, "State");
                        if (runOnly && !string.Equals(state, "Running", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var currentInstance = ResolveInstanceFromDisplayName(computerName, displayName);
                        if (!string.IsNullOrWhiteSpace(instanceFilter)
                            && !InstanceMatchesFilter(instanceFilter, currentInstance))
                        {
                            continue;
                        }

                        var processId = GetString(managementObject, "ProcessId");
                        if (processId == "0")
                        {
                            processId = string.Empty;
                        }

                        results.Add(new SqlLocalService
                        {
                            ComputerName = computerName,
                            Instance = currentInstance,
                            ServiceDisplayName = displayName,
                            ServiceName = GetString(managementObject, "Name"),
                            ServicePath = GetString(managementObject, "PathName"),
                            ServiceAccount = GetString(managementObject, "StartName"),
                            ServiceState = state,
                            ServiceProcessId = processId
                        });
                    }
                }
            }

            return results;
        }

        internal static string ResolveInstanceFromDisplayName(string computerName, string displayName)
        {
            var openIndex = displayName.IndexOf('(');
            var closeIndex = displayName.IndexOf(')', openIndex + 1);
            if (openIndex < 0 || closeIndex <= openIndex)
            {
                return computerName;
            }

            var instanceName = displayName.Substring(openIndex + 1, closeIndex - openIndex - 1);
            if (string.Equals(instanceName, "MSSQLSERVER", StringComparison.OrdinalIgnoreCase))
            {
                return computerName;
            }

            return $"{computerName}\\{instanceName}";
        }

        private static bool InstanceMatchesFilter(string filter, string currentInstance)
        {
            return string.Equals(filter, currentInstance, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetString(ManagementObject managementObject, string propertyName)
        {
            return managementObject[propertyName]?.ToString() ?? string.Empty;
        }
    }
}
