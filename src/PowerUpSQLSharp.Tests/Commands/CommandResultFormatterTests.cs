using System.Collections.Generic;
using PowerUpSQLSharp.Core.Commands;
using PowerUpSQLSharp.Core.Models;
using Xunit;

namespace PowerUpSQLSharp.Tests.Commands
{
    public class CommandResultFormatterTests
    {
        [Fact]
        public void ToServerInfoRows_IncludesKeyFields()
        {
            var rows = CommandResultFormatter.ToServerInfoRows(new[]
            {
                new SqlServerInfo
                {
                    ComputerName = "SQL01",
                    Instance = @"SQL01\MSSQL",
                    SqlServerVersionNumber = "15.0.2000.5",
                    CurrentLogin = "sa",
                    IsSysadmin = "Yes",
                    XpCmdShellEnabled = "No",
                    XpDirtreeEnabled = "Yes",
                    XpFileexistEnabled = "No",
                    XpSubdirsEnabled = "No",
                    ActiveSessions = "2"
                }
            });

            var row = Assert.Single(rows);
            Assert.Equal("15.0.2000.5", row["SQLServerVersionNumber"]);
            Assert.Equal("sa", row["Currentlogin"]);
            Assert.Equal("Yes", row["IsSysadmin"]);
            Assert.Equal("No", row["XpCmdShellEnabled"]);
            Assert.Equal("Yes", row["XpDirtreeEnabled"]);
            Assert.Equal("2", row["ActiveSessions"]);
        }

        [Fact]
        public void FromRows_JsonFormat_ProducesValidArray()
        {
            var rows = new List<IDictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "ComputerName", "HOST-01" },
                    { "IsClustered", false },
                    { "Port", 1433 }
                }
            };

            var result = CommandResultFormatter.FromRows(rows, new GlobalOptions { Format = "json" });

            Assert.Equal(ExitCodes.Success, result.ExitCode);
            Assert.StartsWith("[{", result.Output);
            Assert.Contains("\"ComputerName\":\"HOST-01\"", result.Output);
            Assert.Contains("\"IsClustered\":false", result.Output);
            Assert.Contains("\"Port\":1433", result.Output);
        }
    }
}
