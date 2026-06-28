namespace PowerUpSQLSharp.Core.Models
{
    public sealed class ReconQueryFilters
    {
        public string DatabaseName { get; set; }
        public string SchemaName { get; set; }
        public bool NoDefaults { get; set; }
        public bool HasAccess { get; set; }
        public bool SysAdminOnly { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string ColumnNameSearch { get; set; }
        public string ViewName { get; set; }
        public string ProcedureName { get; set; }
        public string Keyword { get; set; }
        public bool AutoExec { get; set; }
        public bool OnlySigned { get; set; }
        public string TriggerName { get; set; }
        public string PrincipalName { get; set; }
        public string DatabaseUser { get; set; }
        public string RolePrincipalName { get; set; }
        public string RoleOwner { get; set; }
        public string PermissionName { get; set; }
        public string PermissionType { get; set; }
        public string CredentialName { get; set; }
        public string AuditName { get; set; }
        public string AuditSpecification { get; set; }
        public string AuditAction { get; set; }
        public string AssemblyName { get; set; }
        public string ExportFolder { get; set; }
        public bool ShowAll { get; set; }
        public bool ShowRoleSchemas { get; set; }
        public string SubSystem { get; set; }
        public bool UsingProxyCredential { get; set; }
        public string ProxyCredential { get; set; }
        public bool Migrate { get; set; }
        public string QueryText { get; set; }
        public bool NoOutput { get; set; }
        public int SampleSize { get; set; } = 1;
        public string Keywords { get; set; } = "Password";
        public bool ValidateCc { get; set; }
        public string OutFolder { get; set; }
        public bool Xml { get; set; }
        public bool Csv { get; set; } = true;
        public bool CrawlLinks { get; set; }
    }
}
