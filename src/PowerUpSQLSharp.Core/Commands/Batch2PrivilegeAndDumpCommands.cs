using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;

namespace PowerUpSQLSharp.Core.Commands
{
    public sealed class GetSqlDatabasePrivCommand : ICommand
    {
        private readonly IPrivilegeInspectionService _privilegeInspectionService;

        public GetSqlDatabasePrivCommand(IPrivilegeInspectionService privilegeInspectionService)
        {
            _privilegeInspectionService = privilegeInspectionService;
        }

        public string Name => "Get-SQLDatabasePriv";

        public string Description => "Returns database privilege information from accessible databases.";

        public Task<CommandResult> ExecuteAsync(
            CommandContext context,
            CancellationToken cancellationToken = default)
        {
            return ExecutePrivilegeQuery(context, cancellationToken, _privilegeInspectionService.GetDatabasePriv);
        }

        private static Task<CommandResult> ExecutePrivilegeQuery(
            CommandContext context,
            CancellationToken cancellationToken,
            System.Func<SqlConnectionOptions, ReconQueryFilters, CancellationToken, IReadOnlyList<IDictionary<string, object>>> query)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var instances = ConnectionTestCommandHelper.ResolveInstances(context);
            var filters = ReconCommandHelper.ResolveFilters(context);
            var rows = new List<IDictionary<string, object>>();

            foreach (var instance in instances)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var options = ConnectionTestCommandHelper.ResolveConnectionOptions(context, instance);
                rows.AddRange(query(options, filters, cancellationToken));
            }

            return Task.FromResult(ReconCommandHelper.BuildReconResult(
                rows,
                instances.Count,
                context?.Global));
        }
    }

    public sealed class GetSqlServerPrivCommand : ICommand
    {
        private readonly IPrivilegeInspectionService _privilegeInspectionService;

        public GetSqlServerPrivCommand(IPrivilegeInspectionService privilegeInspectionService)
        {
            _privilegeInspectionService = privilegeInspectionService;
        }

        public string Name => "Get-SQLServerPriv";

        public string Description => "Returns server-level privilege information.";

        public Task<CommandResult> ExecuteAsync(
            CommandContext context,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var instances = ConnectionTestCommandHelper.ResolveInstances(context);
            var filters = ReconCommandHelper.ResolveFilters(context);
            var rows = new List<IDictionary<string, object>>();

            foreach (var instance in instances)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var options = ConnectionTestCommandHelper.ResolveConnectionOptions(context, instance);
                rows.AddRange(_privilegeInspectionService.GetServerPriv(options, filters, cancellationToken));
            }

            return Task.FromResult(ReconCommandHelper.BuildReconResult(
                rows,
                instances.Count,
                context?.Global));
        }
    }

    public sealed class InvokeSqlDumpInfoCommand : ICommand
    {
        private readonly IReconnaissanceService _reconnaissanceService;
        private readonly IPrivilegeInspectionService _privilegeInspectionService;

        public InvokeSqlDumpInfoCommand(
            IReconnaissanceService reconnaissanceService,
            IPrivilegeInspectionService privilegeInspectionService)
        {
            _reconnaissanceService = reconnaissanceService;
            _privilegeInspectionService = privilegeInspectionService;
        }

        public string Name => "Invoke-SQLDumpInfo";

        public string Description =>
            "Exports reconnaissance data from a SQL Server instance to CSV files in the output folder.";

        public Task<CommandResult> ExecuteAsync(
            CommandContext context,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var instances = ConnectionTestCommandHelper.ResolveInstances(context);
            if (instances.Count != 1)
            {
                return Task.FromResult(CommandResult.Failure(
                    ExitCodes.ArgumentError,
                    "Invoke-SQLDumpInfo supports a single -Instance value."));
            }

            var filters = ReconCommandHelper.ResolveFilters(context);
            var outFolder = string.IsNullOrWhiteSpace(filters.OutFolder) ? "." : filters.OutFolder;
            var options = ConnectionTestCommandHelper.ResolveConnectionOptions(context, instances[0]);
            var dumpFilters = new ReconQueryFilters { NoDefaults = true };

            try
            {
                Directory.CreateDirectory(outFolder);
                var testFile = Path.Combine(outFolder, "test.txt");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
            }
            catch
            {
                return Task.FromResult(CommandResult.Failure(
                    ExitCodes.PermissionDenied,
                    "Access denied to output directory."));
            }

            var instanceToken = instances[0].Replace('\\', '-').Replace(',', '-');
            var exportedFiles = new List<string>();

            Export(_reconnaissanceService.Query(SqlReconOperation.Database, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Databases", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.DatabaseUser, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Database_users", filters, exportedFiles);
            Export(_privilegeInspectionService.GetDatabasePriv(options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Database_privileges", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.DatabaseRole, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Database_roles", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.DatabaseRoleMember, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Database_role_members", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.DatabaseSchema, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Database_schemas", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.TableTemp, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Server_temp_tables", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.Table, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Database_tables", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.View, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Database_views", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.Column, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Database_columns", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.ServerLogin, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Server_logins", filters, exportedFiles);
            Export(_privilegeInspectionService.GetServerPriv(options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Server_privileges", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.ServerRole, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Server_roles", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.ServerRoleMember, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Server_role_members", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.ServerCredential, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Server_credentials", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.ServiceAccount, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Service_accounts", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.StoredProcedure, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Stored_procedures", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.StoredProcedureXp, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Stored_procedures_xp", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.ServerPolicy, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Server_policies", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.StoredProcedureSqli, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Stored_procedures_sqli", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.StoredProcedureAutoExec, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Stored_procedures_autoexec", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.StoredProcedureClr, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Stored_procedures_clr", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.TriggerDml, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Database_triggers_dml", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.TriggerDdl, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Server_triggers_ddl", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.AuditDatabaseSpec, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Audit_database_specs", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.AuditServerSpec, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Audit_server_specs", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.AgentJob, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Agent_jobs", filters, exportedFiles);
            Export(_reconnaissanceService.Query(SqlReconOperation.OleDbProvider, options, dumpFilters, cancellationToken),
                outFolder, instanceToken, "Ole_db_providers", filters, exportedFiles);
            Export(_reconnaissanceService.GetServerConfiguration(options, cancellationToken),
                outFolder, instanceToken, "Server_configuration", filters, exportedFiles);
            Export(_reconnaissanceService.GetServerInfo(options, cancellationToken),
                outFolder, instanceToken, "Server_info", filters, exportedFiles);

            var output = exportedFiles.Count == 0
                ? string.Empty
                : string.Join(System.Environment.NewLine, exportedFiles);

            return Task.FromResult(new CommandResult
            {
                ExitCode = ExitCodes.Success,
                Output = output
            });
        }

        private static void Export(
            IReadOnlyList<IDictionary<string, object>> rows,
            string outFolder,
            string instanceToken,
            string suffix,
            ReconQueryFilters filters,
            ICollection<string> exportedFiles)
        {
            if (rows == null || rows.Count == 0)
            {
                return;
            }

            var path = Path.Combine(outFolder, instanceToken + "_" + suffix + ".csv");
            WriteCsv(path, rows);
            exportedFiles.Add(path);
        }

        private static void Export(
            IReadOnlyList<SqlServerConfigurationEntry> entries,
            string outFolder,
            string instanceToken,
            string suffix,
            ReconQueryFilters filters,
            ICollection<string> exportedFiles)
        {
            if (entries == null || entries.Count == 0)
            {
                return;
            }

            var rows = CommandResultFormatter.ToServerConfigurationRows(entries)
                .Cast<IDictionary<string, object>>()
                .ToList();
            Export(rows, outFolder, instanceToken, suffix, filters, exportedFiles);
        }

        private static void Export(
            SqlServerInfo info,
            string outFolder,
            string instanceToken,
            string suffix,
            ReconQueryFilters filters,
            ICollection<string> exportedFiles)
        {
            if (info == null)
            {
                return;
            }

            var rows = CommandResultFormatter.ToServerInfoRows(new[] { info })
                .Cast<IDictionary<string, object>>()
                .ToList();
            Export(rows, outFolder, instanceToken, suffix, filters, exportedFiles);
        }

        private static void WriteCsv(string path, IReadOnlyList<IDictionary<string, object>> rows)
        {
            var headers = rows
                .SelectMany(row => row.Keys)
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", headers.Select(EscapeCsv)));

            foreach (var row in rows)
            {
                var values = headers.Select(header =>
                {
                    if (!row.TryGetValue(header, out var value) || value == null)
                    {
                        return string.Empty;
                    }

                    return value.ToString();
                });

                sb.AppendLine(string.Join(",", values.Select(EscapeCsv)));
            }

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0)
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }
    }
}
