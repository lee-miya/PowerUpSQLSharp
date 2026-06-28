using System;
using System.Collections.Generic;
using System.IO;
using PowerUpSQLSharp.Core.Models;

namespace PowerUpSQLSharp.Core.Utils
{
    internal static class SqlInstanceFileReader
    {
        public static IReadOnlyList<SqlInstance> ReadInstances(string filePath)
        {
            var instances = new List<SqlInstance>();
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return instances;
            }

            foreach (var line in File.ReadAllLines(filePath))
            {
                var instance = line?.Trim();
                if (string.IsNullOrEmpty(instance))
                {
                    continue;
                }

                var computerName = ExtractComputerName(instance);
                instances.Add(new SqlInstance
                {
                    ComputerName = computerName,
                    ServerInstance = instance,
                    InstanceName = ExtractInstanceName(instance, computerName)
                });
            }

            return instances;
        }

        private static string ExtractComputerName(string instance)
        {
            var commaIndex = instance.IndexOf(',');
            if (commaIndex > 0)
            {
                return instance.Substring(0, commaIndex);
            }

            var slashIndex = instance.IndexOf('\\');
            return slashIndex > 0 ? instance.Substring(0, slashIndex) : instance;
        }

        private static string ExtractInstanceName(string instance, string computerName)
        {
            if (instance.IndexOf(',') >= 0)
            {
                return string.Empty;
            }

            if (!instance.StartsWith(computerName + "\\", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return instance.Substring(computerName.Length + 1);
        }
    }
}
