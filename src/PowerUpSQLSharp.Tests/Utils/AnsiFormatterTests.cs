using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Utils;
using Xunit;

namespace PowerUpSQLSharp.Tests.Utils
{
    public class AnsiFormatterTests
    {
        [Fact]
        public void Strip_RemovesAnsiCodes()
        {
            var input = AnsiFormatter.Wrap("SQL01", AnsiFormatter.Bold + AnsiFormatter.Cyan);
            var stripped = AnsiFormatter.Strip(input);
            Assert.Equal("SQL01", stripped);
        }

        [Fact]
        public void Strip_PreservesPlainText()
        {
            const string input = "Instance | Port";
            Assert.Equal(input, AnsiFormatter.Strip(input));
        }
    }
}
