namespace PowerUpSQLSharp.Core.Utils

{

    internal static class SqlServerInfoQueryBuilder

    {

        internal const string SysadminCheckQuery =

            "SELECT CASE WHEN IS_SRVROLEMEMBER('sysadmin') = 0 THEN 'No' ELSE 'Yes' END AS IsSysadmin";



        internal const string ActiveSessionsQuery =

            "USE master; SELECT status FROM sys.dm_exec_sessions";

        internal const string XpCmdShellConfigureQuery = "EXEC sp_configure 'xp_cmdshell'";

        internal const string XpExecutePermissionsQuery = @"
SELECT
  CASE
    WHEN NOT EXISTS (SELECT 1 FROM master.sys.objects WHERE name = N'xp_dirtree' AND type = N'X') THEN N'No'
    WHEN ISNULL(HAS_PERMS_BY_NAME(N'xp_dirtree', N'OBJECT', N'EXECUTE'), 0) = 1 THEN N'Yes'
    ELSE N'No'
  END AS XpDirtreeEnabled,
  CASE
    WHEN NOT EXISTS (SELECT 1 FROM master.sys.objects WHERE name = N'xp_fileexist' AND type = N'X') THEN N'No'
    WHEN ISNULL(HAS_PERMS_BY_NAME(N'xp_fileexist', N'OBJECT', N'EXECUTE'), 0) = 1 THEN N'Yes'
    ELSE N'No'
  END AS XpFileexistEnabled,
  CASE
    WHEN NOT EXISTS (SELECT 1 FROM master.sys.objects WHERE name = N'xp_subdirs' AND type = N'X') THEN N'No'
    WHEN ISNULL(HAS_PERMS_BY_NAME(N'xp_subdirs', N'OBJECT', N'EXECUTE'), 0) = 1 THEN N'Yes'
    ELSE N'No'
  END AS XpSubdirsEnabled";



        internal static string BuildServerInfoQuery(string computerName, string isSysadmin, string activeSessions)

        {

            var escapedComputerName = EscapeSqlLiteral(computerName);

            var escapedIsSysadmin = EscapeSqlLiteral(isSysadmin);

            var escapedActiveSessions = EscapeSqlLiteral(activeSessions);



            var sysadminSetup = string.Equals(isSysadmin, "Yes", System.StringComparison.OrdinalIgnoreCase)

                ? @"

 -- Get machine type

 DECLARE @MachineType SYSNAME

 EXECUTE master.dbo.xp_regread

 @rootkey = N'HKEY_LOCAL_MACHINE',

 @key = N'SYSTEM\CurrentControlSet\Control\ProductOptions',

 @value_name = N'ProductType',

 @value = @MachineType output



 -- Get OS version

 DECLARE @ProductName SYSNAME

 EXECUTE master.dbo.xp_regread

 @rootkey = N'HKEY_LOCAL_MACHINE',

 @key = N'SOFTWARE\Microsoft\Windows NT\CurrentVersion',

 @value_name = N'ProductName',

 @value = @ProductName output"

                : string.Empty;



            var sysadminQuery = string.Equals(isSysadmin, "Yes", System.StringComparison.OrdinalIgnoreCase)

                ? " @MachineType as [OsMachineType],\r\n @ProductName as [OSVersionName],"

                : string.Empty;



            return $@" -- Get SQL Server Information



 -- Get SQL Server Service Name and Path

 DECLARE @SQLServerInstance varchar(250)

 DECLARE @SQLServerServiceName varchar(250)

 if @@SERVICENAME = 'MSSQLSERVER'

 BEGIN

 set @SQLServerInstance = 'SYSTEM\CurrentControlSet\Services\MSSQLSERVER'

 set @SQLServerServiceName = 'MSSQLSERVER'

 END

 ELSE

 BEGIN

 set @SQLServerInstance = 'SYSTEM\CurrentControlSet\Services\MSSQL$'+cast(@@SERVICENAME as varchar(250))

 set @SQLServerServiceName = 'MSSQL$'+cast(@@SERVICENAME as varchar(250))

 END



 -- Get SQL Server Service Account

 DECLARE @ServiceAccountName varchar(250)

 EXECUTE master.dbo.xp_instance_regread

 N'HKEY_LOCAL_MACHINE', @SQLServerInstance,

 N'ObjectName',@ServiceAccountName OUTPUT, N'no_output'



 -- Get authentication mode

 DECLARE @AuthenticationMode INT

 EXEC master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE',

 N'Software\Microsoft\MSSQLServer\MSSQLServer',

 N'LoginMode', @AuthenticationMode OUTPUT



 -- Get the forced encryption flag

 BEGIN TRY

 DECLARE @ForcedEncryption INT

 EXEC master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE',

 N'SOFTWARE\MICROSOFT\Microsoft SQL Server\MSSQLServer\SuperSocketNetLib',

 N'ForceEncryption', @ForcedEncryption OUTPUT

 END TRY

 BEGIN CATCH

 END CATCH



 -- Grab additional information as sysadmin

 {sysadminSetup}



 -- Return server and version information

 SELECT '{escapedComputerName}' as [ComputerName],

 @@servername as [Instance],

 DEFAULT_DOMAIN() as [DomainName],

 SERVERPROPERTY('processid') as ServiceProcessID,

 @SQLServerServiceName as [ServiceName],

 @ServiceAccountName as [ServiceAccount],

 (SELECT CASE @AuthenticationMode

 WHEN 1 THEN 'Windows Authentication'

 WHEN 2 THEN 'Windows and SQL Server Authentication'

 ELSE 'Unknown'

 END) as [AuthenticationMode],

 @ForcedEncryption as ForcedEncryption,

 CASE SERVERPROPERTY('IsClustered')

 WHEN 0

 THEN 'No'

 ELSE 'Yes'

 END as [Clustered],

 SERVERPROPERTY('productversion') as [SQLServerVersionNumber],

 SUBSTRING(@@VERSION, CHARINDEX('2', @@VERSION), 4) as [SQLServerMajorVersion],

 serverproperty('Edition') as [SQLServerEdition],

 SERVERPROPERTY('ProductLevel') AS [SQLServerServicePack],

 SUBSTRING(@@VERSION, CHARINDEX('x', @@VERSION), 3) as [OSArchitecture],

 {sysadminQuery}

 RIGHT(SUBSTRING(@@VERSION, CHARINDEX('Windows NT', @@VERSION), 14), 3) as [OsVersionNumber],

 SYSTEM_USER as [Currentlogin],

 '{escapedIsSysadmin}' as [IsSysadmin],

 '{escapedActiveSessions}' as [ActiveSessions]";

        }



        private static string EscapeSqlLiteral(string value)

        {

            return (value ?? string.Empty).Replace("'", "''");

        }

    }

}

