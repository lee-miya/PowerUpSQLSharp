using System.Collections.Generic;
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

        IReadOnlyList<IDictionary<string, object>> GetDatabasePriv(
            SqlConnectionOptions options,
            ReconQueryFilters filters,
            CancellationToken cancellationToken = default);

        IReadOnlyList<IDictionary<string, object>> GetServerPriv(
            SqlConnectionOptions options,
            ReconQueryFilters filters,
            CancellationToken cancellationToken = default);
    }
}
