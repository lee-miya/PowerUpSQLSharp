using System;
using PowerUpSQLSharp.Core.Models;

namespace PowerUpSQLSharp.Core.Utils
{
    public static class RuntimeEnvironment
    {
        public static bool IsC2Hosted =>
            Console.IsOutputRedirected || Console.IsInputRedirected;

        public static bool SupportsColor =>
            !Console.IsOutputRedirected &&
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NO_COLOR"));

        public static bool ShouldUseColor(GlobalOptions options)
        {
            if (options != null && options.NoColor)
            {
                return false;
            }

            return SupportsColor;
        }

        public static bool ShouldUseQuiet(GlobalOptions options)
        {
            if (options == null)
            {
                return IsC2Hosted;
            }

            return options.Quiet || (IsC2Hosted && !options.Verbose);
        }
    }
}
