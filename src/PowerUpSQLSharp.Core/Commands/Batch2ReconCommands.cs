using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;

namespace PowerUpSQLSharp.Core.Commands
{
    public abstract class SqlReconCommandBase : ICommand
    {
        private readonly IReconnaissanceService _reconnaissanceService;
        private readonly SqlReconOperation _operation;
        private readonly bool _threaded;

        protected SqlReconCommandBase(
            IReconnaissanceService reconnaissanceService,
            SqlReconOperation operation,
            bool threaded = false)
        {
            _reconnaissanceService = reconnaissanceService;
            _operation = operation;
            _threaded = threaded;
        }

        public abstract string Name { get; }
        public abstract string Description { get; }

        public Task<CommandResult> ExecuteAsync(
            CommandContext context,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var instances = ConnectionTestCommandHelper.ResolveInstances(context);
            var templateOptions = ConnectionTestCommandHelper.ResolveConnectionOptions(context, instances[0]);
            var filters = ReconCommandHelper.ResolveFilters(context);
            var threads = ReconCommandHelper.ResolveThreads(context);

            IReadOnlyList<IDictionary<string, object>> rows;
            if (_threaded)
            {
                rows = _reconnaissanceService.QueryThreaded(
                    _operation,
                    instances,
                    templateOptions,
                    filters,
                    threads,
                    cancellationToken);
            }
            else
            {
                rows = QueryInstances(instances, templateOptions, filters, cancellationToken);
            }

            return Task.FromResult(ReconCommandHelper.BuildReconResult(
                rows,
                instances.Count,
                context?.Global));
        }

        private IReadOnlyList<IDictionary<string, object>> QueryInstances(
            IReadOnlyList<string> instances,
            SqlConnectionOptions templateOptions,
            ReconQueryFilters filters,
            CancellationToken cancellationToken)
        {
            var rows = new List<IDictionary<string, object>>();
            foreach (var instance in instances)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var options = new SqlConnectionOptions
                {
                    Instance = instance,
                    Username = templateOptions.Username,
                    Password = templateOptions.Password,
                    Dac = templateOptions.Dac,
                    Database = templateOptions.Database,
                    ConnectTimeoutSeconds = templateOptions.ConnectTimeoutSeconds
                };
                rows.AddRange(_reconnaissanceService.Query(_operation, options, filters, cancellationToken));
            }

            return rows;
        }
    }

    public sealed class GetSqlDatabaseCommand : SqlReconCommandBase
    {
        public GetSqlDatabaseCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.Database)
        {
        }

        public override string Name => "Get-SQLDatabase";
        public override string Description => "Returns database information from target SQL Server instances.";
    }

    public sealed class GetSqlDatabaseThreadedCommand : SqlReconCommandBase
    {
        public GetSqlDatabaseThreadedCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.Database, threaded: true)
        {
        }

        public override string Name => "Get-SQLDatabaseThreaded";
        public override string Description => "Returns database information from target SQL Server instances using parallel threads.";
    }

    public sealed class GetSqlDatabaseUserCommand : SqlReconCommandBase
    {
        public GetSqlDatabaseUserCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.DatabaseUser)
        {
        }

        public override string Name => "Get-SQLDatabaseUser";
        public override string Description => "Returns database user information from accessible databases.";
    }

    public sealed class GetSqlDatabaseSchemaCommand : SqlReconCommandBase
    {
        public GetSqlDatabaseSchemaCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.DatabaseSchema)
        {
        }

        public override string Name => "Get-SQLDatabaseSchema";
        public override string Description => "Returns database schema information from accessible databases.";
    }

    public sealed class GetSqlDatabaseRoleCommand : SqlReconCommandBase
    {
        public GetSqlDatabaseRoleCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.DatabaseRole)
        {
        }

        public override string Name => "Get-SQLDatabaseRole";
        public override string Description => "Returns database role information from accessible databases.";
    }

    public sealed class GetSqlDatabaseRoleMemberCommand : SqlReconCommandBase
    {
        public GetSqlDatabaseRoleMemberCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.DatabaseRoleMember)
        {
        }

        public override string Name => "Get-SQLDatabaseRoleMember";
        public override string Description => "Returns database role membership information from accessible databases.";
    }

    public sealed class GetSqlTableCommand : SqlReconCommandBase
    {
        public GetSqlTableCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.Table)
        {
        }

        public override string Name => "Get-SQLTable";
        public override string Description => "Returns table information from accessible databases.";
    }

    public sealed class GetSqlTableTempCommand : SqlReconCommandBase
    {
        public GetSqlTableTempCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.TableTemp)
        {
        }

        public override string Name => "Get-SQLTableTemp";
        public override string Description => "Returns temp table and table variable information from tempdb.";
    }

    public sealed class GetSqlColumnCommand : SqlReconCommandBase
    {
        public GetSqlColumnCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.Column)
        {
        }

        public override string Name => "Get-SQLColumn";
        public override string Description => "Returns column information from accessible databases.";
    }

    public sealed class GetSqlColumnSampleDataCommand : SqlReconCommandBase
    {
        public GetSqlColumnSampleDataCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.ColumnSampleData)
        {
        }

        public override string Name => "Get-SQLColumnSampleData";
        public override string Description => "Samples column data from columns matching keyword filters.";
    }

    public sealed class GetSqlColumnSampleDataThreadedCommand : SqlReconCommandBase
    {
        public GetSqlColumnSampleDataThreadedCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.ColumnSampleData, threaded: true)
        {
        }

        public override string Name => "Get-SQLColumnSampleDataThreaded";
        public override string Description => "Samples column data from columns matching keyword filters using parallel threads.";
    }

    public sealed class GetSqlViewCommand : SqlReconCommandBase
    {
        public GetSqlViewCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.View)
        {
        }

        public override string Name => "Get-SQLView";
        public override string Description => "Returns view information from accessible databases.";
    }

    public sealed class GetSqlStoredProcedureCommand : SqlReconCommandBase
    {
        public GetSqlStoredProcedureCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.StoredProcedure)
        {
        }

        public override string Name => "Get-SQLStoredProcedure";
        public override string Description => "Returns stored procedure information from accessible databases.";
    }

    public sealed class GetSqlStoredProcedureClrCommand : SqlReconCommandBase
    {
        public GetSqlStoredProcedureClrCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.StoredProcedureClr)
        {
        }

        public override string Name => "Get-SQLStoredProcedureCLR";
        public override string Description => "Returns CLR stored procedure and assembly information.";
    }

    public sealed class GetSqlStoredProcedureSqliCommand : SqlReconCommandBase
    {
        public GetSqlStoredProcedureSqliCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.StoredProcedureSqli)
        {
        }

        public override string Name => "Get-SQLStoredProcedureSQLi";
        public override string Description => "Returns stored procedures that may be vulnerable to SQL injection.";
    }

    public sealed class GetSqlStoredProcedureAutoExecCommand : SqlReconCommandBase
    {
        public GetSqlStoredProcedureAutoExecCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.StoredProcedureAutoExec)
        {
        }

        public override string Name => "Get-SQLStoredProcedureAutoExec";
        public override string Description => "Returns auto-executed stored procedures from master.";
    }

    public sealed class GetSqlStoredProcedureXpCommand : SqlReconCommandBase
    {
        public GetSqlStoredProcedureXpCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.StoredProcedureXp)
        {
        }

        public override string Name => "Get-SQLStoredProcedureXp";
        public override string Description => "Returns extended stored procedures from accessible databases.";
    }

    public sealed class GetSqlTriggerDdlCommand : SqlReconCommandBase
    {
        public GetSqlTriggerDdlCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.TriggerDdl)
        {
        }

        public override string Name => "Get-SQLTriggerDdl";
        public override string Description => "Returns server-level DDL triggers.";
    }

    public sealed class GetSqlTriggerDmlCommand : SqlReconCommandBase
    {
        public GetSqlTriggerDmlCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.TriggerDml)
        {
        }

        public override string Name => "Get-SQLTriggerDml";
        public override string Description => "Returns database DML triggers from accessible databases.";
    }

    public sealed class GetSqlSessionCommand : SqlReconCommandBase
    {
        public GetSqlSessionCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.Session)
        {
        }

        public override string Name => "Get-SQLSession";
        public override string Description => "Returns active SQL Server sessions.";
    }

    public sealed class GetSqlQueryCommand : SqlReconCommandBase
    {
        public GetSqlQueryCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.CustomQuery)
        {
        }

        public override string Name => "Get-SQLQuery";
        public override string Description => "Executes a custom SQL query on target SQL Server instances.";
    }

    public sealed class GetSqlQueryThreadedCommand : SqlReconCommandBase
    {
        public GetSqlQueryThreadedCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.CustomQuery, threaded: true)
        {
        }

        public override string Name => "Get-SQLQueryThreaded";
        public override string Description => "Executes a custom SQL query on target SQL Server instances using parallel threads.";
    }

    public sealed class GetSqlServerLoginCommand : SqlReconCommandBase
    {
        public GetSqlServerLoginCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.ServerLogin)
        {
        }

        public override string Name => "Get-SQLServerLogin";
        public override string Description => "Returns SQL Server login information.";
    }

    public sealed class GetSqlServerPasswordHashCommand : SqlReconCommandBase
    {
        public GetSqlServerPasswordHashCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.ServerPasswordHash)
        {
        }

        public override string Name => "Get-SQLServerPasswordHash";
        public override string Description => "Returns SQL Server login password hashes.";
    }

    public sealed class GetSqlServerRoleCommand : SqlReconCommandBase
    {
        public GetSqlServerRoleCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.ServerRole)
        {
        }

        public override string Name => "Get-SQLServerRole";
        public override string Description => "Returns SQL Server role information.";
    }

    public sealed class GetSqlServerRoleMemberCommand : SqlReconCommandBase
    {
        public GetSqlServerRoleMemberCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.ServerRoleMember)
        {
        }

        public override string Name => "Get-SQLServerRoleMember";
        public override string Description => "Returns SQL Server role membership information.";
    }

    public sealed class GetSqlServerCredentialCommand : SqlReconCommandBase
    {
        public GetSqlServerCredentialCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.ServerCredential)
        {
        }

        public override string Name => "Get-SQLServerCredential";
        public override string Description => "Returns SQL Server credential information.";
    }

    public sealed class GetSqlServerPolicyCommand : SqlReconCommandBase
    {
        public GetSqlServerPolicyCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.ServerPolicy)
        {
        }

        public override string Name => "Get-SQLServerPolicy";
        public override string Description => "Returns SQL Server policy information from msdb.";
    }

    public sealed class GetSqlServiceAccountCommand : SqlReconCommandBase
    {
        public GetSqlServiceAccountCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.ServiceAccount)
        {
        }

        public override string Name => "Get-SQLServiceAccount";
        public override string Description => "Returns SQL Server related Windows service accounts.";
    }

    public sealed class GetSqlAgentJobCommand : SqlReconCommandBase
    {
        public GetSqlAgentJobCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.AgentJob)
        {
        }

        public override string Name => "Get-SQLAgentJob";
        public override string Description => "Returns SQL Agent job information from msdb.";
    }

    public sealed class GetSqlAssemblyFileCommand : SqlReconCommandBase
    {
        public GetSqlAssemblyFileCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.AssemblyFile)
        {
        }

        public override string Name => "Get-SQLAssemblyFile";
        public override string Description => "Returns SQL assembly file information from accessible databases.";
    }

    public sealed class GetSqlOleDbProviderCommand : SqlReconCommandBase
    {
        public GetSqlOleDbProviderCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.OleDbProvider)
        {
        }

        public override string Name => "Get-SQLOleDbProvder";
        public override string Description => "Returns OLE DB provider information and registry properties.";
    }

    public sealed class GetSqlAuditDatabaseSpecCommand : SqlReconCommandBase
    {
        public GetSqlAuditDatabaseSpecCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.AuditDatabaseSpec)
        {
        }

        public override string Name => "Get-SQLAuditDatabaseSpec";
        public override string Description => "Returns database audit specification information.";
    }

    public sealed class GetSqlAuditServerSpecCommand : SqlReconCommandBase
    {
        public GetSqlAuditServerSpecCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.AuditServerSpec)
        {
        }

        public override string Name => "Get-SQLAuditServerSpec";
        public override string Description => "Returns server audit specification information.";
    }

    public sealed class GetSqlRecoverPwAutoLogonCommand : SqlReconCommandBase
    {
        public GetSqlRecoverPwAutoLogonCommand(IReconnaissanceService reconnaissanceService)
            : base(reconnaissanceService, SqlReconOperation.RecoverPwAutoLogon)
        {
        }

        public override string Name => "Get-SQLRecoverPwAutoLogon";
        public override string Description => "Attempts to recover Windows auto-logon credentials via xp_regread.";
    }
}
