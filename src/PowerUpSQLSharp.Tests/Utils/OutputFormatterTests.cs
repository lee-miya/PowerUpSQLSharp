using System.Collections.Generic;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Utils;
using Xunit;

namespace PowerUpSQLSharp.Tests.Utils
{
    public class OutputFormatterTests
    {
        [Fact]
        public void FormatTable_RendersColumns()
        {
            var rows = new List<IDictionary<string, object>>
            {
                new Dictionary<string, object> { { "Instance", "SQL01" }, { "Port", 1433 } }
            };

            var output = OutputFormatter.FormatTable(rows, new GlobalOptions { NoColor = true });
            Assert.Contains("Instance", output);
            Assert.Contains("SQL01", output);
            Assert.DoesNotContain("\x1b[", output);
        }

        [Fact]
        public void FormatTable_WithColorEnabled_AddsHeaderAnsi()
        {
            var rows = new List<IDictionary<string, object>>
            {
                new Dictionary<string, object> { { "Instance", "SQL01" } }
            };

            var output = OutputFormatter.FormatTable(rows, new GlobalOptions());
            if (RuntimeEnvironment.ShouldUseColor(new GlobalOptions()))
            {
                Assert.Contains("\x1b[", output);
            }
            else
            {
                Assert.DoesNotContain("\x1b[", output);
            }
        }
    }
}
