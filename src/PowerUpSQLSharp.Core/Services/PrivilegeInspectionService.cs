using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Utils;

namespace PowerUpSQLSharp.Core.Services
{
    public sealed class PrivilegeInspectionService : IPrivilegeInspectionService
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly IConnectionTestService _connectionTestService;
        private readonly ReconQueryRunner _reconQueryRunner;

        public PrivilegeInspectionService(
            ISqlConnectionFactory connectionFactory,
            IConnectionTestService connectionTestService)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _connectionTestService = connectionTestService ?? throw new ArgumentNullException(nameof(connectionTestService));
            _reconQueryRunner = new ReconQueryRunner(connectionFactory, connectionTestService);
        }

        public SqlSysadminStatus GetSysadminStatus(
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
            var connectionOptions = CloneOptions(options, instance);

            var testResult = _connectionTestService.TestConnection(connectionOptions, cancellationToken);
            if (testResult == null || !testResult.Success)
            {
                return null;
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (var connection = _connectionFactory.CreateConnection(
                    _connectionFactory.BuildConnectionString(connectionOptions)))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = SqlServerInfoQueryBuilder.SysadminCheckQuery;
                        command.CommandType = CommandType.Text;
                        var scalar = command.ExecuteScalar();
                        var isSysadmin = scalar != null
                            && scalar != DBNull.Value
                            && string.Equals(scalar.ToString(), "Yes", StringComparison.OrdinalIgnoreCase);

                        return new SqlSysadminStatus
                        {
                            ComputerName = computerName,
                            Instance = instance,
                            IsSysadmin = isSysadmin ? "Yes" : "No"
                        };
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public SqlLocalAdminStatus CheckLocalAdministrator()
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return new SqlLocalAdminStatus
            {
                CurrentUser = identity.Name ?? string.Empty,
                IsLocalAdmin = principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator)
            };
        }

        public IReadOnlyList<IDictionary<string, object>> GetDatabasePriv(
            SqlConnectionOptions options,
            ReconQueryFilters filters,
            CancellationToken cancellationToken = default)
        {
            return _reconQueryRunner.Execute(SqlReconOperation.DatabasePriv, options, filters, cancellationToken);
        }

        public IReadOnlyList<IDictionary<string, object>> GetServerPriv(
            SqlConnectionOptions options,
            ReconQueryFilters filters,
            CancellationToken cancellationToken = default)
        {
            return _reconQueryRunner.Execute(SqlReconOperation.ServerPriv, options, filters, cancellationToken);
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
