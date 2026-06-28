using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PowerUpSQLSharp.Core.Models;

namespace PowerUpSQLSharp.Core.Utils
{
    internal static class SqlReconQueryBuilder
    {
        internal static string BuildDatabaseQuery(
            string computerName,
            string instance,
            ReconQueryFilters filters,
            int sqlMajorVersion)
        {
            var queryStart = $@"
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    a.database_id AS [DatabaseId],
    a.name AS [DatabaseName],
    SUSER_SNAME(a.owner_sid) AS [DatabaseOwner],
    IS_SRVROLEMEMBER('sysadmin', SUSER_SNAME(a.owner_sid)) AS [OwnerIsSysadmin],
    a.is_trustworthy_on,
    a.is_db_chaining_on,";

            var versionColumns = sqlMajorVersion >= 10
                ? @"
    a.is_broker_enabled,
    a.is_encrypted,
    a.is_read_only,"
                : string.Empty;

            var queryEnd = $@"
    a.create_date,
    a.recovery_model_desc,
    b.filename AS [FileName],
    (SELECT CAST(SUM(size) * 8. / 1024 AS DECIMAL(8,2))
        FROM sys.master_files WHERE name LIKE a.name) AS [DbSizeMb],
    HAS_DBACCESS(a.name) AS [has_dbaccess]
FROM sys.databases a
INNER JOIN sys.sysdatabases b ON a.database_id = b.dbid
WHERE 1=1";

            return queryStart + versionColumns + queryEnd
                + BuildDatabaseNameFilter(filters)
                + BuildNoDefaultsFilter(filters)
                + BuildHasAccessFilter(filters)
                + BuildSysAdminOnlyFilter(filters)
                + " ORDER BY a.database_id";
        }

        internal static string BuildDatabaseUserQuery(
            string computerName,
            string instance,
            string databaseName,
            ReconQueryFilters filters)
        {
            return $@"
USE [{EscapeIdentifier(databaseName)}];
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    '{EscapeSqlLiteral(databaseName)}' AS [DatabaseName],
    a.principal_id AS [DatabaseUserId],
    a.name AS [DatabaseUser],
    a.sid AS [PrincipalSid],
    b.name AS [PrincipalName],
    a.type_desc AS [PrincipalType],
    a.default_schema_name AS [deault_schema_name],
    a.create_date,
    a.is_fixed_role
FROM sys.database_principals a
LEFT JOIN sys.server_principals b ON a.sid = b.sid
WHERE 1=1
{BuildLikeFilter("a.name", filters?.DatabaseUser, "DatabaseUser")}
{BuildLikeFilter("b.name", filters?.PrincipalName, "PrincipalName")}";
        }

        internal static string BuildDatabaseSchemaQuery(
            string computerName,
            string instance,
            string databaseName,
            ReconQueryFilters filters)
        {
            return $@"
USE [{EscapeIdentifier(databaseName)}];
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    DB_NAME() AS [DatabaseName],
    s.schema_id AS [SchemaId],
    s.name AS [SchemaName],
    s.principal_id AS [OwnerId],
    USER_NAME(s.principal_id) AS [OwnerName]
FROM sys.schemas s
JOIN [{EscapeIdentifier(databaseName)}].INFORMATION_SCHEMA.SCHEMATA i ON s.name = i.SCHEMA_NAME
{BuildContainsFilter("s.name", filters?.SchemaName, "SchemaName", useWhere: true)}
ORDER BY s.name";
        }

        internal static string BuildDatabaseRoleQuery(
            string computerName,
            string instance,
            string databaseName,
            ReconQueryFilters filters)
        {
            return $@"
USE [{EscapeIdentifier(databaseName)}];
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    '{EscapeSqlLiteral(databaseName)}' AS [DatabaseName],
    principal_id AS [RolePrincipalId],
    sid AS [RolePrincipalSid],
    name AS [RolePrincipalName],
    type_desc AS [RolePrincipalType],
    owning_principal_id AS [OwnerPrincipalId],
    SUSER_NAME(owning_principal_id) AS [OwnerPrincipalName],
    is_fixed_role,
    create_date,
    modify_date AS [modify_Date],
    default_schema_name
FROM [{EscapeIdentifier(databaseName)}].sys.database_principals
WHERE type LIKE 'R'
{BuildLikeFilter("name", filters?.RolePrincipalName, "RolePrincipalName")}
{BuildLikeFilter("SUSER_NAME(owning_principal_id)", filters?.RoleOwner, "RoleOwner")}";
        }

        internal static string BuildDatabaseRoleMemberQuery(
            string computerName,
            string instance,
            string databaseName,
            ReconQueryFilters filters)
        {
            return $@"
USE [{EscapeIdentifier(databaseName)}];
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    '{EscapeSqlLiteral(databaseName)}' AS [DatabaseName],
    role_principal_id AS [RolePrincipalId],
    USER_NAME(role_principal_id) AS [RolePrincipalName],
    member_principal_id AS [PrincipalId],
    USER_NAME(member_principal_id) AS [PrincipalName]
FROM [{EscapeIdentifier(databaseName)}].sys.database_role_members
WHERE 1=1
{BuildLikeFilter("USER_NAME(role_principal_id)", filters?.RolePrincipalName, "RolePrincipalName")}
{BuildLikeFilter("USER_NAME(member_principal_id)", filters?.PrincipalName, "PrincipalName")}";
        }

        internal static string BuildDatabasePrivQuery(
            string computerName,
            string instance,
            string databaseName,
            ReconQueryFilters filters)
        {
            return $@"
USE [{EscapeIdentifier(databaseName)}];
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    '{EscapeSqlLiteral(databaseName)}' AS [DatabaseName],
    rp.name AS [PrincipalName],
    rp.type_desc AS [PrincipalType],
    pm.class_desc AS [PermissionType],
    pm.permission_name AS [PermissionName],
    pm.state_desc AS [StateDescription],
    ObjectType = CASE
        WHEN obj.type_desc IS NULL OR obj.type_desc = 'SYSTEM_TABLE' THEN pm.class_desc
        ELSE obj.type_desc
    END,
    ObjectName = ISNULL(ss.name, OBJECT_NAME(pm.major_id))
FROM [{EscapeIdentifier(databaseName)}].sys.database_principals rp
INNER JOIN [{EscapeIdentifier(databaseName)}].sys.database_permissions pm ON pm.grantee_principal_id = rp.principal_id
LEFT JOIN [{EscapeIdentifier(databaseName)}].sys.schemas ss ON pm.major_id = ss.schema_id
LEFT JOIN [{EscapeIdentifier(databaseName)}].sys.objects obj ON pm.major_id = obj.object_id
WHERE 1=1
{BuildLikeFilter("pm.class_desc", filters?.PermissionType, "PermissionType")}
{BuildLikeFilter("pm.permission_name", filters?.PermissionName, "PermissionName")}
{BuildLikeFilter("rp.name", filters?.PrincipalName, "PrincipalName")}";
        }

        internal static string BuildTableQuery(
            string computerName,
            string instance,
            string databaseName,
            ReconQueryFilters filters)
        {
            var tableFilter = string.IsNullOrWhiteSpace(filters?.TableName)
                ? string.Empty
                : $" WHERE t.TABLE_NAME LIKE '%{EscapeSqlLiteral(filters.TableName)}%'";

            return $@"
USE [{EscapeIdentifier(databaseName)}];
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    t.TABLE_CATALOG AS [DatabaseName],
    t.TABLE_SCHEMA AS [SchemaName],
    t.TABLE_NAME AS [TableName],
    CASE
        WHEN (SELECT CASE WHEN LEN(t.TABLE_NAME) - LEN(REPLACE(t.TABLE_NAME,'#','')) > 1 THEN 1 ELSE 0 END) = 1 THEN 'GlobalTempTable'
        WHEN t.TABLE_NAME LIKE '%[_]%' AND (SELECT CASE WHEN LEN(t.TABLE_NAME) - LEN(REPLACE(t.TABLE_NAME,'#','')) = 1 THEN 1 ELSE 0 END) = 1 THEN 'LocalTempTable'
        WHEN t.TABLE_NAME NOT LIKE '%[_]%' AND (SELECT CASE WHEN LEN(t.TABLE_NAME) - LEN(REPLACE(t.TABLE_NAME,'#','')) = 1 THEN 1 ELSE 0 END) = 1 THEN 'TableVariable'
        ELSE t.TABLE_TYPE
    END AS [TableType],
    s.is_ms_shipped,
    s.is_published,
    s.is_schema_published,
    s.create_date,
    s.modify_date AS [modified_date]
FROM [{EscapeIdentifier(databaseName)}].INFORMATION_SCHEMA.TABLES t
JOIN sys.tables st ON t.TABLE_NAME = st.name AND t.TABLE_SCHEMA = OBJECT_SCHEMA_NAME(st.object_id)
JOIN sys.objects s ON st.object_id = s.object_id
{tableFilter}
ORDER BY t.TABLE_CATALOG, t.TABLE_SCHEMA, t.TABLE_NAME";
        }

        internal static string BuildTableTempQuery()
        {
            return @"
SELECT SERVERPROPERTY('MachineName') AS [Computer_Name],
    @@SERVERNAME AS [Instance],
    'tempdb' AS [Database_Name],
    SCHEMA_NAME(t1.schema_id) AS [Schema_Name],
    t1.name AS [Table_Name],
    CASE
        WHEN (SELECT CASE WHEN LEN(t1.name) - LEN(REPLACE(t1.name,'#','')) > 1 THEN 1 ELSE 0 END) = 1 THEN 'GlobalTempTable'
        WHEN t1.name LIKE '%[_]%' AND (SELECT CASE WHEN LEN(t1.name) - LEN(REPLACE(t1.name,'#','')) = 1 THEN 1 ELSE 0 END) = 1 THEN 'LocalTempTable'
        WHEN t1.name NOT LIKE '%[_]%' AND (SELECT CASE WHEN LEN(t1.name) - LEN(REPLACE(t1.name,'#','')) = 1 THEN 1 ELSE 0 END) = 1 THEN 'TableVariable'
        ELSE t1.type_desc
    END AS [Table_Type],
    t2.name AS [Column_Name],
    t3.name AS [Column_Type],
    t1.is_ms_shipped,
    t1.is_published,
    t1.is_schema_published,
    t1.create_date,
    t1.modify_date
FROM tempdb.sys.objects t1
JOIN tempdb.sys.columns t2 ON t1.object_id = t2.object_id
JOIN sys.types t3 ON t2.system_type_id = t3.system_type_id
WHERE t1.name LIKE '#%'";
        }

        internal static string BuildColumnQuery(
            string computerName,
            string instance,
            string databaseName,
            ReconQueryFilters filters)
        {
            var where = new StringBuilder(" WHERE 1=1");
            if (!string.IsNullOrWhiteSpace(filters?.ColumnNameSearch))
            {
                var keywords = filters.ColumnNameSearch
                    .Split(',')
                    .Select(value => value.Trim())
                    .Where(value => !string.IsNullOrEmpty(value))
                    .ToList();

                if (keywords.Count > 0)
                {
                    where.Append(" AND (");
                    for (var i = 0; i < keywords.Count; i++)
                    {
                        if (i > 0)
                        {
                            where.Append(" OR ");
                        }

                        where.Append("c.COLUMN_NAME LIKE '%").Append(EscapeSqlLiteral(keywords[i])).Append("%'");
                    }

                    where.Append(')');
                }
            }

            if (!string.IsNullOrWhiteSpace(filters?.ColumnName))
            {
                where.Append(" AND c.COLUMN_NAME LIKE '").Append(EscapeSqlLiteral(filters.ColumnName)).Append('\'');
            }

            if (!string.IsNullOrWhiteSpace(filters?.TableName))
            {
                where.Append(" AND t.TABLE_NAME LIKE '%").Append(EscapeSqlLiteral(filters.TableName)).Append("%'");
            }

            return $@"
USE [{EscapeIdentifier(databaseName)}];
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    t.TABLE_CATALOG AS [DatabaseName],
    t.TABLE_SCHEMA AS [SchemaName],
    t.TABLE_NAME AS [TableName],
    CASE
        WHEN (SELECT CASE WHEN LEN(t.TABLE_NAME) - LEN(REPLACE(t.TABLE_NAME,'#','')) > 1 THEN 1 ELSE 0 END) = 1 THEN 'GlobalTempTable'
        WHEN t.TABLE_NAME LIKE '%[_]%' AND (SELECT CASE WHEN LEN(t.TABLE_NAME) - LEN(REPLACE(t.TABLE_NAME,'#','')) = 1 THEN 1 ELSE 0 END) = 1 THEN 'LocalTempTable'
        WHEN t.TABLE_NAME NOT LIKE '%[_]%' AND (SELECT CASE WHEN LEN(t.TABLE_NAME) - LEN(REPLACE(t.TABLE_NAME,'#','')) = 1 THEN 1 ELSE 0 END) = 1 THEN 'TableVariable'
        ELSE t.TABLE_TYPE
    END AS [TableType],
    c.COLUMN_NAME AS [ColumnName],
    c.DATA_TYPE AS [ColumnDataType],
    c.CHARACTER_MAXIMUM_LENGTH AS [ColumnMaxLength],
    s.is_ms_shipped,
    s.is_published,
    s.is_schema_published,
    s.create_date,
    s.modify_date AS [modified_date]
FROM [{EscapeIdentifier(databaseName)}].INFORMATION_SCHEMA.TABLES t
JOIN sys.tables st ON t.TABLE_NAME = st.name AND t.TABLE_SCHEMA = OBJECT_SCHEMA_NAME(st.object_id)
JOIN sys.objects s ON st.object_id = s.object_id
LEFT JOIN sys.extended_properties ep ON s.object_id = ep.major_id AND ep.minor_id = 0
JOIN [{EscapeIdentifier(databaseName)}].INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME AND t.TABLE_SCHEMA = c.TABLE_SCHEMA
{where}
ORDER BY t.TABLE_CATALOG, t.TABLE_SCHEMA, t.TABLE_NAME, c.ORDINAL_POSITION";
        }

        internal static string BuildColumnSampleQuery(string databaseName, string schema, string table, string column, int sampleSize)
        {
            return $@"
USE [{EscapeIdentifier(databaseName)}];
SELECT TOP {Math.Max(1, sampleSize)} [{EscapeIdentifier(column)}]
FROM [{EscapeIdentifier(databaseName)}].[{EscapeIdentifier(schema)}].[{EscapeIdentifier(table)}]
WHERE [{EscapeIdentifier(column)}] IS NOT NULL";
        }

        internal static string BuildColumnRowCountQuery(string databaseName, string schema, string table, string column)
        {
            return $@"
USE [{EscapeIdentifier(databaseName)}];
SELECT COUNT(CAST([{EscapeIdentifier(column)}] AS VARCHAR(200))) AS [NumRows]
FROM [{EscapeIdentifier(databaseName)}].[{EscapeIdentifier(schema)}].[{EscapeIdentifier(table)}]
WHERE [{EscapeIdentifier(column)}] IS NOT NULL";
        }

        internal static string BuildViewQuery(
            string computerName,
            string instance,
            string databaseName,
            ReconQueryFilters filters)
        {
            var viewFilter = string.IsNullOrWhiteSpace(filters?.ViewName)
                ? string.Empty
                : $" WHERE TABLE_NAME LIKE '%{EscapeSqlLiteral(filters.ViewName)}%'";

            return $@"
USE [{EscapeIdentifier(databaseName)}];
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    TABLE_CATALOG AS [DatabaseName],
    TABLE_SCHEMA AS [SchemaName],
    TABLE_NAME AS [ViewName],
    VIEW_DEFINITION AS [ViewDefinition],
    IS_UPDATABLE AS [IsUpdatable],
    CHECK_OPTION AS [CheckOption]
FROM INFORMATION_SCHEMA.VIEWS
{viewFilter}
ORDER BY TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME";
        }

        internal static string BuildStoredProcedureQuery(
            string computerName,
            string instance,
            string databaseName,
            ReconQueryFilters filters)
        {
            var where = new StringBuilder(" WHERE 1=1");
            if (filters?.AutoExec == true)
            {
                where.Append(" AND b.is_auto_executed = 1");
            }

            if (!string.IsNullOrWhiteSpace(filters?.ProcedureName))
            {
                where.Append(" AND ROUTINE_NAME LIKE '").Append(EscapeSqlLiteral(filters.ProcedureName)).Append('\'');
            }

            if (!string.IsNullOrWhiteSpace(filters?.Keyword))
            {
                where.Append(" AND ROUTINE_DEFINITION LIKE '%").Append(EscapeSqlLiteral(filters.Keyword)).Append("%'");
            }

            return $@"
USE [{EscapeIdentifier(databaseName)}];
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    ROUTINE_CATALOG AS [DatabaseName],
    ROUTINE_SCHEMA AS [SchemaName],
    ROUTINE_NAME AS [ProcedureName],
    ROUTINE_TYPE AS [ProcedureType],
    ROUTINE_DEFINITION AS [ProcedureDefinition],
    SQL_DATA_ACCESS,
    ROUTINE_BODY,
    CREATED,
    LAST_ALTERED,
    b.is_ms_shipped,
    b.is_auto_executed
FROM INFORMATION_SCHEMA.ROUTINES a
JOIN sys.procedures b ON a.ROUTINE_NAME = b.name
{where}";
        }

        internal static string BuildStoredProcedureSqliQuery(
            string computerName,
            string instance,
            string databaseName,
            ReconQueryFilters filters)
        {
            if (filters?.OnlySigned == true)
            {
                return $@"
USE [{EscapeIdentifier(databaseName)}];
SELECT DB_NAME() AS [DB_NAME],
    SCHEMA_NAME(o.schema_id) AS [SCHEMA_NAME],
    o.name AS [SP_NAME],
    OBJECT_DEFINITION(o.object_id) AS [SP_CODE],
    c.name AS [CERT_NAME],
    p.name AS [CERT_LOGIN],
    p.sid AS [CERT_SID]
FROM sys.crypt_properties cp
JOIN sys.certificates c ON cp.thumbprint = c.thumbprint
JOIN sys.objects o ON cp.major_id = o.object_id
JOIN sys.server_principals p ON c.pvtkey_last_backup_date IS NOT NULL AND p.sid = c.pvtkey_last_backup_date
WHERE o.type = 'P'";
            }

            var where = new StringBuilder(@"
WHERE 1=1
AND (ROUTINE_DEFINITION LIKE '%sp_executesql%'
    OR ROUTINE_DEFINITION LIKE '%exec (@%'
    OR ROUTINE_DEFINITION LIKE '%exec(''%'
    OR ROUTINE_DEFINITION LIKE '%exec (N''%'
    OR ROUTINE_DEFINITION LIKE '%'''''' +%')
AND ROUTINE_DEFINITION LIKE '%+%'
AND ROUTINE_CATALOG NOT LIKE 'msdb'");

            if (!string.IsNullOrWhiteSpace(filters?.ProcedureName))
            {
                where.Append(" AND ROUTINE_NAME LIKE '").Append(EscapeSqlLiteral(filters.ProcedureName)).Append('\'');
            }

            if (!string.IsNullOrWhiteSpace(filters?.Keyword))
            {
                where.Append(" AND ROUTINE_DEFINITION LIKE '%").Append(EscapeSqlLiteral(filters.Keyword)).Append("%'");
            }

            return BuildStoredProcedureQuery(computerName, instance, databaseName, filters)
                .Replace("WHERE 1=1", where.ToString());
        }

        internal static string BuildStoredProcedureAutoExecQuery(
            string computerName,
            string instance,
            ReconQueryFilters filters)
        {
            var autoExecFilters = filters ?? new ReconQueryFilters();
            autoExecFilters.AutoExec = true;
            return BuildStoredProcedureQuery(computerName, instance, "master", autoExecFilters);
        }

        internal static string BuildStoredProcedureXpQuery(
            string computerName,
            string instance,
            string databaseName,
            ReconQueryFilters filters)
        {
            return $@"
USE [{EscapeIdentifier(databaseName)}];
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    '{EscapeSqlLiteral(databaseName)}' AS [DatabaseName],
    o.object_id,
    o.parent_object_id,
    o.schema_id,
    sc.name AS schema_name,
    o.type,
    o.type_desc,
    o.name,
    o.principal_id,
    s.text,
    CAST(s.ctext AS NVARCHAR(MAX)) AS ctext,
    s.status,
    o.create_date,
    o.modify_date,
    o.is_ms_shipped,
    o.is_published,
    o.is_schema_published,
    s.colid,
    s.compressed,
    s.encrypted,
    s.id,
    s.language,
    s.number,
    s.texttype
FROM sys.objects o
INNER JOIN sys.syscomments s ON o.object_id = s.id
INNER JOIN sys.schemas sc ON o.schema_id = sc.schema_id
WHERE o.type = 'x'
{BuildLikeFilter("o.name", filters?.ProcedureName, "ProcedureName")}";
        }

        internal static string BuildTriggerDdlQuery(
            string computerName,
            string instance,
            ReconQueryFilters filters)
        {
            return $@"
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    name AS [TriggerName],
    object_id AS [TriggerId],
    'SERVER' AS [TriggerType],
    type_desc AS [ObjectType],
    parent_class_desc AS [ObjectClass],
    OBJECT_DEFINITION(OBJECT_ID) AS [TriggerDefinition],
    create_date,
    modify_date,
    is_ms_shipped,
    is_disabled
FROM master.sys.server_triggers
WHERE 1=1
{BuildLikeFilter("name", filters?.TriggerName, "TriggerName")}";
        }

        internal static string BuildTriggerDmlQuery(
            string computerName,
            string instance,
            string databaseName,
            ReconQueryFilters filters)
        {
            return $@"
USE [{EscapeIdentifier(databaseName)}];
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    '{EscapeSqlLiteral(databaseName)}' AS [DatabaseName],
    SCHEMA_NAME(o.schema_id) AS [SchemaName],
    t.name AS [TriggerName],
    t.object_id AS [TriggerId],
    'DATABASE' AS [TriggerType],
    t.type_desc AS [ObjectType],
    t.parent_class_desc AS [ObjectClass],
    OBJECT_DEFINITION(t.object_id) AS [TriggerDefinition],
    t.create_date,
    t.modify_date,
    t.is_ms_shipped,
    t.is_disabled,
    t.is_not_for_replication,
    t.is_instead_of_trigger
FROM sys.triggers t
INNER JOIN sys.objects o ON t.parent_id = o.object_id
WHERE 1=1
{BuildLikeFilter("t.name", filters?.TriggerName, "TriggerName")}";
        }

        internal static string BuildSessionQuery(
            string computerName,
            string instance,
            ReconQueryFilters filters)
        {
            return $@"
USE master;
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    security_id AS [PrincipalSid],
    login_name AS [PrincipalName],
    original_login_name AS [OriginalPrincipalName],
    session_id AS [SessionId],
    last_request_start_time AS [SessionStartTime],
    login_time AS [SessionLoginTime],
    status AS [SessionStatus]
FROM sys.dm_exec_sessions
ORDER BY status
{BuildLikeFilter("login_name", filters?.PrincipalName, "PrincipalName")}";
        }

        internal static string BuildServerLoginQuery(
            string computerName,
            string instance,
            ReconQueryFilters filters)
        {
            return $@"
USE master;
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    principal_id AS [PrincipalId],
    name AS [PrincipalName],
    sid AS [PrincipalSid],
    type_desc AS [PrincipalType],
    create_date AS [CreateDate],
    LOGINPROPERTY(name, 'IsLocked') AS [IsLocked]
FROM sys.server_principals
WHERE type IN ('S','U','C')
{BuildLikeFilter("name", filters?.PrincipalName, "PrincipalName")}";
        }

        internal static string BuildServerPasswordHashQuery(
            string computerName,
            string instance,
            int sqlMajorVersion)
        {
            if (sqlMajorVersion >= 9)
            {
                return $@"
USE master;
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    name AS [PrincipalName],
    principal_id AS [PrincipalId],
    type_desc AS [PrincipalType],
    sid AS [PrincipalSid],
    create_date AS [CreateDate],
    default_database_name AS [DefaultDatabaseName],
    sys.fn_varbintohexstr(password_hash) AS [PasswordHash]
FROM sys.sql_logins";
            }

            return $@"
USE master;
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    name AS [PrincipalName],
    sid AS [PrincipalSid],
    createdate AS [CreateDate],
    dbname AS [DefaultDatabaseName],
    password AS [PasswordHash]
FROM sysxlogins";
        }

        internal static string BuildServerRoleQuery(
            string computerName,
            string instance,
            ReconQueryFilters filters)
        {
            return $@"
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    principal_id AS [RolePrincipalId],
    sid AS [RolePrincipalSid],
    name AS [RolePrincipalName],
    type_desc AS [RolePrincipalType],
    owning_principal_id AS [OwnerPrincipalId],
    SUSER_NAME(owning_principal_id) AS [OwnerPrincipalName],
    is_disabled,
    is_fixed_role,
    create_date,
    modify_date AS [modify_Date],
    default_database_name
FROM master.sys.server_principals
WHERE type LIKE 'R'
{BuildLikeFilter("name", filters?.RolePrincipalName, "RolePrincipalName")}
{BuildLikeFilter("SUSER_NAME(owning_principal_id)", filters?.RoleOwner, "RoleOwner")}";
        }

        internal static string BuildServerRoleMemberQuery(
            string computerName,
            string instance,
            ReconQueryFilters filters)
        {
            return $@"
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    role_principal_id AS [RolePrincipalId],
    SUSER_NAME(role_principal_id) AS [RolePrincipalName],
    member_principal_id AS [PrincipalId],
    SUSER_NAME(member_principal_id) AS [PrincipalName]
FROM sys.server_role_members
WHERE 1=1
{BuildLikeFilter("SUSER_NAME(member_principal_id)", filters?.PrincipalName, "PrincipalName")}
{BuildLikeFilter("SUSER_NAME(role_principal_id)", filters?.RolePrincipalName, "RolePrincipalName")}";
        }

        internal static string BuildServerPrivQuery(
            string computerName,
            string instance,
            ReconQueryFilters filters)
        {
            var where = string.IsNullOrWhiteSpace(filters?.PermissionName)
                ? string.Empty
                : $" WHERE PER.permission_name LIKE '{EscapeSqlLiteral(filters.PermissionName)}'";

            return $@"
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    GRE.name AS [GranteeName],
    GRO.name AS [GrantorName],
    PER.class_desc AS [PermissionClass],
    PER.permission_name AS [PermissionName],
    PER.state_desc AS [PermissionState],
    COALESCE(PRC.name, EP.name, N'') AS [ObjectName],
    COALESCE(PRC.type_desc, EP.type_desc, N'') AS [ObjectType]
FROM sys.server_permissions PER
INNER JOIN sys.server_principals GRE ON PER.grantee_principal_id = GRE.principal_id
INNER JOIN sys.server_principals GRO ON PER.grantor_principal_id = GRO.principal_id
LEFT JOIN sys.server_principals PRC ON PER.class = 101 AND PER.major_id = PRC.principal_id
LEFT JOIN sys.endpoints EP ON PER.class = 105 AND PER.major_id = EP.endpoint_id
{where}
ORDER BY GranteeName, PermissionName";
        }

        internal static string BuildServerCredentialQuery(
            string computerName,
            string instance,
            ReconQueryFilters filters)
        {
            var where = string.IsNullOrWhiteSpace(filters?.CredentialName)
                ? string.Empty
                : $" WHERE name LIKE '{EscapeSqlLiteral(filters.CredentialName)}'";

            return $@"
USE master;
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    credential_id,
    name AS [CredentialName],
    credential_identity,
    create_date,
    modify_date,
    target_type,
    target_id
FROM master.sys.credentials
{where}";
        }

        internal static string BuildServerPolicyQuery(string computerName, string instance)
        {
            return $@"
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    p.policy_id,
    p.name AS [PolicyName],
    p.condition_id,
    c.name AS [ConditionName],
    c.facet,
    c.expression AS [ConditionExpression],
    p.root_condition_id,
    p.is_enabled,
    p.date_created,
    p.date_modified,
    p.description,
    p.created_by,
    p.is_system,
    t.target_set_id,
    t.TYPE,
    t.type_skeleton
FROM msdb.dbo.syspolicy_policies p
INNER JOIN msdb.dbo.syspolicy_conditions c ON p.condition_id = c.condition_id
INNER JOIN msdb.dbo.syspolicy_target_sets t ON t.object_set_id = p.object_set_id";
        }

        internal static string BuildServiceAccountQuery(
            string computerName,
            string instance,
            bool isSysadmin)
        {
            var sysadminSetup = isSysadmin
                ? @"
EXECUTE master.dbo.xp_instance_regread
    @rootkey = N'HKEY_LOCAL_MACHINE',
    @key = N'SYSTEM\CurrentControlSet\Services\SQLBrowser',
    @value_name = N'ObjectName',
    @value = @BrowserLogin OUTPUT;

EXECUTE master.dbo.xp_instance_regread
    @rootkey = N'HKEY_LOCAL_MACHINE',
    @key = N'SYSTEM\CurrentControlSet\Services\SQLWriter',
    @value_name = N'ObjectName',
    @value = @WriterLogin OUTPUT;

EXECUTE master.dbo.xp_instance_regread
    N'HKEY_LOCAL_MACHINE', @MSOLAPInstance,
    N'ObjectName', @AnalysisLogin OUTPUT;

EXECUTE master.dbo.xp_instance_regread
    N'HKEY_LOCAL_MACHINE', @ReportInstance,
    N'ObjectName', @ReportLogin OUTPUT;

EXECUTE master.dbo.xp_instance_regread
    N'HKEY_LOCAL_MACHINE', @IntegrationVersion,
    N'ObjectName', @IntegrationDtsLogin OUTPUT"
                : string.Empty;

            var sysadminColumns = isSysadmin
                ? @",
    [BrowserLogin] = @BrowserLogin,
    [WriterLogin] = @WriterLogin,
    [AnalysisLogin] = @AnalysisLogin,
    [ReportLogin] = @ReportLogin,
    [IntegrationLogin] = @IntegrationDtsLogin"
                : string.Empty;

            return $@"
DECLARE @SQLServerInstance VARCHAR(250);
DECLARE @MSOLAPInstance VARCHAR(250);
DECLARE @ReportInstance VARCHAR(250);
DECLARE @AgentInstance VARCHAR(250);
DECLARE @IntegrationVersion VARCHAR(250);
DECLARE @DBEngineLogin VARCHAR(100);
DECLARE @AgentLogin VARCHAR(100);
DECLARE @BrowserLogin VARCHAR(100);
DECLARE @WriterLogin VARCHAR(100);
DECLARE @AnalysisLogin VARCHAR(100);
DECLARE @ReportLogin VARCHAR(100);
DECLARE @IntegrationDtsLogin VARCHAR(100);

IF @@SERVICENAME = 'MSSQLSERVER' OR @@SERVICENAME = HOST_NAME()
BEGIN
    SET @SQLServerInstance = 'SYSTEM\CurrentControlSet\Services\MSSQLSERVER';
    SET @MSOLAPInstance = 'SYSTEM\CurrentControlSet\Services\MSSQLServerOLAPService';
    SET @ReportInstance = 'SYSTEM\CurrentControlSet\Services\ReportServer';
    SET @AgentInstance = 'SYSTEM\CurrentControlSet\Services\SQLSERVERAGENT';
    SET @IntegrationVersion = 'SYSTEM\CurrentControlSet\Services\MsDtsServer' + SUBSTRING(CAST(SERVERPROPERTY('productversion') AS VARCHAR(255)), 0, 3) + '0';
END
ELSE
BEGIN
    SET @SQLServerInstance = 'SYSTEM\CurrentControlSet\Services\MSSQL$' + CAST(@@SERVICENAME AS VARCHAR(250));
    SET @MSOLAPInstance = 'SYSTEM\CurrentControlSet\Services\MSOLAP$' + CAST(@@SERVICENAME AS VARCHAR(250));
    SET @ReportInstance = 'SYSTEM\CurrentControlSet\Services\ReportServer$' + CAST(@@SERVICENAME AS VARCHAR(250));
    SET @AgentInstance = 'SYSTEM\CurrentControlSet\Services\SQLAgent$' + CAST(@@SERVICENAME AS VARCHAR(250));
    SET @IntegrationVersion = 'SYSTEM\CurrentControlSet\Services\MsDtsServer' + SUBSTRING(CAST(SERVERPROPERTY('productversion') AS VARCHAR(255)), 0, 3) + '0';
END

EXECUTE master.dbo.xp_instance_regread
    N'HKEY_LOCAL_MACHINE', @SQLServerInstance,
    N'ObjectName', @DBEngineLogin OUTPUT;

EXECUTE master.dbo.xp_instance_regread
    N'HKEY_LOCAL_MACHINE', @AgentInstance,
    N'ObjectName', @AgentLogin OUTPUT;

{sysadminSetup}

SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    [DBEngineLogin] = @DBEngineLogin,
    [AgentLogin] = @AgentLogin
    {sysadminColumns}";
        }

        internal static string BuildAgentJobQuery(ReconQueryFilters filters)
        {
            var where = new StringBuilder(" WHERE 1=1");
            if (!string.IsNullOrWhiteSpace(filters?.Keyword))
            {
                where.Append(" AND steps.command LIKE '%").Append(EscapeSqlLiteral(filters.Keyword)).Append("%'");
            }

            if (!string.IsNullOrWhiteSpace(filters?.SubSystem))
            {
                where.Append(" AND steps.subsystem LIKE '").Append(EscapeSqlLiteral(filters.SubSystem)).Append('\'');
            }

            if (!string.IsNullOrWhiteSpace(filters?.ProxyCredential))
            {
                where.Append(" AND proxies.name LIKE '").Append(EscapeSqlLiteral(filters.ProxyCredential)).Append('\'');
            }

            if (filters?.UsingProxyCredential == true)
            {
                where.Append(" AND steps.proxy_id > 0");
            }

            return $@"
SELECT steps.database_name AS [DatabaseName],
    job.job_id AS [Job_Id],
    job.name AS [Job_Name],
    job.description AS [Job_Description],
    SUSER_SNAME(job.owner_sid) AS [Job_Owner],
    steps.proxy_id AS [Proxy_Id],
    proxies.name AS [Proxy_Credential],
    job.enabled AS [Enabled],
    steps.server AS [Server],
    job.date_created AS [Date_Created],
    steps.last_run_date AS [Last_Run_Date],
    steps.step_name AS [Step_Name],
    steps.subsystem AS [SubSystem],
    steps.command AS [Command]
FROM msdb.dbo.sysjobs job
INNER JOIN msdb.dbo.sysjobsteps steps ON job.job_id = steps.job_id
LEFT JOIN msdb.dbo.sysproxies proxies ON steps.proxy_id = proxies.proxy_id
{where}";
        }

        internal static string BuildAssemblyFileQuery(
            string computerName,
            string instance,
            string databaseName,
            ReconQueryFilters filters)
        {
            return $@"
USE [{EscapeIdentifier(databaseName)}];
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    '{EscapeSqlLiteral(databaseName)}' AS [DatabaseName],
    a.assembly_id,
    a.name AS [assembly_name],
    af.file_id,
    af.name AS [file_name],
    af.name AS [clr_name],
    a.permission_set_desc,
    a.create_date,
    a.modify_date,
    a.is_user_defined,
    af.content
FROM sys.assemblies a
JOIN sys.assembly_files af ON a.assembly_id = af.assembly_id
WHERE 1=1
{BuildContainsFilter("a.name", filters?.AssemblyName, "AssemblyName", useWhere: false)}";
        }

        internal static string BuildStoredProcedureClrQuery(
            string computerName,
            string instance,
            string databaseName,
            ReconQueryFilters filters)
        {
            var showAllFilter = filters?.ShowAll == true
                ? string.Empty
                : " AND a.is_user_defined = 1";

            return $@"
USE [{EscapeIdentifier(databaseName)}];
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    '{EscapeSqlLiteral(databaseName)}' AS [DatabaseName],
    SCHEMA_NAME(p.schema_id) AS [schema_name],
    af.file_id,
    af.name AS [file_name],
    af.name AS [clr_name],
    a.assembly_id,
    a.name AS [assembly_name],
    am.assembly_class,
    am.assembly_method,
    p.object_id AS [sp_object_id],
    p.name AS [sp_name],
    p.type AS [sp_type],
    a.permission_set_desc,
    a.create_date,
    a.modify_date,
    af.content
FROM sys.assemblies a
JOIN sys.assembly_files af ON a.assembly_id = af.assembly_id
JOIN sys.assembly_modules am ON a.assembly_id = am.assembly_id
JOIN sys.procedures p ON am.object_id = p.object_id
WHERE 1=1
{showAllFilter}
{BuildContainsFilter("a.name", filters?.AssemblyName, "AssemblyName", useWhere: false)}";
        }

        internal static string BuildOleDbProviderQuery(string computerName, string instance)
        {
            return $@"
CREATE TABLE #Providers (
    [ProviderName] varchar(8000),
    [ParseName] varchar(8000),
    [ProviderDescription] varchar(8000));

INSERT INTO #Providers EXEC xp_enum_oledb_providers;

CREATE TABLE #ProviderInformation (
    [ProviderName] varchar(8000),
    [ProviderDescription] varchar(8000),
    [ProviderParseName] varchar(8000),
    [AllowInProcess] int,
    [DisallowAdHocAccess] int,
    [DynamicParameters] int,
    [IndexAsAccessPath] int,
    [LevelZeroOnly] int,
    [NestedQueries] int,
    [NonTransactedUpdates] int,
    [SqlServerLIKE] int);

DECLARE @Provider_name varchar(8000);
DECLARE @Provider_parse_name varchar(8000);
DECLARE @Provider_description varchar(8000);
DECLARE @regpath nvarchar(512);
DECLARE @AllowInProcess int;
DECLARE @DisallowAdHocAccess int;
DECLARE @DynamicParameters int;
DECLARE @IndexAsAccessPath int;
DECLARE @LevelZeroOnly int;
DECLARE @NestedQueries int;
DECLARE @NonTransactedUpdates int;
DECLARE @SqlServerLIKE int;

DECLARE MY_CURSOR1 CURSOR FOR SELECT * FROM #Providers;
OPEN MY_CURSOR1;
FETCH NEXT FROM MY_CURSOR1 INTO @Provider_name, @Provider_parse_name, @Provider_description;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @regpath = N'SOFTWARE\Microsoft\MSSQLServer\Providers\' + @provider_name;
    SET @AllowInProcess = 0;
    EXEC sys.xp_instance_regread N'HKEY_LOCAL_MACHINE', @regpath, 'AllowInProcess', @AllowInProcess OUTPUT;
    IF @AllowInProcess IS NULL SET @AllowInProcess = 0;
    SET @DisallowAdHocAccess = 0;
    EXEC sys.xp_instance_regread N'HKEY_LOCAL_MACHINE', @regpath, 'DisallowAdHocAccess', @DisallowAdHocAccess OUTPUT;
    IF @DisallowAdHocAccess IS NULL SET @DisallowAdHocAccess = 0;
    SET @DynamicParameters = 0;
    EXEC sys.xp_instance_regread N'HKEY_LOCAL_MACHINE', @regpath, 'DynamicParameters', @DynamicParameters OUTPUT;
    IF @DynamicParameters IS NULL SET @DynamicParameters = 0;
    SET @IndexAsAccessPath = 0;
    EXEC sys.xp_instance_regread N'HKEY_LOCAL_MACHINE', @regpath, 'IndexAsAccessPath', @IndexAsAccessPath OUTPUT;
    IF @IndexAsAccessPath IS NULL SET @IndexAsAccessPath = 0;
    SET @LevelZeroOnly = 0;
    EXEC sys.xp_instance_regread N'HKEY_LOCAL_MACHINE', @regpath, 'LevelZeroOnly', @LevelZeroOnly OUTPUT;
    IF @LevelZeroOnly IS NULL SET @LevelZeroOnly = 0;
    SET @NestedQueries = 0;
    EXEC sys.xp_instance_regread N'HKEY_LOCAL_MACHINE', @regpath, 'NestedQueries', @NestedQueries OUTPUT;
    IF @NestedQueries IS NULL SET @NestedQueries = 0;
    SET @NonTransactedUpdates = 0;
    EXEC sys.xp_instance_regread N'HKEY_LOCAL_MACHINE', @regpath, 'NonTransactedUpdates', @NonTransactedUpdates OUTPUT;
    IF @NonTransactedUpdates IS NULL SET @NonTransactedUpdates = 0;
    SET @SqlServerLIKE = 0;
    EXEC sys.xp_instance_regread N'HKEY_LOCAL_MACHINE', @regpath, 'SqlServerLIKE', @SqlServerLIKE OUTPUT;
    IF @SqlServerLIKE IS NULL SET @SqlServerLIKE = 0;
    INSERT INTO #ProviderInformation
    VALUES (@Provider_name, @Provider_description, @Provider_parse_name, @AllowInProcess, @DisallowAdHocAccess,
        @DynamicParameters, @IndexAsAccessPath, @LevelZeroOnly, @NestedQueries, @NonTransactedUpdates, @SqlServerLIKE);
    FETCH NEXT FROM MY_CURSOR1 INTO @Provider_name, @Provider_parse_name, @Provider_description;
END;
CLOSE MY_CURSOR1;
DEALLOCATE MY_CURSOR1;

SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    ProviderName,
    ProviderDescription,
    ProviderParseName,
    AllowInProcess,
    DisallowAdHocAccess,
    DynamicParameters,
    IndexAsAccessPath,
    LevelZeroOnly,
    NestedQueries,
    NonTransactedUpdates,
    SqlServerLIKE
FROM #ProviderInformation;

DROP TABLE #Providers;
DROP TABLE #ProviderInformation;";
        }

        internal static string BuildAuditDatabaseSpecQuery(
            string computerName,
            string instance,
            ReconQueryFilters filters)
        {
            return $@"
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    audit_id AS [AuditId],
    a.name AS [AuditName],
    s.name AS [AuditSpecification],
    d.audit_action_id AS [AuditActionId],
    d.audit_action_name AS [AuditAction],
    d.major_id,
    OBJECT_NAME(d.major_id) AS [object],
    s.is_state_enabled,
    d.is_group,
    s.create_date,
    s.modify_date,
    d.audited_result
FROM sys.server_audits a
JOIN sys.database_audit_specifications s ON a.audit_guid = s.audit_guid
JOIN sys.database_audit_specification_details d ON s.database_specification_id = d.database_specification_id
WHERE 1=1
{BuildContainsFilter("a.name", filters?.AuditName, "AuditName", useWhere: false)}
{BuildContainsFilter("s.name", filters?.AuditSpecification, "AuditSpecification", useWhere: false)}
{BuildContainsFilter("d.audit_action_name", filters?.AuditAction, "AuditAction", useWhere: false)}";
        }

        internal static string BuildAuditServerSpecQuery(
            string computerName,
            string instance,
            ReconQueryFilters filters)
        {
            return $@"
SELECT '{EscapeSqlLiteral(computerName)}' AS [ComputerName],
    '{EscapeSqlLiteral(instance)}' AS [Instance],
    audit_id AS [AuditId],
    a.name AS [AuditName],
    s.name AS [AuditSpecification],
    d.audit_action_name AS [AuditAction],
    s.is_state_enabled,
    d.is_group,
    d.audit_action_id AS [AuditActionId],
    s.create_date,
    s.modify_date
FROM sys.server_audits a
JOIN sys.server_audit_specifications s ON a.audit_guid = s.audit_guid
JOIN sys.server_audit_specification_details d ON s.server_specification_id = d.server_specification_id
WHERE 1=1
{BuildContainsFilter("a.name", filters?.AuditName, "AuditName", useWhere: false)}
{BuildContainsFilter("s.name", filters?.AuditSpecification, "AuditSpecification", useWhere: false)}
{BuildContainsFilter("d.audit_action_name", filters?.AuditAction, "AuditAction", useWhere: false)}";
        }

        internal static string BuildRecoverPwAutoLogonQuery(bool alternate)
        {
            if (alternate)
            {
                return @"
DECLARE @AltAutoLoginDomain NVARCHAR(512);
DECLARE @AltAutoLoginUser NVARCHAR(512);
DECLARE @AltAutoLoginPassword NVARCHAR(512);
EXEC master.dbo.xp_regread
    'HKEY_LOCAL_MACHINE',
    'SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon',
    'AltDefaultDomainName', @AltAutoLoginDomain OUTPUT;
EXEC master.dbo.xp_regread
    'HKEY_LOCAL_MACHINE',
    'SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon',
    'AltDefaultUserName', @AltAutoLoginUser OUTPUT;
EXEC master.dbo.xp_regread
    'HKEY_LOCAL_MACHINE',
    'SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon',
    'AltDefaultPassword', @AltAutoLoginPassword OUTPUT;
SELECT Domain = @AltAutoLoginDomain, Username = @AltAutoLoginUser, Password = @AltAutoLoginPassword;";
            }

            return @"
DECLARE @AutoLoginDomain NVARCHAR(512);
DECLARE @AutoLoginUser NVARCHAR(512);
DECLARE @AutoLoginPassword NVARCHAR(512);
EXEC master.dbo.xp_regread
    'HKEY_LOCAL_MACHINE',
    'SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon',
    'DefaultDomainName', @AutoLoginDomain OUTPUT;
EXEC master.dbo.xp_regread
    'HKEY_LOCAL_MACHINE',
    'SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon',
    'DefaultUserName', @AutoLoginUser OUTPUT;
EXEC master.dbo.xp_regread
    'HKEY_LOCAL_MACHINE',
    'SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon',
    'DefaultPassword', @AutoLoginPassword OUTPUT;
SELECT Domain = @AutoLoginDomain, Username = @AutoLoginUser, Password = @AutoLoginPassword;";
        }

        private static string BuildDatabaseNameFilter(ReconQueryFilters filters)
        {
            return string.IsNullOrWhiteSpace(filters?.DatabaseName)
                ? string.Empty
                : $" AND a.name LIKE '{EscapeSqlLiteral(filters.DatabaseName)}'";
        }

        private static string BuildNoDefaultsFilter(ReconQueryFilters filters)
        {
            return filters?.NoDefaults == true
                ? " AND a.name NOT IN ('master','tempdb','msdb','model')"
                : string.Empty;
        }

        private static string BuildHasAccessFilter(ReconQueryFilters filters)
        {
            return filters?.HasAccess == true
                ? " AND HAS_DBACCESS(a.name)=1"
                : string.Empty;
        }

        private static string BuildSysAdminOnlyFilter(ReconQueryFilters filters)
        {
            return filters?.SysAdminOnly == true
                ? " AND IS_SRVROLEMEMBER('sysadmin',SUSER_SNAME(a.owner_sid))=1"
                : string.Empty;
        }

        private static string BuildLikeFilter(string column, string value, string _)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : $" AND {column} LIKE '{EscapeSqlLiteral(value)}'";
        }

        private static string BuildContainsFilter(string column, string value, string _, bool useWhere)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var prefix = useWhere ? " WHERE " : " AND ";
            return prefix + column + " LIKE '%" + EscapeSqlLiteral(value) + "%'";
        }

        private static string EscapeSqlLiteral(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value.Replace("'", "''");
        }

        private static string EscapeIdentifier(string value)
        {
            return string.IsNullOrEmpty(value) ? value : value.Replace("]", "]]");
        }
    }
}
