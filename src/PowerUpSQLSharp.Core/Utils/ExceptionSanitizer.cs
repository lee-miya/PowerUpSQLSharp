using System;
using System.Text.RegularExpressions;

namespace PowerUpSQLSharp.Core.Utils
{
    public static class ExceptionSanitizer
    {
        private static readonly Regex UserPathPattern = new Regex(
            @"[A-Za-z]:\\Users\\[^\\]+",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex UncUserPattern = new Regex(
            @"\\\\[^\\]+\\[^\\]+",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string Sanitize(Exception exception)
        {
            if (exception == null)
            {
                return string.Empty;
            }

            var root = Unwrap(exception);
            var message = root.Message ?? string.Empty;
            message = UserPathPattern.Replace(message, @"C:\Users\<user>");
            message = UncUserPattern.Replace(message, @"\\<host>\<share>");

            return $"{root.GetType().Name}: {message}";
        }

        private static Exception Unwrap(Exception exception)
        {
            if (exception is AggregateException aggregate)
            {
                return aggregate.GetBaseException();
            }

            return exception;
        }

        public static string RedactConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return connectionString;
            }

            return Regex.Replace(
                connectionString,
                @"(Password|Pwd)\s*=\s*[^;]*",
                "$1=[REDACTED]",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }
    }
}
