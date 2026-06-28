using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Utils;

namespace PowerUpSQLSharp.Core.Services
{
    internal sealed class ReconQueryRunner
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly IConnectionTestService _connectionTestService;

        public ReconQueryRunner(
            ISqlConnectionFactory connectionFactory,
            IConnectionTestService connectionTestService)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _connectionTestService = connectionTestService ?? throw new ArgumentNullException(nameof(connectionTestService));
        }

        public IReadOnlyList<IDictionary<string, object>> Execute(
            SqlReconOperation operation,
            SqlConnectionOptions options,
            ReconQueryFilters filters,
            CancellationToken cancellationToken = default)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            filters = filters ?? new ReconQueryFilters();
            var instance = ResolveInstance(options.Instance);
            var computerName = SqlInstanceParser.GetComputerNameFromInstance(instance);
            var connectionOptions = CloneOptions(options, instance);

            if (!IsAccessible(connectionOptions, cancellationToken))
            {
                return Array.Empty<IDictionary<string, object>>();
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                return ExecuteInternal(operation, connectionOptions, filters, computerName, instance, cancellationToken);
            }
            catch
            {
                return Array.Empty<IDictionary<string, object>>();
            }
        }

        private IReadOnlyList<IDictionary<string, object>> ExecuteInternal(
            SqlReconOperation operation,
            SqlConnectionOptions connectionOptions,
            ReconQueryFilters filters,
            string computerName,
            string instance,
            CancellationToken cancellationToken)
        {
            switch (operation)
            {
                case SqlReconOperation.Database:
                    return ExecuteDatabase(connectionOptions, filters, computerName, instance, cancellationToken);
                case SqlReconOperation.CustomQuery:
                    return ExecuteRawQuery(connectionOptions, filters.QueryText, cancellationToken);
                case SqlReconOperation.ColumnSampleData:
                    return ExecuteColumnSampleData(connectionOptions, filters, computerName, instance, cancellationToken);
                case SqlReconOperation.ServiceAccount:
                    return ExecuteServiceAccount(connectionOptions, computerName, instance, cancellationToken);
                case SqlReconOperation.AgentJob:
                    return ExecuteAgentJob(connectionOptions, filters, computerName, instance, cancellationToken);
                case SqlReconOperation.RecoverPwAutoLogon:
                    return ExecuteRecoverPwAutoLogon(connectionOptions, computerName, instance, cancellationToken);
                case SqlReconOperation.StoredProcedureAutoExec:
                    return ExecuteRawQuery(
                        connectionOptions,
                        SqlReconQueryBuilder.BuildStoredProcedureAutoExecQuery(computerName, instance, filters),
                        cancellationToken);
                case SqlReconOperation.TableTemp:
                    return ExecuteRawQuery(
                        connectionOptions,
                        SqlReconQueryBuilder.BuildTableTempQuery(),
                        cancellationToken);
                case SqlReconOperation.TriggerDdl:
                    return ExecuteRawQuery(
                        connectionOptions,
                        SqlReconQueryBuilder.BuildTriggerDdlQuery(computerName, instance, filters),
                        cancellationToken);
                case SqlReconOperation.Session:
                    return ExecuteRawQuery(
                        connectionOptions,
                        SqlReconQueryBuilder.BuildSessionQuery(computerName, instance, filters),
                        cancellationToken);
                case SqlReconOperation.ServerLogin:
                    return ExecuteRawQuery(
                        connectionOptions,
                        SqlReconQueryBuilder.BuildServerLoginQuery(computerName, instance, filters),
                        cancellationToken);
                case SqlReconOperation.ServerPasswordHash:
                    return ExecuteServerPasswordHash(connectionOptions, computerName, instance, filters, cancellationToken);
                case SqlReconOperation.ServerRole:
                    return ExecuteRawQuery(
                        connectionOptions,
                        SqlReconQueryBuilder.BuildServerRoleQuery(computerName, instance, filters),
                        cancellationToken);
                case SqlReconOperation.ServerRoleMember:
                    return ExecuteRawQuery(
                        connectionOptions,
                        SqlReconQueryBuilder.BuildServerRoleMemberQuery(computerName, instance, filters),
                        cancellationToken);
                case SqlReconOperation.ServerPriv:
                    return ExecuteRawQuery(
                        connectionOptions,
                        SqlReconQueryBuilder.BuildServerPrivQuery(computerName, instance, filters),
                        cancellationToken);
                case SqlReconOperation.ServerCredential:
                    return ExecuteRawQuery(
                        connectionOptions,
                        SqlReconQueryBuilder.BuildServerCredentialQuery(computerName, instance, filters),
                        cancellationToken);
                case SqlReconOperation.ServerPolicy:
                    return ExecuteRawQuery(
                        ChangeDatabase(connectionOptions, "msdb"),
                        SqlReconQueryBuilder.BuildServerPolicyQuery(computerName, instance),
                        cancellationToken);
                case SqlReconOperation.OleDbProvider:
                    return ExecuteRawQuery(
                        connectionOptions,
                        SqlReconQueryBuilder.BuildOleDbProviderQuery(computerName, instance),
                        cancellationToken);
                case SqlReconOperation.AuditDatabaseSpec:
                    return ExecuteRawQuery(
                        connectionOptions,
                        SqlReconQueryBuilder.BuildAuditDatabaseSpecQuery(computerName, instance, filters),
                        cancellationToken);
                case SqlReconOperation.AuditServerSpec:
                    return ExecuteRawQuery(
                        connectionOptions,
                        SqlReconQueryBuilder.BuildAuditServerSpecQuery(computerName, instance, filters),
                        cancellationToken);
                default:
                    return ExecuteForEachDatabase(
                        operation,
                        connectionOptions,
                        filters,
                        computerName,
                        instance,
                        cancellationToken);
            }
        }

        private IReadOnlyList<IDictionary<string, object>> ExecuteDatabase(
            SqlConnectionOptions connectionOptions,
            ReconQueryFilters filters,
            string computerName,
            string instance,
            CancellationToken cancellationToken)
        {
            var majorVersion = GetSqlMajorVersion(connectionOptions, cancellationToken);
            var query = SqlReconQueryBuilder.BuildDatabaseQuery(computerName, instance, filters, majorVersion);
            var rows = ExecuteRawQuery(connectionOptions, query, cancellationToken);

            if (majorVersion >= 10)
            {
                return rows;
            }

            var normalized = new List<IDictionary<string, object>>(rows.Count);
            foreach (var row in rows)
            {
                var copy = new Dictionary<string, object>(row, StringComparer.OrdinalIgnoreCase)
                {
                    ["is_broker_enabled"] = "NA",
                    ["is_encrypted"] = "NA",
                    ["is_read_only"] = "NA"
                };
                normalized.Add(copy);
            }

            return normalized;
        }

        private IReadOnlyList<IDictionary<string, object>> ExecuteForEachDatabase(
            SqlReconOperation operation,
            SqlConnectionOptions connectionOptions,
            ReconQueryFilters filters,
            string computerName,
            string instance,
            CancellationToken cancellationToken)
        {
            var databaseFilters = CloneDatabaseFilters(filters);
            databaseFilters.HasAccess = true;
            if (ShouldUseNoDefaults(operation))
            {
                databaseFilters.NoDefaults = filters.NoDefaults;
            }

            var databases = EnumerateDatabaseNames(connectionOptions, databaseFilters, computerName, instance, cancellationToken);
            var results = new List<IDictionary<string, object>>();

            foreach (var databaseName in databases)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var query = BuildDatabaseScopedQuery(operation, computerName, instance, databaseName, filters);
                if (string.IsNullOrWhiteSpace(query))
                {
                    continue;
                }

                var scopedOptions = ChangeDatabase(connectionOptions, databaseName);
                results.AddRange(ExecuteRawQuery(scopedOptions, query, cancellationToken));
            }

            if (operation == SqlReconOperation.DatabaseSchema && filters.ShowRoleSchemas != true)
            {
                return results
                    .Where(row =>
                    {
                        if (!row.TryGetValue("SchemaId", out var schemaId) || schemaId == null || schemaId == DBNull.Value)
                        {
                            return true;
                        }

                        return Convert.ToInt32(schemaId) >= 1000;
                    })
                    .ToList();
            }

            return results;
        }

        private static bool ShouldUseNoDefaults(SqlReconOperation operation)
        {
            switch (operation)
            {
                case SqlReconOperation.DatabaseUser:
                case SqlReconOperation.DatabaseSchema:
                case SqlReconOperation.DatabaseRole:
                case SqlReconOperation.DatabaseRoleMember:
                case SqlReconOperation.DatabasePriv:
                case SqlReconOperation.Table:
                case SqlReconOperation.Column:
                case SqlReconOperation.View:
                case SqlReconOperation.StoredProcedure:
                case SqlReconOperation.StoredProcedureClr:
                case SqlReconOperation.StoredProcedureSqli:
                case SqlReconOperation.StoredProcedureXp:
                case SqlReconOperation.AssemblyFile:
                    return true;
                default:
                    return false;
            }
        }

        private static string BuildDatabaseScopedQuery(
            SqlReconOperation operation,
            string computerName,
            string instance,
            string databaseName,
            ReconQueryFilters filters)
        {
            switch (operation)
            {
                case SqlReconOperation.DatabaseUser:
                    return SqlReconQueryBuilder.BuildDatabaseUserQuery(computerName, instance, databaseName, filters);
                case SqlReconOperation.DatabaseSchema:
                    return SqlReconQueryBuilder.BuildDatabaseSchemaQuery(computerName, instance, databaseName, filters);
                case SqlReconOperation.DatabaseRole:
                    return SqlReconQueryBuilder.BuildDatabaseRoleQuery(computerName, instance, databaseName, filters);
                case SqlReconOperation.DatabaseRoleMember:
                    return SqlReconQueryBuilder.BuildDatabaseRoleMemberQuery(computerName, instance, databaseName, filters);
                case SqlReconOperation.DatabasePriv:
                    return SqlReconQueryBuilder.BuildDatabasePrivQuery(computerName, instance, databaseName, filters);
                case SqlReconOperation.Table:
                    return SqlReconQueryBuilder.BuildTableQuery(computerName, instance, databaseName, filters);
                case SqlReconOperation.Column:
                    return SqlReconQueryBuilder.BuildColumnQuery(computerName, instance, databaseName, filters);
                case SqlReconOperation.View:
                    return SqlReconQueryBuilder.BuildViewQuery(computerName, instance, databaseName, filters);
                case SqlReconOperation.StoredProcedure:
                    return SqlReconQueryBuilder.BuildStoredProcedureQuery(computerName, instance, databaseName, filters);
                case SqlReconOperation.StoredProcedureClr:
                    return SqlReconQueryBuilder.BuildStoredProcedureClrQuery(computerName, instance, databaseName, filters);
                case SqlReconOperation.StoredProcedureSqli:
                    return SqlReconQueryBuilder.BuildStoredProcedureSqliQuery(computerName, instance, databaseName, filters);
                case SqlReconOperation.StoredProcedureXp:
                    return SqlReconQueryBuilder.BuildStoredProcedureXpQuery(computerName, instance, databaseName, filters);
                case SqlReconOperation.TriggerDml:
                    return SqlReconQueryBuilder.BuildTriggerDmlQuery(computerName, instance, databaseName, filters);
                case SqlReconOperation.AssemblyFile:
                    return SqlReconQueryBuilder.BuildAssemblyFileQuery(computerName, instance, databaseName, filters);
                default:
                    return null;
            }
        }

        private IReadOnlyList<IDictionary<string, object>> ExecuteColumnSampleData(
            SqlConnectionOptions connectionOptions,
            ReconQueryFilters filters,
            string computerName,
            string instance,
            CancellationToken cancellationToken)
        {
            var columnFilters = CloneDatabaseFilters(filters);
            columnFilters.HasAccess = true;
            columnFilters.ColumnNameSearch = !string.IsNullOrWhiteSpace(filters.Keywords)
                ? filters.Keywords
                : filters.ColumnNameSearch;

            if (string.IsNullOrWhiteSpace(columnFilters.ColumnNameSearch))
            {
                columnFilters.ColumnNameSearch = "Password";
            }

            var columns = ExecuteForEachDatabase(
                SqlReconOperation.Column,
                connectionOptions,
                columnFilters,
                computerName,
                instance,
                cancellationToken);

            var results = new List<IDictionary<string, object>>();
            foreach (var column in columns)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var database = SqlQueryRowReader.GetString(column, "DatabaseName");
                var schema = SqlQueryRowReader.GetString(column, "SchemaName");
                var table = SqlQueryRowReader.GetString(column, "TableName");
                var columnName = SqlQueryRowReader.GetString(column, "ColumnName");
                if (string.IsNullOrWhiteSpace(database)
                    || string.IsNullOrWhiteSpace(schema)
                    || string.IsNullOrWhiteSpace(table)
                    || string.IsNullOrWhiteSpace(columnName))
                {
                    continue;
                }

                var scopedOptions = ChangeDatabase(connectionOptions, database);
                var rowCountRows = ExecuteRawQuery(
                    scopedOptions,
                    SqlReconQueryBuilder.BuildColumnRowCountQuery(database, schema, table, columnName),
                    cancellationToken);
                var rowCount = rowCountRows.Count > 0
                    ? SqlQueryRowReader.GetString(rowCountRows[0], "NumRows")
                    : "0";

                if (filters.NoOutput)
                {
                    var summary = CreateSampleRow(
                        computerName,
                        instance,
                        database,
                        schema,
                        table,
                        columnName,
                        string.Empty,
                        rowCount,
                        filters.ValidateCc,
                        null);
                    results.Add(summary);
                    continue;
                }

                var sampleRows = ExecuteRawQuery(
                    scopedOptions,
                    SqlReconQueryBuilder.BuildColumnSampleQuery(
                        database,
                        schema,
                        table,
                        columnName,
                        filters.SampleSize),
                    cancellationToken);

                if (sampleRows.Count == 0)
                {
                    results.Add(CreateSampleRow(
                        computerName,
                        instance,
                        database,
                        schema,
                        table,
                        columnName,
                        string.Empty,
                        rowCount,
                        filters.ValidateCc,
                        null));
                    continue;
                }

                var sampleColumn = sampleRows[0].Keys.FirstOrDefault(key =>
                    !string.Equals(key, "ComputerName", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(key, "Instance", StringComparison.OrdinalIgnoreCase));

                foreach (var sampleRow in sampleRows)
                {
                    var sampleValue = sampleColumn != null && sampleRow.TryGetValue(sampleColumn, out var value)
                        ? value?.ToString() ?? string.Empty
                        : string.Empty;

                    results.Add(CreateSampleRow(
                        computerName,
                        instance,
                        database,
                        schema,
                        table,
                        columnName,
                        sampleValue,
                        rowCount,
                        filters.ValidateCc,
                        sampleValue));
                }
            }

            return results;
        }

        private static Dictionary<string, object> CreateSampleRow(
            string computerName,
            string instance,
            string database,
            string schema,
            string table,
            string column,
            string sample,
            string rowCount,
            bool validateCc,
            string sampleForValidation)
        {
            var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["ComputerName"] = computerName,
                ["Instance"] = instance,
                ["Database"] = database,
                ["Schema"] = schema,
                ["Table"] = table,
                ["Column"] = column,
                ["Sample"] = sample,
                ["RowCount"] = rowCount
            };

            if (validateCc)
            {
                row["IsCC"] = LuhnValidator.IsValidCreditCard(sampleForValidation ?? sample);
            }

            return row;
        }

        private IReadOnlyList<IDictionary<string, object>> ExecuteServiceAccount(
            SqlConnectionOptions connectionOptions,
            string computerName,
            string instance,
            CancellationToken cancellationToken)
        {
            var isSysadmin = string.Equals(
                QueryIsSysadmin(connectionOptions, cancellationToken),
                "Yes",
                StringComparison.OrdinalIgnoreCase);

            var query = SqlReconQueryBuilder.BuildServiceAccountQuery(computerName, instance, isSysadmin);
            var rows = ExecuteRawQuery(connectionOptions, query, cancellationToken);
            if (rows.Count == 0)
            {
                return rows;
            }

            var source = rows[0];
            var pivoted = new List<IDictionary<string, object>>
            {
                CreateServiceAccountRow(source, "AgentService", "AgentLogin"),
                CreateServiceAccountRow(source, "AnalysisService", "AnalysisLogin"),
                CreateServiceAccountRow(source, "BrowserService", "BrowserLogin"),
                CreateServiceAccountRow(source, "SQLService", "DBEngineLogin"),
                CreateServiceAccountRow(source, "IntegrationService", "IntegrationLogin"),
                CreateServiceAccountRow(source, "ReportService", "ReportLogin"),
                CreateServiceAccountRow(source, "WriterService", "WriterLogin")
            };

            return pivoted;
        }

        private static Dictionary<string, object> CreateServiceAccountRow(
            IDictionary<string, object> source,
            string serviceType,
            string accountColumn)
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["ComputerName"] = SqlQueryRowReader.GetString(source, "ComputerName"),
                ["Instance"] = SqlQueryRowReader.GetString(source, "Instance"),
                ["ServiceType"] = serviceType,
                ["ServiceAccount"] = SqlQueryRowReader.GetString(source, accountColumn)
            };
        }

        private IReadOnlyList<IDictionary<string, object>> ExecuteAgentJob(
            SqlConnectionOptions connectionOptions,
            ReconQueryFilters filters,
            string computerName,
            string instance,
            CancellationToken cancellationToken)
        {
            var scopedOptions = ChangeDatabase(connectionOptions, "msdb");
            var rows = ExecuteRawQuery(
                scopedOptions,
                SqlReconQueryBuilder.BuildAgentJobQuery(filters),
                cancellationToken);

            var normalized = new List<IDictionary<string, object>>(rows.Count);
            foreach (var row in rows)
            {
                var copy = new Dictionary<string, object>(row, StringComparer.OrdinalIgnoreCase)
                {
                    ["ComputerName"] = computerName,
                    ["Instance"] = instance
                };
                normalized.Add(copy);
            }

            return normalized;
        }

        private IReadOnlyList<IDictionary<string, object>> ExecuteRecoverPwAutoLogon(
            SqlConnectionOptions connectionOptions,
            string computerName,
            string instance,
            CancellationToken cancellationToken)
        {
            var results = new List<IDictionary<string, object>>();
            foreach (var alternate in new[] { false, true })
            {
                var rows = ExecuteRawQuery(
                    connectionOptions,
                    SqlReconQueryBuilder.BuildRecoverPwAutoLogonQuery(alternate),
                    cancellationToken);

                foreach (var row in rows)
                {
                    results.Add(new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["ComputerName"] = computerName,
                        ["Instance"] = instance,
                        ["Domain"] = SqlQueryRowReader.GetString(row, "Domain"),
                        ["UserName"] = SqlQueryRowReader.GetString(row, "Username"),
                        ["Password"] = SqlQueryRowReader.GetString(row, "Password")
                    });
                }
            }

            return results;
        }

        private IReadOnlyList<IDictionary<string, object>> ExecuteServerPasswordHash(
            SqlConnectionOptions connectionOptions,
            string computerName,
            string instance,
            ReconQueryFilters filters,
            CancellationToken cancellationToken)
        {
            if (filters.Migrate)
            {
                return Array.Empty<IDictionary<string, object>>();
            }

            var majorVersion = GetSqlMajorVersion(connectionOptions, cancellationToken);
            var query = SqlReconQueryBuilder.BuildServerPasswordHashQuery(computerName, instance, majorVersion);
            var rows = ExecuteRawQuery(connectionOptions, query, cancellationToken);

            if (string.IsNullOrWhiteSpace(filters?.PrincipalName))
            {
                return rows;
            }

            return rows
                .Where(row => SqlQueryRowReader.GetString(row, "PrincipalName")
                    .IndexOf(filters.PrincipalName, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        private IReadOnlyList<string> EnumerateDatabaseNames(
            SqlConnectionOptions connectionOptions,
            ReconQueryFilters filters,
            string computerName,
            string instance,
            CancellationToken cancellationToken)
        {
            var rows = ExecuteDatabase(connectionOptions, filters, computerName, instance, cancellationToken);
            return rows
                .Select(row => SqlQueryRowReader.GetString(row, "DatabaseName"))
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private IReadOnlyList<IDictionary<string, object>> ExecuteRawQuery(
            SqlConnectionOptions connectionOptions,
            string query,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Array.Empty<IDictionary<string, object>>();
            }

            using (var connection = _connectionFactory.CreateConnection(
                _connectionFactory.BuildConnectionString(connectionOptions)))
            {
                connection.Open();
                cancellationToken.ThrowIfCancellationRequested();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    command.CommandType = CommandType.Text;

                    using (var reader = command.ExecuteReader())
                    {
                        return SqlQueryRowReader.ReadAllRows(reader);
                    }
                }
            }
        }

        private int GetSqlMajorVersion(SqlConnectionOptions connectionOptions, CancellationToken cancellationToken)
        {
            var rows = ExecuteRawQuery(
                connectionOptions,
                "SELECT CAST(SERVERPROPERTY('ProductMajorVersion') AS INT) AS [MajorVersion]",
                cancellationToken);

            if (rows.Count == 0)
            {
                return 10;
            }

            var value = rows[0].TryGetValue("MajorVersion", out var majorVersion) ? majorVersion : null;
            if (value == null || value == DBNull.Value)
            {
                return 10;
            }

            return Convert.ToInt32(value);
        }

        private string QueryIsSysadmin(SqlConnectionOptions connectionOptions, CancellationToken cancellationToken)
        {
            var rows = ExecuteRawQuery(connectionOptions, SqlServerInfoQueryBuilder.SysadminCheckQuery, cancellationToken);
            if (rows.Count == 0)
            {
                return "No";
            }

            foreach (var row in rows)
            {
                foreach (var value in row.Values)
                {
                    if (value == null || value == DBNull.Value)
                    {
                        continue;
                    }

                    return value.ToString();
                }
            }

            return "No";
        }

        private bool IsAccessible(SqlConnectionOptions options, CancellationToken cancellationToken)
        {
            var testResult = _connectionTestService.TestConnection(options, cancellationToken);
            return testResult != null && testResult.Success;
        }

        private static ReconQueryFilters CloneDatabaseFilters(ReconQueryFilters filters)
        {
            return new ReconQueryFilters
            {
                DatabaseName = filters?.DatabaseName,
                NoDefaults = filters?.NoDefaults ?? false,
                HasAccess = filters?.HasAccess ?? false,
                ColumnName = filters?.ColumnName,
                ColumnNameSearch = filters?.ColumnNameSearch ?? filters?.Keywords
            };
        }

        private static SqlConnectionOptions ChangeDatabase(SqlConnectionOptions template, string database)
        {
            var clone = CloneOptions(template, template.Instance);
            clone.Database = database;
            return clone;
        }

        private static string ResolveInstance(string instance)
        {
            return string.IsNullOrWhiteSpace(instance) ? Environment.MachineName : instance;
        }

        private static SqlConnectionOptions CloneOptions(SqlConnectionOptions template, string instance)
        {
            return new SqlConnectionOptions
            {
                Instance = instance,
                Username = template.Username,
                Password = template.Password,
                Dac = template.Dac,
                Database = template.Database,
                ConnectTimeoutSeconds = template.ConnectTimeoutSeconds
            };
        }
    }

    internal static class LuhnValidator
    {
        internal static bool IsValidCreditCard(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var digits = new List<int>();
            foreach (var character in value)
            {
                if (char.IsDigit(character))
                {
                    digits.Add(character - '0');
                }
            }

            if (digits.Count < 13)
            {
                return false;
            }

            var sum = 0;
            var alternate = false;
            for (var i = digits.Count - 1; i >= 0; i--)
            {
                var digit = digits[i];
                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9)
                    {
                        digit -= 9;
                    }
                }

                sum += digit;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }
    }
}
