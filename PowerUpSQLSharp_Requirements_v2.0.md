# PowerUpSQLSharp 开发需求文档 (v2.0 CONFIRMED)

## 项目代号：PowerUpSQLSharp

| 属性 | 值 |
|------|-----|
| 许可证 | BSD-3-Clause（继承 [NetSPI/PowerUpSQL](https://github.com/NetSPI/PowerUpSQL)） |
| 参考版本 | PowerUpSQL v1.105.0 |
| 目标框架 | .NET Framework 4.8 |
| 交付形态 | 单文件 `PowerUpSQLSharp.exe`（Release 无额外 DLL） |
| Sliver 模式 | `execute-assembly --in-process` |
| 运行模式 | 独立 CLI + C2 托管双模式 |
| CLI 命名 | 保留 PowerUpSQL 函数名（如 `Get-SQLInstanceDomain`） |
| 功能范围 | 全量功能对等（108 个导出函数） |

---

## 一、项目定位

PowerUpSQLSharp 是对 PowerUpSQL 的 **C# 领域化重写**，目标是实现与原版 **功能语义对等**，同时提升可靠性、可维护性与发布安全性。

### 1.1 重写 vs 简单移植

**禁止（简单移植）：**

- 将 108 个 PowerShell 函数逐行翻译为 108 个无共享逻辑的 C# 类
- 原样复制 PowerShell 管道、`ForEach-Object`、字符串拼接
- 继承原版缺陷（无超时、无并发上限、错误处理不一致）

**要求（领域化重写）：**

- 内部按能力域划分服务层（Discovery / Connection / Recon / Audit / Execution 等）
- 对外 CLI 保留 PowerUpSQL 函数名，每个 `FunctionsToExport` 均有对应子命令
- 统一连接管理、凭据、超时、重试、输出格式、错误语义
- `Commands/` 为薄适配层，`Services/` 为厚业务层

### 1.2 已确认技术决策（2026-06-28）

| 决策项 | 选择 | 约束 |
|--------|------|------|
| 功能范围 | 全量功能对等 | 覆盖 `PowerUpSQL.psd1` 全部 `FunctionsToExport` |
| CLI 命名 | 保留 PowerUpSQL 函数名 | 含 `Get-` / `Invoke-` / `Create-` 前缀 |
| 目标框架 | .NET Framework 4.8 | `System.Data.SqlClient`，零额外运行时 |
| 交付形态 | 单文件 exe | ILMerge 或等价单程序集策略 |
| C2 支持 | 正式支持 Sliver | `--in-process` 为验收场景 |
| OPSEC | 代码层不混淆 | AMSI/ETW 由 C2 侧处理 |

---

## 二、非目标（Out of Scope）

- PowerShell 模块（`.psm1`）或 `Import-Module` 运行时兼容
- 与 PowerUpSQL 输出 **100% 字节级一致**（语义与结构等价即可）
- GUI 界面
- 程序集内嵌入编译机用户名、路径、计算机名、内网 IP

---

## 三、功能需求

### 3.1 验收基准

以 [PowerUpSQL.psd1](https://github.com/NetSPI/PowerUpSQL/blob/master/PowerUpSQL.psd1) 中 **108 个** `FunctionsToExport` 为准。完整映射见 [docs/PowerUpSQL-Mapping.md](docs/PowerUpSQL-Mapping.md)。

| 能力域 | 函数数 | Core 服务 |
|--------|--------|-----------|
| 实例发现 | 6 | `IInstanceDiscoveryService` |
| 连接测试 | 3 | `IConnectionTestService` |
| 服务器/库侦察 | 40+ | `IReconnaissanceService` |
| 权限/角色 | 8+ | `IPrivilegeInspectionService` |
| 审计 | 16 | `IAuditService` |
| OS 命令执行 | 7 | `ICommandExecutionService` |
| 提权/模拟 | 4 | `IPrivilegeEscalationService` |
| 持久化检测 | 3 | `IPersistenceDetectionService` |
| AD 侦察 | 16 | `IActiveDirectoryReconService` |
| 链路/横向 | 5 | `ILinkedServerService` |
| Fuzz/辅助 | 4 | `IFuzzService` |
| DLL 生成 | 2 | `IDllGenerationService` |

**项目完成定义：** 全部 108 个函数已实现、有对照测试、文档齐全、Release exe 通过 strip 验证。

### 3.2 CLI 设计

```bash
# 函数名即子命令
PowerUpSQLSharp.exe Get-SQLInstanceLocal
PowerUpSQLSharp.exe Get-SQLInstanceDomain -DomainController dc01.example.com -Username "DOMAIN\user" -Password "..."
PowerUpSQLSharp.exe Get-SQLConnectionTest -Instance "SQL01\MSSQL,1433" -Username sa -Password "..."
PowerUpSQLSharp.exe Get-SQLServerInfo -Instance "SQL01\MSSQL" -Username sa -Password "..."
PowerUpSQLSharp.exe Invoke-SQLAudit -Instance "SQL01\MSSQL" -Username sa -Password "..."
PowerUpSQLSharp.exe Invoke-SQLOSCmd -Instance "SQL01\MSSQL" -Command "whoami" -RawResults

# 全局选项
PowerUpSQLSharp.exe --help
PowerUpSQLSharp.exe Get-SQLInstanceLocal --help
PowerUpSQLSharp.exe --quiet --no-color --verbose --format json
```

**CLI 规则：**

- 子命令名称与 PowerUpSQL 导出名完全一致
- 参数名 PascalCase；同时接受 `-Instance` / `--instance`
- 支持 `-ComputerName`、`-Instance`、`-Username`、`-Password`、`-Threads` 等常用参数
- PowerShell 管道用法通过 `-InputFile` 或 stdin 实例列表等价实现
- `*Threaded` 变体保留为独立命令，内部复用同一服务

**Sliver 托管：**

```bash
sliver (SESSION) > execute-assembly --in-process /tmp/PowerUpSQLSharp.exe Get-SQLInstanceLocal
sliver (SESSION) > execute-assembly --in-process /tmp/PowerUpSQLSharp.exe Invoke-SQLOSCmd -Instance "SQL01\MSSQL" -Command "whoami"
```

### 3.3 可靠性需求

| 编号 | 需求 | 验收标准 |
|------|------|----------|
| R-01 | 连接超时 | SQL/LDAP/UDP 可配置超时（默认 30s），超时返回明确错误 |
| R-02 | 并发控制 | `*Threaded` 命令使用 `SemaphoreSlim`；默认并发上限可配置（默认 10） |
| R-03 | 状态恢复 | `Invoke-SQLOSCmd` 等支持 `-RestoreState`，与 PowerUpSQL 对齐 |
| R-04 | 错误语义统一 | 区分连接/认证/权限/不可达/LDAP 错误；`ExceptionSanitizer` 脱敏 |
| R-05 | 大结果集 | 流式或分页输出；JSON 流式序列化，避免 OOM |
| R-06 | 非域降级 | LDAP/域操作失败返回空集合 + 警告，不崩溃 |
| R-07 | 行为对照 | 每个函数至少 1 个集成用例与 PowerUpSQL 对照 |
| R-08 | in-process 安全 | Sliver `--in-process` 下未捕获异常不得导致 beacon 崩溃 |

### 3.4 独立运行需求

- 不依赖 PowerShell、Sliver 或外部配置文件
- 完整 `--help` / 子命令帮助
- 标准退出码：

| 码 | 含义 |
|----|------|
| 0 | 成功（含无结果但执行正常） |
| 1 | 通用错误 / 未捕获异常 |
| 2 | 参数错误 |
| 3 | 连接/认证失败 |
| 4 | 权限不足 |
| 5 | 部分目标失败（Threaded/批量模式） |

- 独立运行：彩色表格；`--no-color` 或输出重定向时禁用 ANSI
- C2 托管：`RuntimeEnvironment.IsC2Hosted` 或 `--quiet` 时仅输出结果

### 3.5 认证与凭据

- Windows 集成认证
- SQL Server 用户名/密码
- 显式域凭据（`domain\user`）
- 可选：`--config` JSON（实例列表 + 凭据）
- 凭据不得明文写入日志；连接字符串密码替换为 `[REDACTED]`

---

## 四、编译主机信息剥离（发布硬性要求）

发布物 `PowerUpSQLSharp.exe` **不得**包含可追溯到开发机的信息。

### 4.1 必须实施

- MSBuild：`Deterministic=true`，`PathMap` 映射本地路径为 `/src/`
- Release：`DebugType=none`，不发布 `.pdb`
- 手动维护 `AssemblyInfo.cs`（泛化版本号与公司名）
- 发布脚本 `scripts/release.ps1`
- 验证脚本 `scripts/verify-strip.ps1`

### 4.2 验证项（发布前必跑）

- 字符串扫描：无 `C:\Users\`、无开发机计算机名
- 两次相同源码 Release 构建 SHA256 一致
- PE 头无真实编译时间戳

### 4.3 MSBuild 参数示例

```xml
<PropertyGroup Condition="'$(Configuration)'=='Release'">
  <Deterministic>true</Deterministic>
  <DebugType>none</DebugType>
  <DebugSymbols>false</DebugSymbols>
  <PathMap>$(MSBuildProjectDirectory)=/src/</PathMap>
</PropertyGroup>
```

---

## 五、项目结构

```
PowerUpSQLSharp/
├── PowerUpSQLSharp.sln
├── scripts/
│   ├── release.ps1
│   └── verify-strip.ps1
├── src/
│   ├── PowerUpSQLSharp.Core/
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Commands/
│   │   └── Utils/
│   ├── PowerUpSQLSharp.CLI/
│   └── PowerUpSQLSharp.Tests/
└── docs/
    ├── PowerUpSQL-Mapping.md
    └── Acceptance-Tests.md
```

---

## 六、测试与验收

| 类型 | 范围 |
|------|------|
| 单元测试 | 连接字符串、参数映射、输出格式化、异常脱敏 |
| 集成测试 | SQL Server 2016/2019/2022；域环境测 AD/Domain 函数 |
| 对照测试 | 每个导出函数与 PowerUpSQL 原版行为对照 |
| Sliver 兼容性 | 代表性命令在 `--in-process` 下输出可读、无 ANSI |
| Strip 验证 | `verify-strip.ps1` 全绿 |

验收用例详见 [docs/Acceptance-Tests.md](docs/Acceptance-Tests.md)。

---

## 七、交付批次与里程碑

实现分 4 批交付，**项目完成标准是全量 108 函数对等**。

| 批次 | 范围 | 函数约数 | 工期估算 |
|------|------|----------|----------|
| Batch 1 | 基础架构 + 发现 + 连接 + ServerInfo | ~15 | 4–5 周 |
| Batch 2 | 全量 Recon + Priv + DumpInfo | ~35 | 6–8 周 |
| Batch 3 | 全量 Audit + OS Cmd 执行向量 | ~25 | 6–8 周 |
| Batch 4 | AD + 链路 + 提权/持久化/Fuzz/DLL + 发布 | ~33 | 8–10 周 |

**合计：约 24–31 周（1 人全职）**

### Batch 1 明细

- M1：解决方案、Core/CLI 骨架、Command 注册框架
- M2：`Get-SQLInstance*` 全系列 + UDP 扫描
- M3：`Get-SQLConnectionTest*` + `Get-SQLServerLoginDefaultPw`
- M4：`Get-SQLServerInfo` / `Get-SQLServerInfoThreaded`
- M5：Sliver 兼容性 + strip 验证流水线

---

## 八、默认假设（非阻塞）

| 项 | 默认假设 |
|----|----------|
| SQL Server 测试矩阵 | 2016 / 2019 / 2022 Developer 或 Express |
| Threaded 默认并发 | 10，可通过 `-Threads` 覆盖 |
| JSON 输出 | 所有 Get/Invoke 命令支持 `--format json` |
| 开源范围 | 全量命令开源（BSD-3） |

---

*文档版本：v2.0 CONFIRMED*  
*最后更新：2026-06-28*  
*下一步：Batch 1 / M1 基础架构开发*
