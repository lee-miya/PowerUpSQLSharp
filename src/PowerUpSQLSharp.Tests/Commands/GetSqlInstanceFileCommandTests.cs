using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Commands;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;
using Xunit;

namespace PowerUpSQLSharp.Tests.Commands
{
    public class GetSqlInstanceFileCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_ValidFile_ReturnsInstances()
        {
            var filePath = Path.GetTempFileName();
            try
            {
                File.WriteAllLines(filePath, new[]
                {
                    @"HOST-01\SQLEXPRESS",
                    "HOST-02",
                    "HOST-03,1433"
                });

                var command = new GetSqlInstanceFileCommand(new InstanceDiscoveryService());
                var context = new CommandContext
                {
                    Arguments = new Dictionary<string, string> { { "FilePath", filePath } }
                };

                var result = await command.ExecuteAsync(context).ConfigureAwait(false);

                Assert.Equal(ExitCodes.Success, result.ExitCode);
                Assert.Contains("HOST-01", result.Output);
                Assert.Contains("SQLEXPRESS", result.Output);
                Assert.Contains("HOST-03,1433", result.Output);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public async Task ExecuteAsync_MissingFilePath_ReturnsArgumentError()
        {
            var command = new GetSqlInstanceFileCommand(new InstanceDiscoveryService());
            var result = await command.ExecuteAsync(new CommandContext()).ConfigureAwait(false);

            Assert.Equal(ExitCodes.ArgumentError, result.ExitCode);
            Assert.Contains("FilePath", result.Error);
        }

        [Fact]
        public async Task ExecuteAsync_FileNotFound_ReturnsArgumentError()
        {
            var command = new GetSqlInstanceFileCommand(new InstanceDiscoveryService());
            var context = new CommandContext
            {
                Arguments = new Dictionary<string, string>
                {
                    { "FilePath", Path.Combine(Path.GetTempPath(), "missing-" + Path.GetRandomFileName() + ".txt") }
                }
            };

            var result = await command.ExecuteAsync(context).ConfigureAwait(false);

            Assert.Equal(ExitCodes.ArgumentError, result.ExitCode);
            Assert.Contains("valid", result.Error);
        }
    }
}
