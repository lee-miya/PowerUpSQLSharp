using System;
using System.Collections.Generic;
using System.Data;

namespace PowerUpSQLSharp.Core.Utils
{
    internal static class SqlQueryRowReader
    {
        internal static List<Dictionary<string, object>> ReadAllRows(IDataReader reader)
        {
            var rows = new List<Dictionary<string, object>>();
            if (reader == null)
            {
                return rows;
            }

            while (reader.Read())
            {
                rows.Add(ReadRow(reader));
            }

            return rows;
        }

        internal static Dictionary<string, object> ReadRow(IDataReader reader)
        {
            var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }

            return row;
        }

        internal static string GetString(IReadOnlyDictionary<string, object> row, string columnName)
        {
            if (row == null || !row.TryGetValue(columnName, out var value) || value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            return value.ToString();
        }
    }
}
