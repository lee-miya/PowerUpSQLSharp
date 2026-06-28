using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Models;

namespace PowerUpSQLSharp.Core.Commands
{
    public sealed class NotImplementedCommand : ICommand
    {
        public NotImplementedCommand(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; }
        public string Description { get; }

        public Task<CommandResult> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CommandResult.Failure(
                ExitCodes.GeneralError,
                $"{Name} is registered but not yet implemented. See docs/PowerUpSQL-Mapping.md for batch schedule."));
        }
    }
}
