using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PowerUpSQLSharp.Core.Models;

namespace PowerUpSQLSharp.Core.Utils
{
    public static class OutputFormatter
    {
        public static string FormatTable(IEnumerable<IDictionary<string, object>> rows, GlobalOptions options)
        {
            var materialized = rows?.ToList() ?? new List<IDictionary<string, object>>();
            if (materialized.Count == 0)
            {
                return string.Empty;
            }

            var columns = materialized.SelectMany(r => r.Keys).Distinct().ToList();
            var widths = columns.ToDictionary(
                c => c,
                c => Math.Max(c.Length, materialized.Max(r => (r.ContainsKey(c) ? r[c]?.ToString() : null) ?? string.Empty).Length));

            var useColor = RuntimeEnvironment.ShouldUseColor(options);
            var sb = new StringBuilder();
            var header = string.Join(" | ", columns.Select(c => c.PadRight(widths[c])));
            sb.AppendLine(useColor ? AnsiFormatter.Wrap(header, AnsiFormatter.Bold + AnsiFormatter.Cyan) : header);

            foreach (var row in materialized)
            {
                sb.AppendLine(string.Join(" | ", columns.Select(c =>
                {
                    var value = row.ContainsKey(c) ? row[c]?.ToString() ?? string.Empty : string.Empty;
                    return value.PadRight(widths[c]);
                })));
            }

            return sb.ToString().TrimEnd();
        }

        public static void WriteLine(string message, GlobalOptions options)
        {
            if (RuntimeEnvironment.ShouldUseQuiet(options) || string.IsNullOrEmpty(message))
            {
                return;
            }

            Console.WriteLine(PrepareForOutput(message, options));
        }

        public static void WriteResult(string message, GlobalOptions options)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            Console.WriteLine(PrepareForOutput(message, options));
        }

        public static void WriteWarning(string message, GlobalOptions options)
        {
            if (RuntimeEnvironment.ShouldUseQuiet(options) || string.IsNullOrEmpty(message))
            {
                return;
            }

            var formatted = RuntimeEnvironment.ShouldUseColor(options)
                ? AnsiFormatter.Wrap(message, AnsiFormatter.Yellow)
                : message;
            Console.Error.WriteLine(PrepareForOutput(formatted, options));
        }

        public static void WriteError(string message, GlobalOptions options)
        {
            if (RuntimeEnvironment.ShouldUseQuiet(options) || string.IsNullOrEmpty(message))
            {
                return;
            }

            var formatted = RuntimeEnvironment.ShouldUseColor(options)
                ? AnsiFormatter.Wrap(message, AnsiFormatter.Red)
                : message;
            Console.Error.WriteLine(PrepareForOutput(formatted, options));
        }

        public static void WriteVerbose(string message, GlobalOptions options)
        {
            if (options == null || !options.Verbose || string.IsNullOrEmpty(message))
            {
                return;
            }

            Console.Error.WriteLine(PrepareForOutput(message, options));
        }

        private static string PrepareForOutput(string message, GlobalOptions options)
        {
            if (RuntimeEnvironment.ShouldUseColor(options))
            {
                return message;
            }

            return AnsiFormatter.Strip(message);
        }
    }
}
