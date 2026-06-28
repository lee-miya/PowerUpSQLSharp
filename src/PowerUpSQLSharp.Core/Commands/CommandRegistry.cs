using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Models;

namespace PowerUpSQLSharp.Core.Commands
{
    public sealed class CommandRegistry
    {
        private readonly Dictionary<string, ICommand> _commands =
            new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase);

        public void Register(ICommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            _commands[command.Name] = command;
        }

        public void RegisterRange(IEnumerable<ICommand> commands)
        {
            foreach (var command in commands ?? Enumerable.Empty<ICommand>())
            {
                Register(command);
            }
        }

        public bool TryGet(string name, out ICommand command)
        {
            return _commands.TryGetValue(NormalizeName(name), out command);
        }

        public IReadOnlyCollection<ICommand> GetAll()
        {
            return _commands.Values.OrderBy(c => c.Name).ToList();
        }

        public async Task<CommandResult> ExecuteAsync(
            string name,
            CommandContext context,
            CancellationToken cancellationToken = default)
        {
            if (!TryGet(name, out var command))
            {
                return CommandResult.Failure(
                    ExitCodes.ArgumentError,
                    $"Unknown command: {name}. Use --help to list available commands.");
            }

            context.CommandName = command.Name;
            return await command.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
        }

        private static string NormalizeName(string name)
        {
            return (name ?? string.Empty).Trim();
        }
    }
}
