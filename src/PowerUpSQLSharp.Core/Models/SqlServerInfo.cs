namespace PowerUpSQLSharp.Core.Models

{

    public sealed class SqlServerInfo

    {

        public string ComputerName { get; set; }

        public string Instance { get; set; }

        public string DomainName { get; set; }

        public string ServiceProcessId { get; set; }

        public string ServiceName { get; set; }

        public string ServiceAccount { get; set; }

        public string AuthenticationMode { get; set; }

        public string ForcedEncryption { get; set; }

        public string Clustered { get; set; }

        public string SqlServerVersionNumber { get; set; }

        public string SqlServerMajorVersion { get; set; }

        public string SqlServerEdition { get; set; }

        public string SqlServerServicePack { get; set; }

        public string OsArchitecture { get; set; }

        public string OsMachineType { get; set; }

        public string OsVersionName { get; set; }

        public string OsVersionNumber { get; set; }

        public string CurrentLogin { get; set; }

        public string IsSysadmin { get; set; }

        public string XpCmdShellEnabled { get; set; }

        public string XpDirtreeEnabled { get; set; }

        public string XpFileexistEnabled { get; set; }

        public string XpSubdirsEnabled { get; set; }

        public string ActiveSessions { get; set; }

    }

}

