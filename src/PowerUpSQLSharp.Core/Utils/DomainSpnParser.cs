using System;

namespace PowerUpSQLSharp.Core.Utils
{
    internal static class DomainSpnParser
    {
        public static string ParseServerInstance(string spn)
        {
            if (string.IsNullOrWhiteSpace(spn))
            {
                return string.Empty;
            }

            var slashParts = spn.Split(new[] { '/' }, 2, StringSplitOptions.None);
            if (slashParts.Length < 2)
            {
                return spn;
            }

            var serverPart = slashParts[1];
            var colonParts = serverPart.Split(':');
            var instanceToken = colonParts.Length > 1 ? colonParts[1] : string.Empty;

            string serverInstance;
            if (int.TryParse(instanceToken, out _))
            {
                serverInstance = spn.Replace(":", ",");
            }
            else
            {
                serverInstance = spn.Replace(":", "\\");
            }

            return StripServicePrefix(serverInstance);
        }

        private static string StripServicePrefix(string value)
        {
            const string prefix = "MSSQLSvc/";
            if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return value.Substring(prefix.Length);
            }

            return value;
        }

        public static string ParseComputerNameFromSpn(string spn)
        {
            if (string.IsNullOrWhiteSpace(spn))
            {
                return string.Empty;
            }

            var slashParts = spn.Split(new[] { '/' }, 2, StringSplitOptions.None);
            if (slashParts.Length < 2)
            {
                return string.Empty;
            }

            var hostPart = slashParts[1].Split(':')[0].Split(' ')[0];
            return hostPart.Trim();
        }

        public static string ParseServiceFromSpn(string spn)
        {
            if (string.IsNullOrWhiteSpace(spn))
            {
                return string.Empty;
            }

            var slashIndex = spn.IndexOf('/');
            return slashIndex > 0 ? spn.Substring(0, slashIndex) : spn;
        }
    }
}
