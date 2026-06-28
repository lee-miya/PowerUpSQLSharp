using System.Collections.Generic;
using System.Linq;
using PowerUpSQLSharp.Core.Models;

namespace PowerUpSQLSharp.Core.Commands
{
    internal static class ReconCommandHelper
    {
        internal static ReconQueryFilters ResolveFilters(CommandContext context)
        {
            var filters = new ReconQueryFilters();

            if (CommandArgumentHelper.TryGet(context?.Arguments, "DatabaseName", out var databaseName))
            {
                filters.DatabaseName = databaseName;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "SchemaName", out var schemaName))
            {
                filters.SchemaName = schemaName;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "DatabaseUser", out var databaseUser))
            {
                filters.DatabaseUser = databaseUser;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "TableName", out var tableName))
            {
                filters.TableName = tableName;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "ColumnName", out var columnName))
            {
                filters.ColumnName = columnName;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "ColumnNameSearch", out var columnNameSearch))
            {
                filters.ColumnNameSearch = columnNameSearch;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "ViewName", out var viewName))
            {
                filters.ViewName = viewName;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "ProcedureName", out var procedureName))
            {
                filters.ProcedureName = procedureName;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "Keyword", out var keyword))
            {
                filters.Keyword = keyword;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "TriggerName", out var triggerName))
            {
                filters.TriggerName = triggerName;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "PrincipalName", out var principalName))
            {
                filters.PrincipalName = principalName;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "RolePrincipalName", out var rolePrincipalName))
            {
                filters.RolePrincipalName = rolePrincipalName;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "RoleOwner", out var roleOwner))
            {
                filters.RoleOwner = roleOwner;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "PermissionName", out var permissionName))
            {
                filters.PermissionName = permissionName;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "PermissionType", out var permissionType))
            {
                filters.PermissionType = permissionType;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "CredentialName", out var credentialName))
            {
                filters.CredentialName = credentialName;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "AuditName", out var auditName))
            {
                filters.AuditName = auditName;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "AuditSpecification", out var auditSpecification))
            {
                filters.AuditSpecification = auditSpecification;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "AuditAction", out var auditAction))
            {
                filters.AuditAction = auditAction;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "AssemblyName", out var assemblyName))
            {
                filters.AssemblyName = assemblyName;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "ExportFolder", out var exportFolder))
            {
                filters.ExportFolder = exportFolder;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "Keywords", out var keywords))
            {
                filters.Keywords = keywords;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "Query", out var query))
            {
                filters.QueryText = query;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "OutFolder", out var outFolder))
            {
                filters.OutFolder = outFolder;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "SubSystem", out var subSystem))
            {
                filters.SubSystem = subSystem;
            }

            if (CommandArgumentHelper.TryGet(context?.Arguments, "ProxyCredential", out var proxyCredential))
            {
                filters.ProxyCredential = proxyCredential;
            }

            if (CommandArgumentHelper.TryGetInt(context?.Arguments, "SampleSize", out var sampleSize))
            {
                filters.SampleSize = sampleSize;
            }

            filters.NoDefaults = CommandArgumentHelper.TryGetBool(context?.Arguments, "NoDefaults", out var noDefaults)
                && noDefaults;
            filters.HasAccess = CommandArgumentHelper.TryGetBool(context?.Arguments, "HasAccess", out var hasAccess)
                && hasAccess;
            filters.SysAdminOnly = CommandArgumentHelper.TryGetBool(context?.Arguments, "SysAdminOnly", out var sysAdminOnly)
                && sysAdminOnly;
            filters.AutoExec = CommandArgumentHelper.TryGetBool(context?.Arguments, "AutoExec", out var autoExec)
                && autoExec;
            filters.OnlySigned = CommandArgumentHelper.TryGetBool(context?.Arguments, "OnlySigned", out var onlySigned)
                && onlySigned;
            filters.ShowAll = CommandArgumentHelper.TryGetBool(context?.Arguments, "ShowAll", out var showAll)
                && showAll;
            filters.ShowRoleSchemas = CommandArgumentHelper.TryGetBool(context?.Arguments, "ShowRoleSchemas", out var showRoleSchemas)
                && showRoleSchemas;
            filters.UsingProxyCredential = CommandArgumentHelper.TryGetBool(context?.Arguments, "UsingProxyCredential", out var usingProxyCredential)
                && usingProxyCredential;
            filters.Migrate = CommandArgumentHelper.TryGetBool(context?.Arguments, "Migrate", out var migrate)
                && migrate;
            filters.NoOutput = CommandArgumentHelper.TryGetBool(context?.Arguments, "NoOutput", out var noOutput)
                && noOutput;
            filters.ValidateCc = CommandArgumentHelper.TryGetBool(context?.Arguments, "ValidateCC", out var validateCc)
                && validateCc;
            filters.Xml = CommandArgumentHelper.TryGetBool(context?.Arguments, "xml", out var xml)
                && xml;
            filters.Csv = !filters.Xml;
            if (CommandArgumentHelper.TryGetBool(context?.Arguments, "csv", out var csv))
            {
                filters.Csv = csv;
            }

            filters.CrawlLinks = CommandArgumentHelper.TryGetBool(context?.Arguments, "CrawlLinks", out var crawlLinks)
                && crawlLinks;

            return filters;
        }

        internal static int ResolveThreads(CommandContext context)
        {
            if (CommandArgumentHelper.TryGetInt(context?.Arguments, "Threads", out var threads))
            {
                return threads;
            }

            return context?.Global?.Threads ?? 10;
        }

        internal static CommandResult BuildReconResult(
            IReadOnlyList<IDictionary<string, object>> rows,
            int requestedCount,
            GlobalOptions options)
        {
            var successfulInstances = rows
                .Select(row => row.TryGetValue("Instance", out var instance) ? instance?.ToString() : null)
                .Where(instance => !string.IsNullOrWhiteSpace(instance))
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .Count();

            if (successfulInstances == 0 && rows.Count > 0)
            {
                successfulInstances = requestedCount > 0 ? requestedCount : 1;
            }

            var formatted = CommandResultFormatter.FromRows(rows, options);
            formatted.ExitCode = ServerInfoCommandHelper.ResolveExitCode(requestedCount, successfulInstances);
            return formatted;
        }
    }
}
