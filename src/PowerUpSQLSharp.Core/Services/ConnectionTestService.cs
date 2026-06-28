using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Utils;

namespace PowerUpSQLSharp.Core.Services
{
    public sealed class ConnectionTestService : IConnectionTestService
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public ConnectionTestService(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public SqlConnectionTestResult TestConnection(
            SqlConnectionOptions options,
            CancellationToken cancellationToken = default)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var instance = string.IsNullOrWhiteSpace(options.Instance)
                ? Environment.MachineName
                : options.Instance;

            var computerName = SqlInstanceParser.GetComputerNameFromInstance(instance);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (var connection = _connectionFactory.CreateConnection(
                    _connectionFactory.BuildConnectionString(CloneOptions(options, instance))))
                {
                    connection.Open();
                }

                return new SqlConnectionTestResult
                {
                    ComputerName = computerName,
                    Instance = instance,
                    Status = "Accessible",
                    Success = true
                };
            }
            catch
            {
                return new SqlConnectionTestResult
                {
                    ComputerName = computerName,
                    Instance = instance,
                    Status = "Not Accessible",
                    Success = false
                };
            }
        }

        public IReadOnlyList<SqlConnectionTestResult> TestConnectionsThreaded(
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
                    var result = TestConnection(CloneOptions(templateOptions, instance), token);
                    return new[] { result };
                },
                cancellationToken);
        }

        public IReadOnlyList<SqlDefaultPasswordMatch> TestDefaultPasswords(
            string instance,
            int connectTimeoutSeconds,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(instance))
            {
                instance = Environment.MachineName;
            }

            var namedInstance = SqlInstanceParser.GetNamedInstance(instance);
            if (string.IsNullOrWhiteSpace(namedInstance))
            {
                return Array.Empty<SqlDefaultPasswordMatch>();
            }

            var entries = SqlDefaultPasswordDatabase.GetEntriesForInstanceName(namedInstance);
            if (entries.Count == 0)
            {
                return Array.Empty<SqlDefaultPasswordMatch>();
            }

            var computerName = SqlInstanceParser.GetComputerNameFromInstance(instance);
            var matches = new List<SqlDefaultPasswordMatch>();

            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var probe = ProbeLogin(
                    new SqlConnectionOptions
                    {
                        Instance = instance,
                        Username = entry.Username,
                        Password = entry.Password,
                        ConnectTimeoutSeconds = connectTimeoutSeconds
                    },
                    cancellationToken);

                if (!probe.Connected)
                {
                    continue;
                }

                matches.Add(new SqlDefaultPasswordMatch
                {
                    Computer = computerName,
                    Instance = instance,
                    Username = entry.Username,
                    Password = entry.Password,
                    IsSysadmin = probe.IsSysadmin ? "Yes" : "No"
                });
            }

            return matches;
        }

        private LoginProbeResult ProbeLogin(SqlConnectionOptions options, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (var connection = _connectionFactory.CreateConnection(
                    _connectionFactory.BuildConnectionString(options)))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT IS_SRVROLEMEMBER('sysadmin')";
                        command.CommandType = CommandType.Text;
                        var scalar = command.ExecuteScalar();
                        var isSysadmin = scalar != null
                            && scalar != DBNull.Value
                            && Convert.ToInt32(scalar) == 1;

                        return new LoginProbeResult { Connected = true, IsSysadmin = isSysadmin };
                    }
                }
            }
            catch
            {
                return new LoginProbeResult { Connected = false, IsSysadmin = false };
            }
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

        private sealed class LoginProbeResult
        {
            public bool Connected { get; set; }
            public bool IsSysadmin { get; set; }
        }
    }
}
