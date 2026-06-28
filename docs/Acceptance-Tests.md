# PowerUpSQLSharp 验收测试规范

> 每个 PowerUpSQL 导出函数至少 1 个集成用例，与原版行为语义对照。

## 1. 验收原则

| 原则 | 说明 |
|------|------|
| **语义等价** | 输出字段含义与 PowerUpSQL 一致，允许列顺序、格式细节差异 |
| **独立可重复** | 同一命令对同一目标连续执行两次，结果结构一致 |
| **对照基准** | 在相同测试环境上并行运行 PowerUpSQL 与 PowerUpSQLSharp，对比关键字段 |
| **退出码** | 成功 0；连接失败 3；权限不足 4；参数错误 2 |

## 2. 测试环境要求

| 组件 | 要求 |
|------|------|
| SQL Server | 2016 / 2019 / 2022 至少各 1 实例（Developer 或 Express） |
| 域环境 | 含 DC、至少 1 台加入域的 SQL Server（测 Domain/AD 函数） |
| 非域环境 | 独立 Windows 主机（测 graceful degradation） |
| Sliver | 可选；用于 C2 托管验收 |

## 3. Batch 1 验收用例（优先）

### 3.1 Get-SQLInstanceLocal

| ID | 场景 | PowerUpSQL 对照命令 | 预期 |
|----|------|---------------------|------|
| B1-001 | 本机有默认实例 | `Get-SQLInstanceLocal` | 返回至少 1 个实例；含 InstanceName |
| B1-002 | 本机无 SQL | `Get-SQLInstanceLocal` | 空结果，exit 0 |
| B1-003 | JSON 输出 | `--format json` | 合法 JSON 数组 |

### 3.2 Get-SQLInstanceBroadcast

| ID | 场景 | 预期 |
|----|------|------|
| B1-010 | 子网内有 SQL Browser | 返回实例列表 |
| B1-011 | 超时 `-TimeoutSeconds 1` | 不挂死，exit 0 或 5 |

### 3.3 Get-SQLInstanceDomain

| ID | 场景 | 预期 |
|----|------|------|
| B1-020 | 域内成员机 | 返回域内 SQL SPN/实例 |
| B1-021 | 非域机器 | 空结果 + 警告，不崩溃，exit 0 |

### 3.4 Get-SQLInstanceFile

| ID | 场景 | 预期 |
|----|------|------|
| B1-030 | 有效实例列表文件 | 解析并返回实例 |
| B1-031 | 文件不存在 | exit 2，明确错误信息 |

### 3.5 Get-SQLInstanceScanUDP / Threaded

| ID | 场景 | 预期 |
|----|------|------|
| B1-040 | `/24` 子网扫描 | 发现已知实例 |
| B1-041 | Threaded `-Threads 5` | 与单线程结果一致，耗时更短 |

### 3.6 Get-SQLConnectionTest / Threaded

| ID | 场景 | 预期 |
|----|------|------|
| B1-050 | 正确 sa 凭据 | Success=true |
| B1-051 | 错误密码 | Success=false，exit 3 |
| B1-052 | Windows 集成认证 | Success=true（当前用户有权限时） |

### 3.7 Get-SQLServerLoginDefaultPw

| ID | 场景 | 预期 |
|----|------|------|
| B1-060 | 默认 sa 空密码实例 | 检测到弱口令 |
| B1-061 | 强密码实例 | 无匹配 |

### 3.8 Get-SQLServerInfo / Threaded

| ID | 场景 | 预期 |
|----|------|------|
| B1-070 | 有效连接 | 含 Version、CurrentLogin、IsSysadmin、XpCmdShellEnabled |
| B1-071 | 与 PowerUpSQL 对照 | 关键字段语义一致 |

### 3.9 基础设施

| ID | 场景 | 预期 |
|----|------|------|
| B1-080 | `--help` | 列出已实现子命令 |
| B1-081 | 未知子命令 | exit 2 |
| B1-082 | Sliver `--in-process` | 无 ANSI 转义码 |
| B1-083 | `verify-strip.ps1` | 无 `C:\Users\` 路径泄露 |
| B1-084 | 确定性编译 | 两次 Release SHA256 相同 |

---

## 4. Batch 2–4 验收模板

每个函数使用以下模板追加用例：

```markdown
### {FunctionName}

| ID | 场景 | PowerUpSQL 对照 | 预期 |
|----|------|-----------------|------|
| {ID}-001 | 正常路径 | `{FunctionName} -Instance ...` | 语义等价 |
| {ID}-002 | 无权限 | 同上 | exit 4 |
| {ID}-003 | 实例不可达 | 同上 | exit 3 |
```

### 4.1 Batch 2 重点函数（Recon）

- `Get-SQLDatabase`, `Get-SQLTable`, `Get-SQLColumn`
- `Get-SQLStoredProcedureXp`, `Get-SQLStoredProcedureCLR`
- `Invoke-SQLDumpInfo`

### 4.2 Batch 3 重点函数（Audit + Exec）

- `Invoke-SQLAudit`（聚合全部子检查）
- `Invoke-SQLAuditPrivTrustworthy`, `Invoke-SQLAuditWeakLoginPw`
- `Invoke-SQLOSCmd`（含 `-RestoreState`）
- `Invoke-SQLOSCmdCLR`, `Invoke-SQLOSCmdAgentJob`

### 4.3 Batch 4 重点函数（AD + Link + PrivEsc）

- `Get-SQLInstanceDomain`, `Get-DomainSpn`
- `Get-SQLServerLinkCrawl`
- `Invoke-SQLUncPathInjection`
- `Invoke-SQLEscalatePriv`
- `Create-SQLFileXpDll`

---

## 5. 对照测试执行步骤

1. 在测试 SQL Server 上记录实例名、凭据、已知配置（xp_cmdshell 状态等）
2. 运行 PowerUpSQL：`Import-Module PowerUpSQL; {Function} ...`
3. 运行 PowerUpSQLSharp：`PowerUpSQLSharp.exe {Function} ...`
4. 对比关键输出字段（见各用例「预期」列）
5. 记录差异到 issue；语义差异视为缺陷，格式差异可接受

## 6. 自动化建议

| 层级 | 工具 | 范围 |
|------|------|------|
| 单元测试 | xUnit / NUnit | 连接字符串、OutputFormatter、ExceptionSanitizer |
| 集成测试 | 条件编译 `[Category("Integration")]` | 需真实 SQL Server |
| Strip 测试 | `scripts/verify-strip.ps1` | 发布流水线 |
| 对照测试 | 手动 + 可选 Pester 包装 | 全量 108 函数 |

---

## 7. 项目完成验收清单

- [ ] 108 个函数全部实现（见 [PowerUpSQL-Mapping.md](PowerUpSQL-Mapping.md)）
- [ ] 每个函数至少 1 个集成用例通过
- [ ] Batch 1–4 对照测试记录归档
- [ ] Sliver `--in-process` 代表性命令验收通过
- [ ] `verify-strip.ps1` 全绿
- [ ] README + 需求文档 + 映射表 + 本文档齐全

---

*最后更新：2026-06-28*
