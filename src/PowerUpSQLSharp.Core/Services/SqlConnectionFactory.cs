using System;
using System.Data.SqlClient;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Utils;

namespace PowerUpSQLSharp.Core.Services
{
    public interface ISqlConnectionFactory
    {
        SqlConnection CreateConnection(string connectionString);

        string BuildConnectionString(SqlConnectionOptions options);

        string BuildConnectionString(string instance, string username, string password, bool integratedSecurity);
    }

    public sealed class SqlConnectionFactory : ISqlConnectionFactory
    {
        public SqlConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public string BuildConnectionString(SqlConnectionOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var instance = string.IsNullOrWhiteSpace(options.Instance)
                ? Environment.MachineName
                : options.Instance;

            var dataSource = options.Dac ? "ADMIN:" + instance : instance;
            var database = string.IsNullOrWhiteSpace(options.Database) ? "Master" : options.Database;
            var timeout = options.ConnectTimeoutSeconds > 0 ? options.ConnectTimeoutSeconds : 30;

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = dataSource,
                InitialCatalog = database,
                ConnectTimeout = timeout
            };

            if (string.IsNullOrWhiteSpace(options.Username))
            {
                builder.IntegratedSecurity = true;
            }
            else if (options.Username.IndexOf('\\') >= 0)
            {
                builder.IntegratedSecurity = true;
                builder.UserID = options.Username;
                builder.Password = options.Password ?? string.Empty;
            }
            else
            {
                builder.UserID = options.Username;
                builder.Password = options.Password ?? string.Empty;
            }

            return builder.ConnectionString;
        }

        public string BuildConnectionString(string instance, string username, string password, bool integratedSecurity)
        {
            return BuildConnectionString(new SqlConnectionOptions
            {
                Instance = instance,
                Username = integratedSecurity ? null : username,
                Password = password
            });
        }
    }
}
