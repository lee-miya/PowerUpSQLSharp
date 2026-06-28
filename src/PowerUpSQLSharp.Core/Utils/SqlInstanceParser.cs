using System;

namespace PowerUpSQLSharp.Core.Utils
{
    public static class SqlInstanceParser
    {
        public static string GetComputerNameFromInstance(string instance)
        {
            if (string.IsNullOrWhiteSpace(instance))
            {
                return Environment.MachineName;
            }

            var backslashIndex = instance.IndexOf('\\');
            var commaIndex = instance.IndexOf(',');

            var endIndex = instance.Length;
            if (backslashIndex >= 0)
            {
                endIndex = Math.Min(endIndex, backslashIndex);
            }

            if (commaIndex >= 0)
            {
                endIndex = Math.Min(endIndex, commaIndex);
            }

            return instance.Substring(0, endIndex);
        }

        public static string GetNamedInstance(string instance)
        {
            if (string.IsNullOrWhiteSpace(instance))
            {
                return null;
            }

            var parts = instance.Split('\\');
            return parts.Length > 1 ? parts[1].Split(',')[0] : null;
        }
    }
}
