using System.Collections.Generic;
using PowerUpSQLSharp.Core.Commands;
using PowerUpSQLSharp.Core.Models;
using Xunit;

namespace PowerUpSQLSharp.Tests.Commands
{
    public class GetSqlInstanceScanUdpCommandTests
    {
        [Fact]
        public void ResolveComputerNames_SplitsCommaSeparatedValues()
        {
            var context = new CommandContext
            {
                Arguments = new Dictionary<string, string>
                {
                    { "ComputerName", " SQL01 , SQL02 " }
                }
            };

            var names = GetSqlInstanceScanUdpCommand.ResolveComputerNames(context);

            Assert.Equal(2, names.Count);
            Assert.Equal("SQL01", names[0]);
            Assert.Equal("SQL02", names[1]);
        }

        [Fact]
        public void ResolveComputerNames_MissingArgument_ReturnsEmpty()
        {
            var names = GetSqlInstanceScanUdpCommand.ResolveComputerNames(new CommandContext());
            Assert.Empty(names);
        }

        [Fact]
        public void ResolveUdpTimeout_UsesCommandArgument()
        {
            var context = new CommandContext
            {
                Arguments = new Dictionary<string, string> { { "UDPTimeOut", "5" } }
            };

            Assert.Equal(5, GetSqlInstanceScanUdpCommand.ResolveUdpTimeout(context));
        }

        [Fact]
        public void ResolveUdpTimeout_DefaultsToTwoSeconds()
        {
            Assert.Equal(2, GetSqlInstanceScanUdpCommand.ResolveUdpTimeout(new CommandContext()));
        }
    }
}
