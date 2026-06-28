using System;
using System.Collections.Generic;
using Microsoft.Win32;
using PowerUpSQLSharp.Core.Models;

namespace PowerUpSQLSharp.Core.Utils
{
    internal static class SqlInstanceRegistryReader
    {
        private const string InstanceNamesKey =
            @"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL";

        private const string SqlServerRootKey =
            @"SOFTWARE\Microsoft\Microsoft SQL Server";

        public static IReadOnlyList<SqlInstance> ReadLocalInstances()
        {
            var computerName = Environment.MachineName;
            var instances = new List<SqlInstance>();

            using (var key = Registry.LocalMachine.OpenSubKey(InstanceNamesKey))
            {
                if (key == null)
                {
                    return instances;
                }

                foreach (var instanceName in key.GetValueNames())
                {
                    if (string.IsNullOrWhiteSpace(instanceName))
                    {
                        continue;
                    }

                    var instanceId = key.GetValue(instanceName) as string;
                    instances.Add(BuildInstance(computerName, instanceName, instanceId));
                }
            }

            return instances;
        }

        private static SqlInstance BuildInstance(string computerName, string instanceName, string instanceId)
        {
            var displayName = NormalizeInstanceName(instanceName);
            var serverInstance = string.IsNullOrEmpty(displayName)
                ? computerName
                : $"{computerName}\\{displayName}";

            var instance = new SqlInstance
            {
                ComputerName = computerName,
                InstanceName = displayName,
                ServerInstance = serverInstance
            };

            if (string.IsNullOrEmpty(instanceId))
            {
                return instance;
            }

            PopulateInstanceMetadata(instance, instanceId);
            return instance;
        }

        private static string NormalizeInstanceName(string instanceName)
        {
            return string.Equals(instanceName, "MSSQLSERVER", StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : instanceName;
        }

        private static void PopulateInstanceMetadata(SqlInstance instance, string instanceId)
        {
            using (var setupKey = Registry.LocalMachine.OpenSubKey($@"{SqlServerRootKey}\{instanceId}\Setup"))
            {
                if (setupKey != null)
                {
                    instance.Version = setupKey.GetValue("Version") as string
                        ?? setupKey.GetValue("PatchLevel") as string;
                    instance.IsClustered = IsClusteredValue(setupKey.GetValue("IsClustered"));
                }
            }

            using (var tcpKey = Registry.LocalMachine.OpenSubKey(
                $@"{SqlServerRootKey}\{instanceId}\MSSQLServer\SuperSocketNetLib\Tcp\IPAll"))
            {
                if (tcpKey == null)
                {
                    return;
                }

                var portValue = tcpKey.GetValue("TcpPort") as string;
                if (int.TryParse(portValue, out var port) && port > 0)
                {
                    instance.Port = port;
                }
            }
        }

        private static bool IsClusteredValue(object value)
        {
            if (value == null)
            {
                return false;
            }

            if (value is int intValue)
            {
                return intValue != 0;
            }

            return string.Equals(value.ToString(), "1", StringComparison.OrdinalIgnoreCase);
        }
    }
}
