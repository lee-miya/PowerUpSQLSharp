using System;
using System.Data;
using PowerUpSQLSharp.Core.Models;

namespace PowerUpSQLSharp.Core.Utils
{
    internal static class SqlExtendedProcedureInspector
    {
        internal static void ApplyStatus(SqlServerInfo info, IDbConnection connection)
        {
            if (info == null || connection == null)
            {
                return;
            }

            info.XpCmdShellEnabled = QueryConfigureEnabled(connection, SqlServerInfoQueryBuilder.XpCmdShellConfigureQuery);

            var permissions = QueryExecutePermissions(connection);
            info.XpDirtreeEnabled = permissions.XpDirtreeEnabled;
            info.XpFileexistEnabled = permissions.XpFileexistEnabled;
            info.XpSubdirsEnabled = permissions.XpSubdirsEnabled;
        }

        private static string QueryConfigureEnabled(IDbConnection connection, string query)
        {
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    command.CommandType = CommandType.Text;

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return string.Empty;
                        }

                        var configValue = ReadColumnValue(reader, "config_value");
                        return ToYesNoFromConfigValue(configValue);
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private static ExecutePermissions QueryExecutePermissions(IDbConnection connection)
        {
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = SqlServerInfoQueryBuilder.XpExecutePermissionsQuery;
                    command.CommandType = CommandType.Text;

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return ExecutePermissions.Empty;
                        }

                        return new ExecutePermissions
                        {
                            XpDirtreeEnabled = GetYesNoColumn(reader, "XpDirtreeEnabled"),
                            XpFileexistEnabled = GetYesNoColumn(reader, "XpFileexistEnabled"),
                            XpSubdirsEnabled = GetYesNoColumn(reader, "XpSubdirsEnabled")
                        };
                    }
                }
            }
            catch
            {
                return ExecutePermissions.Empty;
            }
        }

        private static string GetYesNoColumn(IDataReader reader, string columnName)
        {
            var value = ReadColumnValue(reader, columnName);
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            var text = value.ToString();
            return string.IsNullOrWhiteSpace(text) ? string.Empty : text;
        }

        private static string ToYesNoFromConfigValue(object configValue)
        {
            if (configValue == null || configValue == DBNull.Value)
            {
                return string.Empty;
            }

            if (configValue is int intValue)
            {
                return intValue == 1 ? "Yes" : "No";
            }

            return int.TryParse(configValue.ToString(), out var parsed) && parsed == 1 ? "Yes" : "No";
        }

        private static object ReadColumnValue(IDataReader reader, string columnName)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
            }

            return null;
        }

        private sealed class ExecutePermissions
        {
            internal static ExecutePermissions Empty { get; } = new ExecutePermissions();

            internal string XpDirtreeEnabled { get; set; } = string.Empty;

            internal string XpFileexistEnabled { get; set; } = string.Empty;

            internal string XpSubdirsEnabled { get; set; } = string.Empty;
        }
    }
}
