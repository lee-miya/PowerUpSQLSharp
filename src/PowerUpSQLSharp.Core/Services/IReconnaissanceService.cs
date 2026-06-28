using System.Collections.Generic;

using System.Threading;

using PowerUpSQLSharp.Core.Models;



namespace PowerUpSQLSharp.Core.Services

{

    public interface IReconnaissanceService

    {

        SqlServerInfo GetServerInfo(

            SqlConnectionOptions options,

            CancellationToken cancellationToken = default);



        IReadOnlyList<SqlServerInfo> GetServerInfoThreaded(

            IEnumerable<string> instances,

            SqlConnectionOptions templateOptions,

            int maxThreads,

            CancellationToken cancellationToken = default);

        IReadOnlyList<SqlServerConfigurationEntry> GetServerConfiguration(
            SqlConnectionOptions options,
            CancellationToken cancellationToken = default);

        IReadOnlyList<SqlServerConfigurationEntry> GetServerConfigurationThreaded(
            IEnumerable<string> instances,
            SqlConnectionOptions templateOptions,
            int maxThreads,
            CancellationToken cancellationToken = default);

        IReadOnlyList<IDictionary<string, object>> Query(
            SqlReconOperation operation,
            SqlConnectionOptions options,
            ReconQueryFilters filters,
            CancellationToken cancellationToken = default);

        IReadOnlyList<IDictionary<string, object>> QueryThreaded(
            SqlReconOperation operation,
            IEnumerable<string> instances,
            SqlConnectionOptions templateOptions,
            ReconQueryFilters filters,
            int maxThreads,
            CancellationToken cancellationToken = default);

    }

}

