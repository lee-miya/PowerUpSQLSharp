using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Commands;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Utils;

namespace PowerUpSQLSharp.CLI
{
    internal static class Program
    {
        private static readonly CommandRegistry Registry = CommandBootstrap.CreateDefaultRegistry();

        private static int Main(string[] args)
        {
            try
            {
                return RunAsync(args).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                WriteUnhandledException(ex);
                return ExitCodes.GeneralError;
            }
        }

        private static void WriteUnhandledException(Exception ex)
        {
            if (RuntimeEnvironment.IsC2Hosted)
            {
                return;
            }

            Console.Error.WriteLine(ExceptionSanitizer.Sanitize(ex));
        }

        private static async Task<int> RunAsync(string[] args)
        {
            if (args == null || args.Length == 0 || IsHelp(args[0]))
            {
                PrintGlobalHelp();
                return ExitCodes.Success;
            }

            var global = ParseGlobalOptions(ref args);
            if (args.Length == 0)
            {
                PrintGlobalHelp();
                return ExitCodes.Success;
            }

            if (IsHelp(args[0]))
            {
                PrintCommandHelp(args[0]);
                return ExitCodes.Success;
            }

            var commandName = args[0];
            if (IsHelp(args.Length > 1 ? args[1] : null))
            {
                PrintCommandHelp(commandName);
                return ExitCodes.Success;
            }

            var context = new CommandContext
            {
                Global = global,
                Arguments = ParseCommandArguments(args.Skip(1))
            };

            var result = await Registry.ExecuteAsync(commandName, context).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(result.Error))
            {
                OutputFormatter.WriteError(result.Error, global);
            }

            if (!string.IsNullOrEmpty(result.Warning))
            {
                OutputFormatter.WriteWarning(result.Warning, global);
            }

            if (!string.IsNullOrEmpty(result.Output))
            {
                OutputFormatter.WriteResult(result.Output, global);
            }

            return result.ExitCode;
        }

        private static GlobalOptions ParseGlobalOptions(ref string[] args)
        {
            var options = new GlobalOptions();
            var remaining = new List<string>();

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                switch (arg.ToLowerInvariant())
                {
                    case "--quiet":
                    case "-q":
                        options.Quiet = true;
                        break;
                    case "--no-color":
                        options.NoColor = true;
                        break;
                    case "--verbose":
                    case "-v":
                        options.Verbose = true;
                        break;
                    case "--format":
                        if (i + 1 < args.Length)
                        {
                            options.Format = args[++i];
                        }
                        break;
                    case "--timeout":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out var timeout))
                        {
                            options.TimeoutSeconds = timeout;
                        }
                        break;
                    case "--threads":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out var threads))
                        {
                            options.Threads = threads;
                        }
                        break;
                    default:
                        remaining.Add(arg);
                        break;
                }
            }

            args = remaining.ToArray();
            return options;
        }

        private static Dictionary<string, string> ParseCommandArguments(IEnumerable<string> args)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var list = args.ToList();

            for (var i = 0; i < list.Count; i++)
            {
                var token = list[i];
                if (!token.StartsWith("-", StringComparison.Ordinal))
                {
                    continue;
                }

                var key = token.TrimStart('-');
                var value = i + 1 < list.Count && !list[i + 1].StartsWith("-", StringComparison.Ordinal)
                    ? list[++i]
                    : "true";
                map[key] = value;
            }

            return map;
        }

        private static bool IsHelp(string arg)
        {
            return string.Equals(arg, "--help", StringComparison.OrdinalIgnoreCase)
                || string.Equals(arg, "-h", StringComparison.OrdinalIgnoreCase)
                || string.Equals(arg, "/?", StringComparison.OrdinalIgnoreCase);
        }

        private static void PrintGlobalHelp()
        {
            Console.WriteLine("PowerUpSQLSharp - C# rewrite of NetSPI PowerUpSQL");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  PowerUpSQLSharp.exe [global options] <CommandName> [command options]");
            Console.WriteLine();
            Console.WriteLine("Global options:");
            Console.WriteLine("  --quiet, -q          Suppress progress output");
            Console.WriteLine("  --no-color           Disable ANSI colors");
            Console.WriteLine("  --verbose, -v        Verbose output");
            Console.WriteLine("  --format <table|json> Output format");
            Console.WriteLine("  --timeout <seconds>  Default timeout (default: 30)");
            Console.WriteLine("  --threads <n>        Default thread count (default: 10)");
            Console.WriteLine("  --help, -h           Show help");
            Console.WriteLine();
            Console.WriteLine("Registered commands ({0}):", Registry.GetAll().Count);
            foreach (var command in Registry.GetAll())
            {
                Console.WriteLine("  {0}", command.Name);
            }
        }

        private static void PrintCommandHelp(string commandName)
        {
            if (!Registry.TryGet(commandName, out var command))
            {
                Console.Error.WriteLine("Unknown command: {0}", commandName);
                return;
            }

            Console.WriteLine("{0}", command.Name);
            Console.WriteLine("  {0}", command.Description);
        }
    }
}
