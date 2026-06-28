using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Utils;

namespace PowerUpSQLSharp.Core.Services
{
    public sealed class InstanceDiscoveryService : IInstanceDiscoveryService
    {
        private readonly IActiveDirectoryLdapService _ldapService;

        public InstanceDiscoveryService(IActiveDirectoryLdapService ldapService = null)
        {
            _ldapService = ldapService ?? new ActiveDirectoryLdapService();
        }

        public IReadOnlyList<SqlInstance> GetLocalInstances()
        {
            return SqlInstanceRegistryReader.ReadLocalInstances();
        }

        public IReadOnlyList<SqlInstance> GetBroadcastInstances()
        {
            return SqlDataSourceEnumeratorReader.ReadBroadcastInstances();
        }

        public IReadOnlyList<SqlInstance> GetInstancesFromFile(string filePath)
        {
            return SqlInstanceFileReader.ReadInstances(filePath);
        }

        public IReadOnlyList<SqlInstance> ScanUdp(
            string computerName,
            int timeoutSeconds,
            CancellationToken cancellationToken = default)
        {
            return SqlBrowserUdpScanner.Scan(computerName, timeoutSeconds, cancellationToken);
        }

        public IReadOnlyList<SqlInstance> ScanUdpThreaded(
            IEnumerable<string> computerNames,
            int timeoutSeconds,
            int maxThreads,
            CancellationToken cancellationToken = default)
        {
            var names = (computerNames ?? Enumerable.Empty<string>())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return ThreadedExecutor.Execute(
                names,
                maxThreads,
                (name, token) => ScanUdp(name, timeoutSeconds, token),
                cancellationToken);
        }

        public DomainDiscoveryResult GetDomainInstances(
            DomainInstanceQuery query,
            CancellationToken cancellationToken = default)
        {
            query = query ?? new DomainInstanceQuery();
            var ldapOptions = query.Ldap ?? new DomainLdapOptions();

            var spnEntries = _ldapService.GetDomainSpns(ldapOptions, "MSSQL*", out var warning);
            var sqlSpns = spnEntries
                .Where(entry => entry.Service != null && entry.Service.StartsWith("MSSQL", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var instances = sqlSpns
                .Select(entry => MapDomainInstance(entry, query.IncludeIp))
                .ToList();

            if (query.CheckMgmt)
            {
                var mgmtEntries = _ldapService.GetDomainSpns(ldapOptions, "MSServerClusterMgmtAPI", out var mgmtWarning);
                if (string.IsNullOrEmpty(warning))
                {
                    warning = mgmtWarning;
                }

                var mgmtHosts = mgmtEntries
                    .Where(entry => !string.IsNullOrWhiteSpace(entry.ComputerName)
                        && entry.ComputerName.IndexOf('.') >= 0)
                    .Select(entry => entry.ComputerName)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var udpInstances = ScanUdpThreaded(
                    mgmtHosts,
                    query.UdpTimeoutSeconds,
                    query.Threads,
                    cancellationToken);

                var merged = instances
                    .Select(item => CreateSummary(item.ComputerName, item.ServerInstance))
                    .Concat(udpInstances.Select(item => CreateSummary(item.ComputerName, item.ServerInstance)))
                    .GroupBy(item => $"{item.ComputerName}|{item.ServerInstance}", StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.First())
                    .OrderBy(item => item.ComputerName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(item => item.ServerInstance, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return new DomainDiscoveryResult
                {
                    Instances = merged,
                    Warning = warning,
                    SummaryOnly = true
                };
            }

            return new DomainDiscoveryResult
            {
                Instances = instances,
                Warning = warning,
                SummaryOnly = false
            };
        }

        private static SqlDomainInstance MapDomainInstance(DomainSpnEntry entry, bool includeIp)
        {
            var instance = new SqlDomainInstance
            {
                ComputerName = entry.ComputerName,
                ServerInstance = DomainSpnParser.ParseServerInstance(entry.Spn),
                DomainAccountSid = entry.UserSid,
                DomainAccount = entry.User,
                DomainAccountCn = entry.UserCn,
                Service = entry.Service,
                Spn = entry.Spn,
                LastLogon = entry.LastLogon,
                Description = entry.Description
            };

            if (includeIp)
            {
                instance.IpAddress = ResolveIpAddress(entry.ComputerName);
            }

            return instance;
        }

        private static SqlDomainInstance CreateSummary(string computerName, string serverInstance)
        {
            return new SqlDomainInstance
            {
                ComputerName = computerName ?? string.Empty,
                ServerInstance = serverInstance ?? string.Empty
            };
        }

        private static string ResolveIpAddress(string computerName)
        {
            if (string.IsNullOrWhiteSpace(computerName))
            {
                return "0.0.0.0";
            }

            try
            {
                var addresses = Dns.GetHostAddresses(computerName);
                if (addresses.Length == 0)
                {
                    return "0.0.0.0";
                }

                return string.Join(", ", addresses.Select(address => address.ToString()));
            }
            catch
            {
                return "0.0.0.0";
            }
        }
    }
}
