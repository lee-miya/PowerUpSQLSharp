using System;

using System.Collections.Generic;

using System.Data;

using System.Threading;

using PowerUpSQLSharp.Core.Models;

using PowerUpSQLSharp.Core.Utils;



namespace PowerUpSQLSharp.Core.Services

{

    public sealed class ReconnaissanceService : IReconnaissanceService

    {

        private readonly ISqlConnectionFactory _connectionFactory;

        private readonly IConnectionTestService _connectionTestService;

        private readonly ReconQueryRunner _reconQueryRunner;



        public ReconnaissanceService(

            ISqlConnectionFactory connectionFactory,

            IConnectionTestService connectionTestService)

        {

            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

            _connectionTestService = connectionTestService ?? throw new ArgumentNullException(nameof(connectionTestService));

            _reconQueryRunner = new ReconQueryRunner(connectionFactory, connectionTestService);

        }



        public SqlServerInfo GetServerInfo(

            SqlConnectionOptions options,

            CancellationToken cancellationToken = default)

        {

            if (options == null)

            {

                throw new ArgumentNullException(nameof(options));

            }



            var instance = ResolveInstance(options.Instance);

            var computerName = SqlInstanceParser.GetComputerNameFromInstance(instance);

            var connectionOptions = CloneOptions(options, instance);



            if (!IsAccessible(connectionOptions, cancellationToken))

            {

                return null;

            }



            return CollectServerInfo(connectionOptions, computerName, cancellationToken);

        }



        public IReadOnlyList<SqlServerInfo> GetServerInfoThreaded(

            IEnumerable<string> instances,

            SqlConnectionOptions templateOptions,

            int maxThreads,

            CancellationToken cancellationToken = default)

        {

            if (templateOptions == null)

            {

                throw new ArgumentNullException(nameof(templateOptions));

            }



            return ThreadedExecutor.Execute(

                instances,

                maxThreads,

                (instance, token) =>

                {

                    var info = GetServerInfo(CloneOptions(templateOptions, instance), token);

                    return info == null

                        ? Array.Empty<SqlServerInfo>()

                        : new[] { info };

                },

                cancellationToken);

        }

        public IReadOnlyList<SqlServerConfigurationEntry> GetServerConfiguration(
            SqlConnectionOptions options,
            CancellationToken cancellationToken = default)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var instance = ResolveInstance(options.Instance);
            var computerName = SqlInstanceParser.GetComputerNameFromInstance(instance);
            var connectionOptions = CloneOptions(options, instance);

            if (!IsAccessible(connectionOptions, cancellationToken))
            {
                return Array.Empty<SqlServerConfigurationEntry>();
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (var connection = _connectionFactory.CreateConnection(
                    _connectionFactory.BuildConnectionString(connectionOptions)))
                {
                    connection.Open();
                    return CollectServerConfiguration(connection, computerName, instance);
                }
            }
            catch
            {
                return Array.Empty<SqlServerConfigurationEntry>();
            }
        }

        public IReadOnlyList<SqlServerConfigurationEntry> GetServerConfigurationThreaded(
            IEnumerable<string> instances,
            SqlConnectionOptions templateOptions,
            int maxThreads,
            CancellationToken cancellationToken = default)
        {
            if (templateOptions == null)
            {
                throw new ArgumentNullException(nameof(templateOptions));
            }

            return ThreadedExecutor.Execute(
                instances,
                maxThreads,
                (instance, token) => GetServerConfiguration(CloneOptions(templateOptions, instance), token),
                cancellationToken);
        }

        public IReadOnlyList<IDictionary<string, object>> Query(
            SqlReconOperation operation,
            SqlConnectionOptions options,
            ReconQueryFilters filters,
            CancellationToken cancellationToken = default)
        {
            return _reconQueryRunner.Execute(operation, options, filters, cancellationToken);
        }

        public IReadOnlyList<IDictionary<string, object>> QueryThreaded(
            SqlReconOperation operation,
            IEnumerable<string> instances,
            SqlConnectionOptions templateOptions,
            ReconQueryFilters filters,
            int maxThreads,
            CancellationToken cancellationToken = default)
        {
            if (templateOptions == null)
            {
                throw new ArgumentNullException(nameof(templateOptions));
            }

            return ThreadedExecutor.Execute(
                instances,
                maxThreads,
                (instance, token) => Query(
                    operation,
                    CloneOptions(templateOptions, instance),
                    filters,
                    token),
                cancellationToken);
        }

        private IReadOnlyList<SqlServerConfigurationEntry> CollectServerConfiguration(
            IDbConnection connection,
            string computerName,
            string instance)
        {
            var isSysadmin = string.Equals(QueryIsSysadmin(connection), "Yes", StringComparison.OrdinalIgnoreCase);
            var restoreAdvancedOptions = false;

            if (!IsShowAdvancedOptionsEnabled(connection))
            {
                if (isSysadmin)
                {
                    ExecuteNonQuery(connection, "EXEC sp_configure 'Show Advanced Options', 1; RECONFIGURE");
                    restoreAdvancedOptions = IsShowAdvancedOptionsEnabled(connection);
                }
            }

            var entries = QuerySpConfigure(connection, computerName, instance);

            if (restoreAdvancedOptions && isSysadmin)
            {
                ExecuteNonQuery(connection, "EXEC sp_configure 'Show Advanced Options', 0; RECONFIGURE");
            }

            return entries;
        }

        private static bool IsShowAdvancedOptionsEnabled(IDbConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "EXEC sp_configure 'Show Advanced Options'";
                command.CommandType = CommandType.Text;

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return false;
                    }

                    return ReadInt32(reader, "config_value") == 1;
                }
            }
        }

        private static List<SqlServerConfigurationEntry> QuerySpConfigure(
            IDbConnection connection,
            string computerName,
            string instance)
        {
            var entries = new List<SqlServerConfigurationEntry>();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "EXEC sp_configure";
                command.CommandType = CommandType.Text;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        entries.Add(new SqlServerConfigurationEntry
                        {
                            ComputerName = computerName,
                            Instance = instance,
                            Name = ReadString(reader, "name"),
                            Minimum = ReadString(reader, "minimum"),
                            Maximum = ReadString(reader, "maximum"),
                            ConfigValue = ReadString(reader, "config_value"),
                            RunValue = ReadString(reader, "run_value")
                        });
                    }
                }
            }

            return entries;
        }

        private static void ExecuteNonQuery(IDbConnection connection, string sql)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
            }
        }

        private static string ReadString(IDataReader reader, string columnName)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (!string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return reader.IsDBNull(i) ? string.Empty : reader.GetValue(i).ToString();
            }

            return string.Empty;
        }

        private static int ReadInt32(IDataReader reader, string columnName)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (!string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (reader.IsDBNull(i))
                {
                    return 0;
                }

                return Convert.ToInt32(reader.GetValue(i));
            }

            return 0;
        }



        private SqlServerInfo CollectServerInfo(

            SqlConnectionOptions options,

            string computerName,

            CancellationToken cancellationToken)

        {

            try

            {

                cancellationToken.ThrowIfCancellationRequested();

                using (var connection = _connectionFactory.CreateConnection(

                    _connectionFactory.BuildConnectionString(options)))

                {

                    connection.Open();



                    var isSysadmin = QueryIsSysadmin(connection);

                    var activeSessions = QueryActiveSessionCount(connection).ToString();

                    var query = SqlServerInfoQueryBuilder.BuildServerInfoQuery(

                        computerName,

                        isSysadmin,

                        activeSessions);

                    var row = ExecuteSingleRow(connection, query);

                    if (row == null)
                    {
                        return null;
                    }

                    var info = MapServerInfo(row);
                    SqlExtendedProcedureInspector.ApplyStatus(info, connection);
                    return info;

                }

            }

            catch

            {

                return null;

            }

        }



        private bool IsAccessible(SqlConnectionOptions options, CancellationToken cancellationToken)

        {

            var testResult = _connectionTestService.TestConnection(options, cancellationToken);

            return testResult != null && testResult.Success;

        }



        private static string QueryIsSysadmin(IDbConnection connection)

        {

            using (var command = connection.CreateCommand())

            {

                command.CommandText = SqlServerInfoQueryBuilder.SysadminCheckQuery;

                command.CommandType = CommandType.Text;

                var scalar = command.ExecuteScalar();

                return scalar == null || scalar == DBNull.Value

                    ? "No"

                    : scalar.ToString();

            }

        }



        private static int QueryActiveSessionCount(IDbConnection connection)

        {

            var count = 0;



            using (var command = connection.CreateCommand())

            {

                command.CommandText = SqlServerInfoQueryBuilder.ActiveSessionsQuery;

                command.CommandType = CommandType.Text;



                using (var reader = command.ExecuteReader())

                {

                    while (reader.Read())

                    {

                        if (reader.IsDBNull(0))

                        {

                            continue;

                        }



                        var status = reader.GetString(0);

                        if (string.Equals(status, "running", StringComparison.OrdinalIgnoreCase))

                        {

                            count++;

                        }

                    }

                }

            }



            return count;

        }



        private static Dictionary<string, object> ExecuteSingleRow(IDbConnection connection, string query)

        {

            using (var command = connection.CreateCommand())

            {

                command.CommandText = query;

                command.CommandType = CommandType.Text;



                using (var reader = command.ExecuteReader())

                {

                    if (!reader.Read())

                    {

                        return null;

                    }



                    var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                    for (var i = 0; i < reader.FieldCount; i++)

                    {

                        row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);

                    }



                    return row;

                }

            }

        }



        private static SqlServerInfo MapServerInfo(IReadOnlyDictionary<string, object> row)

        {

            return new SqlServerInfo

            {

                ComputerName = GetString(row, "ComputerName"),

                Instance = GetString(row, "Instance"),

                DomainName = GetString(row, "DomainName"),

                ServiceProcessId = GetString(row, "ServiceProcessID"),

                ServiceName = GetString(row, "ServiceName"),

                ServiceAccount = GetString(row, "ServiceAccount"),

                AuthenticationMode = GetString(row, "AuthenticationMode"),

                ForcedEncryption = GetString(row, "ForcedEncryption"),

                Clustered = GetString(row, "Clustered"),

                SqlServerVersionNumber = GetString(row, "SQLServerVersionNumber"),

                SqlServerMajorVersion = GetString(row, "SQLServerMajorVersion"),

                SqlServerEdition = GetString(row, "SQLServerEdition"),

                SqlServerServicePack = GetString(row, "SQLServerServicePack"),

                OsArchitecture = GetString(row, "OSArchitecture"),

                OsMachineType = GetString(row, "OsMachineType"),

                OsVersionName = GetString(row, "OSVersionName"),

                OsVersionNumber = GetString(row, "OsVersionNumber"),

                CurrentLogin = GetString(row, "Currentlogin"),

                IsSysadmin = GetString(row, "IsSysadmin"),

                ActiveSessions = GetString(row, "ActiveSessions")

            };

        }



        private static string GetString(IReadOnlyDictionary<string, object> row, string columnName)

        {

            if (row == null || !row.TryGetValue(columnName, out var value) || value == null || value == DBNull.Value)

            {

                return string.Empty;

            }



            return value.ToString();

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

}

