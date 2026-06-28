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
                { "Get-SQLServerConfiguration", new GetSqlServerConfigurationCommand(reconnaissanceService) },
                { "Get-SQLDatabase", new GetSqlDatabaseCommand(reconnaissanceService) },
                { "Get-SQLDatabaseThreaded", new GetSqlDatabaseThreadedCommand(reconnaissanceService) },
                { "Get-SQLDatabaseUser", new GetSqlDatabaseUserCommand(reconnaissanceService) },
                { "Get-SQLDatabaseSchema", new GetSqlDatabaseSchemaCommand(reconnaissanceService) },
                { "Get-SQLDatabaseRole", new GetSqlDatabaseRoleCommand(reconnaissanceService) },
                { "Get-SQLDatabaseRoleMember", new GetSqlDatabaseRoleMemberCommand(reconnaissanceService) },
                { "Get-SQLDatabasePriv", new GetSqlDatabasePrivCommand(privilegeInspectionService) },
                { "Get-SQLTable", new GetSqlTableCommand(reconnaissanceService) },
                { "Get-SQLTableTemp", new GetSqlTableTempCommand(reconnaissanceService) },
                { "Get-SQLColumn", new GetSqlColumnCommand(reconnaissanceService) },
                { "Get-SQLColumnSampleData", new GetSqlColumnSampleDataCommand(reconnaissanceService) },
                { "Get-SQLColumnSampleDataThreaded", new GetSqlColumnSampleDataThreadedCommand(reconnaissanceService) },
                { "Get-SQLView", new GetSqlViewCommand(reconnaissanceService) },
                { "Get-SQLStoredProcedure", new GetSqlStoredProcedureCommand(reconnaissanceService) },
                { "Get-SQLStoredProcedureCLR", new GetSqlStoredProcedureClrCommand(reconnaissanceService) },
                { "Get-SQLStoredProcedureSQLi", new GetSqlStoredProcedureSqliCommand(reconnaissanceService) },
                { "Get-SQLStoredProcedureAutoExec", new GetSqlStoredProcedureAutoExecCommand(reconnaissanceService) },
                { "Get-SQLStoredProcedureXp", new GetSqlStoredProcedureXpCommand(reconnaissanceService) },
                { "Get-SQLTriggerDdl", new GetSqlTriggerDdlCommand(reconnaissanceService) },
                { "Get-SQLTriggerDml", new GetSqlTriggerDmlCommand(reconnaissanceService) },
                { "Get-SQLSession", new GetSqlSessionCommand(reconnaissanceService) },
                { "Get-SQLQuery", new GetSqlQueryCommand(reconnaissanceService) },
                { "Get-SQLQueryThreaded", new GetSqlQueryThreadedCommand(reconnaissanceService) },
                { "Get-SQLServerLogin", new GetSqlServerLoginCommand(reconnaissanceService) },
                { "Get-SQLServerPasswordHash", new GetSqlServerPasswordHashCommand(reconnaissanceService) },
                { "Get-SQLServerRole", new GetSqlServerRoleCommand(reconnaissanceService) },
                { "Get-SQLServerRoleMember", new GetSqlServerRoleMemberCommand(reconnaissanceService) },
                { "Get-SQLServerPriv", new GetSqlServerPrivCommand(privilegeInspectionService) },
                { "Get-SQLServerCredential", new GetSqlServerCredentialCommand(reconnaissanceService) },
                { "Get-SQLServerPolicy", new GetSqlServerPolicyCommand(reconnaissanceService) },
                { "Get-SQLServiceAccount", new GetSqlServiceAccountCommand(reconnaissanceService) },
                { "Get-SQLAgentJob", new GetSqlAgentJobCommand(reconnaissanceService) },
                { "Get-SQLAssemblyFile", new GetSqlAssemblyFileCommand(reconnaissanceService) },
                { "Get-SQLOleDbProvder", new GetSqlOleDbProviderCommand(reconnaissanceService) },
                { "Get-SQLAuditDatabaseSpec", new GetSqlAuditDatabaseSpecCommand(reconnaissanceService) },
                { "Get-SQLAuditServerSpec", new GetSqlAuditServerSpecCommand(reconnaissanceService) },
                { "Get-SQLRecoverPwAutoLogon", new GetSqlRecoverPwAutoLogonCommand(reconnaissanceService) },
                { "Invoke-SQLDumpInfo", new InvokeSqlDumpInfoCommand(reconnaissanceService, privilegeInspectionService) }
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
