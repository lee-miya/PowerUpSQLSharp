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

    }

}

