using PowerUpSQLSharp.Core.Commands;
using Xunit;

namespace PowerUpSQLSharp.Tests.Commands
{
    public class CommandBootstrapTests
    {
        [Fact]
        public void CreateDefaultRegistry_RegistersAllExportedFunctions()
        {
            var registry = CommandBootstrap.CreateDefaultRegistry();
            Assert.Equal(108, registry.GetAll().Count);
            Assert.True(registry.TryGet("Get-SQLInstanceLocal", out var localCommand));
            Assert.Equal("Get-SQLInstanceLocal", localCommand.Name);
            Assert.True(registry.TryGet("Get-SQLInstanceBroadcast", out var broadcastCommand));
            Assert.False(broadcastCommand is NotImplementedCommand);
            Assert.True(registry.TryGet("Get-SQLInstanceFile", out var fileCommand));
            Assert.False(fileCommand is NotImplementedCommand);
            Assert.True(registry.TryGet("Get-SQLInstanceDomain", out var domainCommand));
            Assert.False(domainCommand is NotImplementedCommand);
            Assert.True(registry.TryGet("Get-SQLConnectionTest", out var connectionTestCommand));
            Assert.False(connectionTestCommand is NotImplementedCommand);
            Assert.True(registry.TryGet("Get-SQLConnectionTestThreaded", out var connectionTestThreadedCommand));
            Assert.False(connectionTestThreadedCommand is NotImplementedCommand);
            Assert.True(registry.TryGet("Get-SQLServerLoginDefaultPw", out var defaultPwCommand));
            Assert.False(defaultPwCommand is NotImplementedCommand);
            Assert.True(registry.TryGet("Get-SQLServerInfo", out var serverInfoCommand));
            Assert.False(serverInfoCommand is NotImplementedCommand);
            Assert.True(registry.TryGet("Get-SQLServerInfoThreaded", out var serverInfoThreadedCommand));
            Assert.False(serverInfoThreadedCommand is NotImplementedCommand);
            Assert.True(registry.TryGet("Get-SQLServiceLocal", out var serviceLocalCommand));
            Assert.False(serviceLocalCommand is NotImplementedCommand);
            Assert.True(registry.TryGet("Get-SQLSysadminCheck", out var sysadminCommand));
            Assert.False(sysadminCommand is NotImplementedCommand);
            Assert.True(registry.TryGet("Get-SQLLocalAdminCheck", out var localAdminCommand));
            Assert.False(localAdminCommand is NotImplementedCommand);
            Assert.True(registry.TryGet("Get-SQLServerConfiguration", out var configurationCommand));
            Assert.False(configurationCommand is NotImplementedCommand);
            Assert.True(registry.TryGet("Get-SQLDatabase", out var databaseCommand));
            Assert.False(databaseCommand is NotImplementedCommand);
            Assert.True(registry.TryGet("Get-SQLTable", out var tableCommand));
            Assert.False(tableCommand is NotImplementedCommand);
            Assert.True(registry.TryGet("Invoke-SQLDumpInfo", out var dumpInfoCommand));
            Assert.False(dumpInfoCommand is NotImplementedCommand);
            Assert.True(registry.TryGet("Invoke-SQLOSCmd", out _));
        }
    }
}
