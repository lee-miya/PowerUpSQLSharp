using PowerUpSQLSharp.Core.Utils;
using Xunit;

namespace PowerUpSQLSharp.Tests.Utils
{
    public class SqlInstanceParserTests
    {
        [Theory]
        [InlineData(@"SQL01\SQLEXPRESS", "SQL01")]
        [InlineData("SQL01,1433", "SQL01")]
        [InlineData(@"SQL01\SQLEXPRESS,1433", "SQL01")]
        public void GetComputerNameFromInstance_ParsesHost(string instance, string expected)
        {
            Assert.Equal(expected, SqlInstanceParser.GetComputerNameFromInstance(instance));
        }

        [Theory]
        [InlineData(@"SQL01\STANDARDDEV2014", "STANDARDDEV2014")]
        [InlineData(@"SQL01\SQLEXPRESS,1433", "SQLEXPRESS")]
        [InlineData("SQL01", null)]
        [InlineData("SQL01,1433", null)]
        public void GetNamedInstance_ReturnsNamedPart(string instance, string expected)
        {
            Assert.Equal(expected, SqlInstanceParser.GetNamedInstance(instance));
        }
    }
}
