using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Models;

namespace PowerUpSQLSharp.Core.Commands
{
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }
        Task<CommandResult> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default);
    }
}
