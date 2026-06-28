using System;
using System.Collections.Generic;
using System.Threading;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;

namespace PowerUpSQLSharp.Tests.TestDoubles
{
    internal sealed class FakeReconnaissanceService : IReconnaissanceService
    {
        private readonly IReadOnlyList<SqlServerInfo> _serverInfoResults;
        private readonly IReadOnlyList<SqlServerConfigurationEntry> _configurationEntries;
        private readonly IReadOnlyList<IDictionary<string, object>> _queryResults;
        private int _callIndex;

        public FakeReconnaissanceService(
            IReadOnlyList<SqlServerInfo> serverInfoResults = null,
            IReadOnlyList<SqlServerConfigurationEntry> configurationEntries = null,
            IReadOnlyList<IDictionary<string, object>> queryResults = null)
        {
            _serverInfoResults = serverInfoResults ?? new SqlServerInfo[0];
            _configurationEntries = configurationEntries ?? new SqlServerConfigurationEntry[0];
            _queryResults = queryResults ?? new List<IDictionary<string, object>>();
        }

        public SqlServerInfo GetServerInfo(
            SqlConnectionOptions options,
            CancellationToken cancellationToken = default)
        {
            if (_serverInfoResults.Count == 0)
            {
                return null;
            }

            return _serverInfoResults[Math.Min(_callIndex++, _serverInfoResults.Count - 1)];
        }

        public IReadOnlyList<SqlServerInfo> GetServerInfoThreaded(
            IEnumerable<string> instances,
            SqlConnectionOptions templateOptions,
            int maxThreads,
            CancellationToken cancellationToken = default)
        {
            return _serverInfoResults;
        }

        public IReadOnlyList<SqlServerConfigurationEntry> GetServerConfiguration(
            SqlConnectionOptions options,
            CancellationToken cancellationToken = default)
        {
            return _configurationEntries;
        }

        public IReadOnlyList<SqlServerConfigurationEntry> GetServerConfigurationThreaded(
            IEnumerable<string> instances,
            SqlConnectionOptions templateOptions,
            int maxThreads,
            CancellationToken cancellationToken = default)
        {
            return _configurationEntries;
        }

        public IReadOnlyList<IDictionary<string, object>> Query(
            SqlReconOperation operation,
            SqlConnectionOptions options,
            ReconQueryFilters filters,
            CancellationToken cancellationToken = default)
        {
            return _queryResults;
        }

        public IReadOnlyList<IDictionary<string, object>> QueryThreaded(
            SqlReconOperation operation,
            IEnumerable<string> instances,
            SqlConnectionOptions templateOptions,
            ReconQueryFilters filters,
            int maxThreads,
            CancellationToken cancellationToken = default)
        {
            return _queryResults;
        }
    }
}
