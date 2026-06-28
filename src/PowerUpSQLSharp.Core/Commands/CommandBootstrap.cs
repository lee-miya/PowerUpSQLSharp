using System;
using System.Collections.Generic;
using PowerUpSQLSharp.Core.Services;

namespace PowerUpSQLSharp.Core.Commands
{
    public static class CommandBootstrap
    {
        private static readonly string[] ExportedFunctions =
        {
            "Create-SQLFileXpDll", "Create-SQLFileCLRDll", "Get-SQLAgentJob", "Get-SQLAssemblyFile",
            "Get-SQLAuditDatabaseSpec", "Get-SQLAuditServerSpec", "Get-SQLColumn", "Get-SQLColumnSampleData",
            "Get-SQLColumnSampleDataThreaded", "Get-SQLConnectionTest", "Get-SQLConnectionTestThreaded",
            "Get-SQLDatabase", "Get-SQLDatabasePriv", "Get-SQLDatabaseRole", "Get-SQLDatabaseRoleMember",
            "Get-SQLDatabaseSchema", "Get-SQLDatabaseThreaded", "Get-SQLDatabaseUser", "Get-SQLDomainObject",
            "Get-SQLDomainComputer", "Get-SQLDomainUser", "Get-SQLDomainSubnet", "Get-SQLDomainSite",
            "Get-SQLDomainGroup", "Get-SQLDomainOu", "Get-SQLDomainAccountPolicy", "Get-SQLDomainTrust",
            "Get-SQLDomainPasswordsLAPS", "Get-SQLDomainController", "Get-SQLDomainExploitableSystem",
            "Get-SQLDomainGroupMember", "Get-SQLFuzzDatabaseName", "Get-SQLFuzzDomainAccount",
            "Get-SQLFuzzObjectName", "Get-SQLFuzzServerLogin", "Get-SQLInstanceBroadcast",
            "Get-SQLInstanceDomain", "Get-SQLInstanceFile", "Get-SQLInstanceLocal", "Get-SQLInstanceScanUDP",
            "Get-SQLInstanceScanUDPThreaded", "Get-SQLLocalAdminCheck", "Get-SQLPersistRegRun",
            "Get-SQLPersistRegDebugger", "Get-SQLPersistTriggerDDL", "Get-SQLOleDbProvder", "Get-SQLQuery",
            "Get-SQLQueryThreaded", "Get-SQLRecoverPwAutoLogon", "Get-SQLServerConfiguration",
            "Get-SQLServerCredential", "Get-SQLServerInfo", "Get-SQLServerInfoThreaded", "Get-SQLServerLink",
            "Get-SQLServerLinkCrawl", "Get-SQLServerLinkData", "Get-SQLServerLinkQuery", "Get-SQLServerLogin",
            "Get-SQLServerLoginDefaultPw", "Get-SQLServerPasswordHash", "Get-SQLServerPolicy", "Get-SQLServerPriv",
            "Get-SQLServerRole", "Get-SQLServerRoleMember", "Get-SQLServiceAccount", "Get-SQLServiceLocal",
            "Get-SQLSession", "Get-SQLStoredProcedure", "Get-SQLStoredProcedureCLR", "Get-SQLStoredProcedureSQLi",
            "Get-SQLStoredProcedureAutoExec", "Get-SQLStoredProcedureXp", "Get-SQLSysadminCheck", "Get-SQLTable",
            "Get-SQLTableTemp", "Get-SQLTriggerDdl", "Get-SQLTriggerDml", "Get-SQLView", "Invoke-SQLAudit",
            "Invoke-SQLAuditPrivCreateProcedure", "Invoke-SQLAuditPrivDbChaining",
            "Invoke-SQLAuditPrivImpersonateLogin", "Invoke-SQLAuditPrivServerLink", "Invoke-SQLAuditPrivTrustworthy",
            "Invoke-SQLAuditPrivXpDirtree", "Invoke-SQLAuditPrivXpFileexit", "Invoke-SQLAuditRoleDbDdlAdmin",
            "Invoke-SQLAuditRoleDbOwner", "Invoke-SQLAuditSampleDataByColumn", "Invoke-SQLAuditWeakLoginPw",
            "Invoke-SQLAuditSQLiSpExecuteAs", "Invoke-SQLAuditSQLiSpSigned", "Invoke-SQLAuditDefaultLoginPw",
            "Invoke-SQLAuditPrivAutoExecSp", "Invoke-SQLDumpInfo", "Invoke-SQLEscalatePriv",
            "Invoke-SQLImpersonateService", "Invoke-SQLImpersonateServiceCmd", "Invoke-SQLUncPathInjection",
            "Invoke-SQLOSCmd", "Invoke-SQLOSCmdCLR", "Invoke-SQLOSCmdCOle", "Invoke-SQLOSCmdPython",
            "Invoke-SQLOSCmdR", "Invoke-SQLOSCmdAgentJob", "Invoke-TokenManipulation", "Get-DomainObject",
            "Get-DomainSpn"
        };

        public static CommandRegistry CreateDefaultRegistry()
        {
            var registry = new CommandRegistry();
            var discoveryService = new InstanceDiscoveryService();
            var connectionFactory = new SqlConnectionFactory();
            var connectionTestService = new ConnectionTestService(connectionFactory);
            var reconnaissanceService = new ReconnaissanceService(connectionFactory, connectionTestService);
            var privilegeInspectionService = new PrivilegeInspectionService(connectionFactory, connectionTestService);
            var implemented = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase)
            {
                { "Get-SQLInstanceLocal", new GetSqlInstanceLocalCommand(discoveryService) },
                { "Get-SQLInstanceBroadcast", new GetSqlInstanceBroadcastCommand(discoveryService) },
                { "Get-SQLInstanceFile", new GetSqlInstanceFileCommand(discoveryService) },
                { "Get-SQLInstanceScanUDP", new GetSqlInstanceScanUdpCommand(discoveryService) },
                { "Get-SQLInstanceScanUDPThreaded", new GetSqlInstanceScanUdpThreadedCommand(discoveryService) },
                { "Get-SQLInstanceDomain", new GetSqlInstanceDomainCommand(discoveryService) },
                { "Get-SQLConnectionTest", new GetSqlConnectionTestCommand(connectionTestService) },
                { "Get-SQLConnectionTestThreaded", new GetSqlConnectionTestThreadedCommand(connectionTestService) },
                { "Get-SQLServerLoginDefaultPw", new GetSqlServerLoginDefaultPwCommand(connectionTestService) },
                { "Get-SQLServerInfo", new GetSqlServerInfoCommand(reconnaissanceService) },
                { "Get-SQLServerInfoThreaded", new GetSqlServerInfoThreadedCommand(reconnaissanceService) },
                { "Get-SQLServiceLocal", new GetSqlServiceLocalCommand() },
                { "Get-SQLSysadminCheck", new GetSqlSysadminCheckCommand(privilegeInspectionService) },
                { "Get-SQLLocalAdminCheck", new GetSqlLocalAdminCheckCommand(privilegeInspectionService) },
                { "Get-SQLServerConfiguration", new GetSqlServerConfigurationCommand(reconnaissanceService) }
            };

            var commands = new List<ICommand>();
            foreach (var name in ExportedFunctions)
            {
                if (implemented.TryGetValue(name, out var command))
                {
                    commands.Add(command);
                    continue;
                }

                commands.Add(new NotImplementedCommand(name, $"PowerUpSQL compatible command: {name}"));
            }

            registry.RegisterRange(commands);
            return registry;
        }
    }
}
