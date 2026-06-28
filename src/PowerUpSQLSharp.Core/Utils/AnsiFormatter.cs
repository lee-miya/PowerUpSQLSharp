using System.Text.RegularExpressions;

namespace PowerUpSQLSharp.Core.Utils
{
    public static class AnsiFormatter
    {
        public const string Reset = "\x1b[0m";
        public const string Bold = "\x1b[1m";
        public const string Cyan = "\x1b[36m";
        public const string Yellow = "\x1b[33m";
        public const string Red = "\x1b[31m";

        private static readonly Regex AnsiPattern = new Regex(
            @"\x1b\[[0-9;]*m",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string Strip(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            return AnsiPattern.Replace(text, string.Empty);
        }

        public static string Wrap(string text, string code)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(code))
            {
                return text ?? string.Empty;
            }

            return code + text + Reset;
        }
    }
}
