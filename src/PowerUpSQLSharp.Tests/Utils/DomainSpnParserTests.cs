using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Utils;
using Xunit;

namespace PowerUpSQLSharp.Tests.Utils
{
    public class DomainSpnParserTests
    {
        [Fact]
        public void ParseServerInstance_NamedInstance_UsesBackslash()
        {
            var instance = DomainSpnParser.ParseServerInstance("MSSQLSvc/SQL01.example.com:SQLEXPRESS");
            Assert.Equal(@"SQL01.example.com\SQLEXPRESS", instance);
        }

        [Fact]
        public void ParseServerInstance_PortInstance_UsesComma()
        {
            var instance = DomainSpnParser.ParseServerInstance("MSSQLSvc/SQL01.example.com:1433");
            Assert.Equal("SQL01.example.com,1433", instance);
        }

        [Fact]
        public void ParseServerInstance_DefaultInstance_ReturnsHostOnly()
        {
            var instance = DomainSpnParser.ParseServerInstance("MSSQLSvc/SQL01.example.com");
            Assert.Equal("SQL01.example.com", instance);
        }

        [Fact]
        public void ParseComputerNameFromSpn_ReturnsHostPart()
        {
            var computerName = DomainSpnParser.ParseComputerNameFromSpn("MSSQLSvc/SQL01.example.com:1433");
            Assert.Equal("SQL01.example.com", computerName);
        }
    }
}
