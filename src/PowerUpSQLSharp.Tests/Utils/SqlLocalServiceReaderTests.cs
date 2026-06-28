using PowerUpSQLSharp.Core.Utils;
using Xunit;

namespace PowerUpSQLSharp.Tests.Utils
{
    public class SqlLocalServiceReaderTests
    {
        [Fact]
        public void ResolveInstanceFromDisplayName_NamedInstance_ReturnsComputerAndInstance()
        {
            var instance = SqlLocalServiceReader.ResolveInstanceFromDisplayName(
                "SQL01",
                "SQL Server (SQLEXPRESS)");

            Assert.Equal(@"SQL01\SQLEXPRESS", instance);
        }

        [Fact]
        public void ResolveInstanceFromDisplayName_DefaultInstance_ReturnsComputerName()
        {
            var instance = SqlLocalServiceReader.ResolveInstanceFromDisplayName(
                "SQL01",
                "SQL Server (MSSQLSERVER)");

            Assert.Equal("SQL01", instance);
        }

        [Fact]
        public void ResolveInstanceFromDisplayName_NoParentheses_ReturnsComputerName()
        {
            var instance = SqlLocalServiceReader.ResolveInstanceFromDisplayName(
                "SQL01",
                "SQL Server Browser");

            Assert.Equal("SQL01", instance);
        }
    }
}
