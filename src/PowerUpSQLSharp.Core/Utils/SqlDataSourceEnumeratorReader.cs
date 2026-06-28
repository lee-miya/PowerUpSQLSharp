using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using PowerUpSQLSharp.Core.Models;

namespace PowerUpSQLSharp.Core.Utils
{
    internal static class SqlDataSourceEnumeratorReader
    {
        public static IReadOnlyList<SqlInstance> ReadBroadcastInstances()
        {
            var instances = new List<SqlInstance>();
            DataTable table;

            try
            {
                table = SqlDataSourceEnumerator.Instance.GetDataSources();
            }
            catch
            {
                return instances;
            }

            foreach (DataRow row in table.Rows)
            {
                var serverName = row["ServerName"] as string ?? string.Empty;
                var instanceName = row["InstanceName"] as string ?? string.Empty;
                var version = row["Version"] as string ?? string.Empty;
                var isClusteredRaw = row["IsClustered"] as string ?? string.Empty;

                var displayInstance = string.IsNullOrEmpty(instanceName)
                    ? serverName
                    : $"{serverName}\\{instanceName}";

                instances.Add(new SqlInstance
                {
                    ComputerName = serverName,
                    InstanceName = instanceName,
                    ServerInstance = displayInstance,
                    Version = version,
                    IsClustered = string.Equals(isClusteredRaw, "Yes", StringComparison.OrdinalIgnoreCase)
                });
            }

            return instances;
        }
    }
}
