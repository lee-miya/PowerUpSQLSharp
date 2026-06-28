using System.Threading;
using PowerUpSQLSharp.Core.Models;

namespace PowerUpSQLSharp.Core.Services
{
    public interface IPrivilegeInspectionService
    {
        SqlSysadminStatus GetSysadminStatus(
            SqlConnectionOptions options,
            CancellationToken cancellationToken = default);

        SqlLocalAdminStatus CheckLocalAdministrator();
    }
}
