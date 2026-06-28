# PowerUpSQL → PowerUpSQLSharp 功能映射表

> 基准：PowerUpSQL v1.105.0 `FunctionsToExport`（共 **108** 个函数）

## 图例

| 列 | 说明 |
|----|------|
| **Batch** | 实现批次（1–4） |
| **Core Service** | 内部领域服务（非 1:1 函数类） |
| **CLI Handler** | `PowerUpSQLSharp.Core.Commands` 中的命令处理器类名 |
| **Status** | `planned` = 待实现 |

---

## Batch 1 — 基础架构 + 发现 + 连接 + ServerInfo

| # | PowerUpSQL 函数 | Core Service | CLI Handler | Batch | Status |
|---|-----------------|--------------|-------------|-------|--------|
| 1 | `Get-SQLInstanceLocal` | `IInstanceDiscoveryService` | `GetSqlInstanceLocalCommand` | 1 | planned |
| 2 | `Get-SQLInstanceBroadcast` | `IInstanceDiscoveryService` | `GetSqlInstanceBroadcastCommand` | 1 | planned |
| 3 | `Get-SQLInstanceDomain` | `IInstanceDiscoveryService` | `GetSqlInstanceDomainCommand` | 1 | planned |
| 4 | `Get-SQLInstanceFile` | `IInstanceDiscoveryService` | `GetSqlInstanceFileCommand` | 1 | planned |
| 5 | `Get-SQLInstanceScanUDP` | `IInstanceDiscoveryService` | `GetSqlInstanceScanUdpCommand` | 1 | planned |
| 6 | `Get-SQLInstanceScanUDPThreaded` | `IInstanceDiscoveryService` | `GetSqlInstanceScanUdpThreadedCommand` | 1 | planned |
| 7 | `Get-SQLConnectionTest` | `IConnectionTestService` | `GetSqlConnectionTestCommand` | 1 | planned |
| 8 | `Get-SQLConnectionTestThreaded` | `IConnectionTestService` | `GetSqlConnectionTestThreadedCommand` | 1 | planned |
| 9 | `Get-SQLServerLoginDefaultPw` | `IConnectionTestService` | `GetSqlServerLoginDefaultPwCommand` | 1 | planned |
| 10 | `Get-SQLServerInfo` | `IReconnaissanceService` | `GetSqlServerInfoCommand` | 1 | planned |
| 11 | `Get-SQLServerInfoThreaded` | `IReconnaissanceService` | `GetSqlServerInfoThreadedCommand` | 1 | planned |
| 12 | `Get-SQLServiceLocal` | `IReconnaissanceService` | `GetSqlServiceLocalCommand` | 1 | planned |
| 13 | `Get-SQLSysadminCheck` | `IPrivilegeInspectionService` | `GetSqlSysadminCheckCommand` | 1 | planned |
| 14 | `Get-SQLLocalAdminCheck` | `IPrivilegeInspectionService` | `GetSqlLocalAdminCheckCommand` | 1 | planned |
| 15 | `Get-SQLServerConfiguration` | `IReconnaissanceService` | `GetSqlServerConfigurationCommand` | 1 | planned |

---

## Batch 2 — 全量 Recon + Priv + DumpInfo

| # | PowerUpSQL 函数 | Core Service | CLI Handler | Batch | Status |
|---|-----------------|--------------|-------------|-------|--------|
| 16 | `Get-SQLDatabase` | `IReconnaissanceService` | `GetSqlDatabaseCommand` | 2 | planned |
| 17 | `Get-SQLDatabaseThreaded` | `IReconnaissanceService` | `GetSqlDatabaseThreadedCommand` | 2 | planned |
| 18 | `Get-SQLDatabaseUser` | `IReconnaissanceService` | `GetSqlDatabaseUserCommand` | 2 | planned |
| 19 | `Get-SQLDatabaseSchema` | `IReconnaissanceService` | `GetSqlDatabaseSchemaCommand` | 2 | planned |
| 20 | `Get-SQLDatabaseRole` | `IReconnaissanceService` | `GetSqlDatabaseRoleCommand` | 2 | planned |
| 21 | `Get-SQLDatabaseRoleMember` | `IReconnaissanceService` | `GetSqlDatabaseRoleMemberCommand` | 2 | planned |
| 22 | `Get-SQLDatabasePriv` | `IPrivilegeInspectionService` | `GetSqlDatabasePrivCommand` | 2 | planned |
| 23 | `Get-SQLTable` | `IReconnaissanceService` | `GetSqlTableCommand` | 2 | planned |
| 24 | `Get-SQLTableTemp` | `IReconnaissanceService` | `GetSqlTableTempCommand` | 2 | planned |
| 25 | `Get-SQLColumn` | `IReconnaissanceService` | `GetSqlColumnCommand` | 2 | planned |
| 26 | `Get-SQLColumnSampleData` | `IReconnaissanceService` | `GetSqlColumnSampleDataCommand` | 2 | planned |
| 27 | `Get-SQLColumnSampleDataThreaded` | `IReconnaissanceService` | `GetSqlColumnSampleDataThreadedCommand` | 2 | planned |
| 28 | `Get-SQLView` | `IReconnaissanceService` | `GetSqlViewCommand` | 2 | planned |
| 29 | `Get-SQLStoredProcedure` | `IReconnaissanceService` | `GetSqlStoredProcedureCommand` | 2 | planned |
| 30 | `Get-SQLStoredProcedureCLR` | `IReconnaissanceService` | `GetSqlStoredProcedureClrCommand` | 2 | planned |
| 31 | `Get-SQLStoredProcedureSQLi` | `IReconnaissanceService` | `GetSqlStoredProcedureSqliCommand` | 2 | planned |
| 32 | `Get-SQLStoredProcedureAutoExec` | `IReconnaissanceService` | `GetSqlStoredProcedureAutoExecCommand` | 2 | planned |
| 33 | `Get-SQLStoredProcedureXp` | `IReconnaissanceService` | `GetSqlStoredProcedureXpCommand` | 2 | planned |
| 34 | `Get-SQLTriggerDdl` | `IReconnaissanceService` | `GetSqlTriggerDdlCommand` | 2 | planned |
| 35 | `Get-SQLTriggerDml` | `IReconnaissanceService` | `GetSqlTriggerDmlCommand` | 2 | planned |
| 36 | `Get-SQLSession` | `IReconnaissanceService` | `GetSqlSessionCommand` | 2 | planned |
| 37 | `Get-SQLQuery` | `IReconnaissanceService` | `GetSqlQueryCommand` | 2 | planned |
| 38 | `Get-SQLQueryThreaded` | `IReconnaissanceService` | `GetSqlQueryThreadedCommand` | 2 | planned |
| 39 | `Get-SQLServerLogin` | `IReconnaissanceService` | `GetSqlServerLoginCommand` | 2 | planned |
| 40 | `Get-SQLServerPasswordHash` | `IReconnaissanceService` | `GetSqlServerPasswordHashCommand` | 2 | planned |
| 41 | `Get-SQLServerRole` | `IReconnaissanceService` | `GetSqlServerRoleCommand` | 2 | planned |
| 42 | `Get-SQLServerRoleMember` | `IReconnaissanceService` | `GetSqlServerRoleMemberCommand` | 2 | planned |
| 43 | `Get-SQLServerPriv` | `IPrivilegeInspectionService` | `GetSqlServerPrivCommand` | 2 | planned |
| 44 | `Get-SQLServerCredential` | `IReconnaissanceService` | `GetSqlServerCredentialCommand` | 2 | planned |
| 45 | `Get-SQLServerPolicy` | `IReconnaissanceService` | `GetSqlServerPolicyCommand` | 2 | planned |
| 46 | `Get-SQLServiceAccount` | `IReconnaissanceService` | `GetSqlServiceAccountCommand` | 2 | planned |
| 47 | `Get-SQLAgentJob` | `IReconnaissanceService` | `GetSqlAgentJobCommand` | 2 | planned |
| 48 | `Get-SQLAssemblyFile` | `IReconnaissanceService` | `GetSqlAssemblyFileCommand` | 2 | planned |
| 49 | `Get-SQLOleDbProvder` | `IReconnaissanceService` | `GetSqlOleDbProviderCommand` | 2 | planned |
| 50 | `Get-SQLAuditDatabaseSpec` | `IReconnaissanceService` | `GetSqlAuditDatabaseSpecCommand` | 2 | planned |
| 51 | `Get-SQLAuditServerSpec` | `IReconnaissanceService` | `GetSqlAuditServerSpecCommand` | 2 | planned |
| 52 | `Get-SQLRecoverPwAutoLogon` | `IReconnaissanceService` | `GetSqlRecoverPwAutoLogonCommand` | 2 | planned |
| 53 | `Invoke-SQLDumpInfo` | `IReconnaissanceService` | `InvokeSqlDumpInfoCommand` | 2 | planned |

---

## Batch 3 — 全量 Audit + OS Cmd 执行

| # | PowerUpSQL 函数 | Core Service | CLI Handler | Batch | Status |
|---|-----------------|--------------|-------------|-------|--------|
| 54 | `Invoke-SQLAudit` | `IAuditService` | `InvokeSqlAuditCommand` | 3 | planned |
| 55 | `Invoke-SQLAuditPrivCreateProcedure` | `IAuditService` | `InvokeSqlAuditPrivCreateProcedureCommand` | 3 | planned |
| 56 | `Invoke-SQLAuditPrivDbChaining` | `IAuditService` | `InvokeSqlAuditPrivDbChainingCommand` | 3 | planned |
| 57 | `Invoke-SQLAuditPrivImpersonateLogin` | `IAuditService` | `InvokeSqlAuditPrivImpersonateLoginCommand` | 3 | planned |
| 58 | `Invoke-SQLAuditPrivServerLink` | `IAuditService` | `InvokeSqlAuditPrivServerLinkCommand` | 3 | planned |
| 59 | `Invoke-SQLAuditPrivTrustworthy` | `IAuditService` | `InvokeSqlAuditPrivTrustworthyCommand` | 3 | planned |
| 60 | `Invoke-SQLAuditPrivXpDirtree` | `IAuditService` | `InvokeSqlAuditPrivXpDirtreeCommand` | 3 | planned |
| 61 | `Invoke-SQLAuditPrivXpFileexit` | `IAuditService` | `InvokeSqlAuditPrivXpFileexitCommand` | 3 | planned |
| 62 | `Invoke-SQLAuditRoleDbDdlAdmin` | `IAuditService` | `InvokeSqlAuditRoleDbDdlAdminCommand` | 3 | planned |
| 63 | `Invoke-SQLAuditRoleDbOwner` | `IAuditService` | `InvokeSqlAuditRoleDbOwnerCommand` | 3 | planned |
| 64 | `Invoke-SQLAuditSampleDataByColumn` | `IAuditService` | `InvokeSqlAuditSampleDataByColumnCommand` | 3 | planned |
| 65 | `Invoke-SQLAuditWeakLoginPw` | `IAuditService` | `InvokeSqlAuditWeakLoginPwCommand` | 3 | planned |
| 66 | `Invoke-SQLAuditSQLiSpExecuteAs` | `IAuditService` | `InvokeSqlAuditSqliSpExecuteAsCommand` | 3 | planned |
| 67 | `Invoke-SQLAuditSQLiSpSigned` | `IAuditService` | `InvokeSqlAuditSqliSpSignedCommand` | 3 | planned |
| 68 | `Invoke-SQLAuditDefaultLoginPw` | `IAuditService` | `InvokeSqlAuditDefaultLoginPwCommand` | 3 | planned |
| 69 | `Invoke-SQLAuditPrivAutoExecSp` | `IAuditService` | `InvokeSqlAuditPrivAutoExecSpCommand` | 3 | planned |
| 70 | `Invoke-SQLOSCmd` | `ICommandExecutionService` | `InvokeSqlOsCmdCommand` | 3 | planned |
| 71 | `Invoke-SQLOSCmdCLR` | `ICommandExecutionService` | `InvokeSqlOsCmdClrCommand` | 3 | planned |
| 72 | `Invoke-SQLOSCmdCOle` | `ICommandExecutionService` | `InvokeSqlOsCmdCOleCommand` | 3 | planned |
| 73 | `Invoke-SQLOSCmdPython` | `ICommandExecutionService` | `InvokeSqlOsCmdPythonCommand` | 3 | planned |
| 74 | `Invoke-SQLOSCmdR` | `ICommandExecutionService` | `InvokeSqlOsCmdRCommand` | 3 | planned |
| 75 | `Invoke-SQLOSCmdAgentJob` | `ICommandExecutionService` | `InvokeSqlOsCmdAgentJobCommand` | 3 | planned |

---

## Batch 4 — AD + 链路 + 提权/持久化/Fuzz/DLL + 发布

| # | PowerUpSQL 函数 | Core Service | CLI Handler | Batch | Status |
|---|-----------------|--------------|-------------|-------|--------|
| 76 | `Get-SQLDomainObject` | `IActiveDirectoryReconService` | `GetSqlDomainObjectCommand` | 4 | planned |
| 77 | `Get-SQLDomainComputer` | `IActiveDirectoryReconService` | `GetSqlDomainComputerCommand` | 4 | planned |
| 78 | `Get-SQLDomainUser` | `IActiveDirectoryReconService` | `GetSqlDomainUserCommand` | 4 | planned |
| 79 | `Get-SQLDomainSubnet` | `IActiveDirectoryReconService` | `GetSqlDomainSubnetCommand` | 4 | planned |
| 80 | `Get-SQLDomainSite` | `IActiveDirectoryReconService` | `GetSqlDomainSiteCommand` | 4 | planned |
| 81 | `Get-SQLDomainGroup` | `IActiveDirectoryReconService` | `GetSqlDomainGroupCommand` | 4 | planned |
| 82 | `Get-SQLDomainOu` | `IActiveDirectoryReconService` | `GetSqlDomainOuCommand` | 4 | planned |
| 83 | `Get-SQLDomainAccountPolicy` | `IActiveDirectoryReconService` | `GetSqlDomainAccountPolicyCommand` | 4 | planned |
| 84 | `Get-SQLDomainTrust` | `IActiveDirectoryReconService` | `GetSqlDomainTrustCommand` | 4 | planned |
| 85 | `Get-SQLDomainPasswordsLAPS` | `IActiveDirectoryReconService` | `GetSqlDomainPasswordsLapsCommand` | 4 | planned |
| 86 | `Get-SQLDomainController` | `IActiveDirectoryReconService` | `GetSqlDomainControllerCommand` | 4 | planned |
| 87 | `Get-SQLDomainExploitableSystem` | `IActiveDirectoryReconService` | `GetSqlDomainExploitableSystemCommand` | 4 | planned |
| 88 | `Get-SQLDomainGroupMember` | `IActiveDirectoryReconService` | `GetSqlDomainGroupMemberCommand` | 4 | planned |
| 89 | `Get-DomainObject` | `IActiveDirectoryReconService` | `GetDomainObjectCommand` | 4 | planned |
| 90 | `Get-DomainSpn` | `IActiveDirectoryReconService` | `GetDomainSpnCommand` | 4 | planned |
| 91 | `Get-SQLServerLink` | `ILinkedServerService` | `GetSqlServerLinkCommand` | 4 | planned |
| 92 | `Get-SQLServerLinkCrawl` | `ILinkedServerService` | `GetSqlServerLinkCrawlCommand` | 4 | planned |
| 93 | `Get-SQLServerLinkData` | `ILinkedServerService` | `GetSqlServerLinkDataCommand` | 4 | planned |
| 94 | `Get-SQLServerLinkQuery` | `ILinkedServerService` | `GetSqlServerLinkQueryCommand` | 4 | planned |
| 95 | `Invoke-SQLUncPathInjection` | `ILinkedServerService` | `InvokeSqlUncPathInjectionCommand` | 4 | planned |
| 96 | `Invoke-SQLEscalatePriv` | `IPrivilegeEscalationService` | `InvokeSqlEscalatePrivCommand` | 4 | planned |
| 97 | `Invoke-SQLImpersonateService` | `IPrivilegeEscalationService` | `InvokeSqlImpersonateServiceCommand` | 4 | planned |
| 98 | `Invoke-SQLImpersonateServiceCmd` | `IPrivilegeEscalationService` | `InvokeSqlImpersonateServiceCmdCommand` | 4 | planned |
| 99 | `Invoke-TokenManipulation` | `IPrivilegeEscalationService` | `InvokeTokenManipulationCommand` | 4 | planned |
| 100 | `Get-SQLPersistRegRun` | `IPersistenceDetectionService` | `GetSqlPersistRegRunCommand` | 4 | planned |
| 101 | `Get-SQLPersistRegDebugger` | `IPersistenceDetectionService` | `GetSqlPersistRegDebuggerCommand` | 4 | planned |
| 102 | `Get-SQLPersistTriggerDDL` | `IPersistenceDetectionService` | `GetSqlPersistTriggerDdlCommand` | 4 | planned |
| 103 | `Get-SQLFuzzDatabaseName` | `IFuzzService` | `GetSqlFuzzDatabaseNameCommand` | 4 | planned |
| 104 | `Get-SQLFuzzDomainAccount` | `IFuzzService` | `GetSqlFuzzDomainAccountCommand` | 4 | planned |
| 105 | `Get-SQLFuzzObjectName` | `IFuzzService` | `GetSqlFuzzObjectNameCommand` | 4 | planned |
| 106 | `Get-SQLFuzzServerLogin` | `IFuzzService` | `GetSqlFuzzServerLoginCommand` | 4 | planned |
| 107 | `Create-SQLFileXpDll` | `IDllGenerationService` | `CreateSqlFileXpDllCommand` | 4 | planned |
| 108 | `Create-SQLFileCLRDll` | `IDllGenerationService` | `CreateSqlFileClrDllCommand` | 4 | planned |

---

## Core Service 汇总

| 服务接口 | 职责 | 函数数 |
|----------|------|--------|
| `IInstanceDiscoveryService` | 本地/广播/域/文件/UDP 实例发现 | 6 |
| `IConnectionTestService` | 连接测试、默认密码 | 3 |
| `IReconnaissanceService` | 库/表/列/SP/会话/配置等枚举 | 38 |
| `IPrivilegeInspectionService` | 权限/角色/sysadmin 检查 | 5 |
| `IAuditService` | 弱配置审计 | 16 |
| `ICommandExecutionService` | OS 命令执行（多向量） | 6 |
| `IActiveDirectoryReconService` | AD 域对象/SPN 侦察 | 15 |
| `ILinkedServerService` | 链接服务器爬取/UNC | 5 |
| `IPrivilegeEscalationService` | 提权/令牌/服务模拟 | 4 |
| `IPersistenceDetectionService` | 持久化检测 | 3 |
| `IFuzzService` | 名称/账户 fuzz | 4 |
| `IDllGenerationService` | xp/CLR DLL 生成 | 2 |

---

*最后更新：2026-06-28*
