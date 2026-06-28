using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using PowerUpSQLSharp.Core.Models;

namespace PowerUpSQLSharp.Core.Utils
{
    internal static class SqlBrowserUdpScanner
    {
        private const int SqlBrowserPort = 0x59a;

        public static IReadOnlyList<SqlInstance> Scan(
            string computerName,
            int timeoutSeconds,
            CancellationToken cancellationToken = default)
        {
            var instances = new List<SqlInstance>();
            if (string.IsNullOrWhiteSpace(computerName))
            {
                return instances;
            }

            cancellationToken.ThrowIfCancellationRequested();

            string serverIp;
            try
            {
                var addresses = Dns.GetHostAddresses(computerName);
                serverIp = addresses.Length > 0 ? addresses[0].ToString() : string.Empty;
            }
            catch
            {
                serverIp = string.Empty;
            }

            UdpClient client = null;
            try
            {
                client = new UdpClient();
                client.Client.ReceiveTimeout = Math.Max(1, timeoutSeconds) * 1000;
                client.Connect(computerName, SqlBrowserPort);

                var packet = new byte[] { 0x03 };
                client.Send(packet, packet.Length);

                var remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
                var bytesReceived = client.Receive(ref remoteEndpoint);
                var response = Encoding.ASCII.GetString(bytesReceived).Split(';');

                ParseResponse(computerName, serverIp, response, instances);
            }
            catch (SocketException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            finally
            {
                client?.Close();
            }

            return instances;
        }

        private static void ParseResponse(
            string computerName,
            string serverIp,
            string[] response,
            ICollection<SqlInstance> instances)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i <= response.Length; i++)
            {
                var segment = i < response.Length ? response[i] : string.Empty;
                if (!string.IsNullOrEmpty(segment))
                {
                    var key = NormalizeKey(segment);
                    var value = i + 1 < response.Length ? response[i + 1] : string.Empty;
                    values[key] = value;
                    continue;
                }

                if (!values.TryGetValue("tcp", out var tcp) || string.IsNullOrEmpty(tcp))
                {
                    values.Clear();
                    continue;
                }

                values.TryGetValue("instancename", out var instanceName);
                values.TryGetValue("version", out var version);
                values.TryGetValue("isclustered", out var isClusteredRaw);

                var displayInstance = string.IsNullOrEmpty(instanceName)
                    ? computerName
                    : $"{computerName}\\{instanceName}";

                instances.Add(new SqlInstance
                {
                    ComputerName = computerName,
                    InstanceName = instanceName ?? string.Empty,
                    ServerInstance = displayInstance,
                    ServerIp = serverIp,
                    Port = int.TryParse(tcp, out var port) ? port : (int?)null,
                    Version = version ?? string.Empty,
                    IsClustered = IsClusteredValue(isClusteredRaw)
                });

                values.Clear();
            }

            if (values.TryGetValue("tcp", out var trailingTcp) && !string.IsNullOrEmpty(trailingTcp))
            {
                values.TryGetValue("instancename", out var instanceName);
                values.TryGetValue("version", out var version);
                values.TryGetValue("isclustered", out var isClusteredRaw);

                var displayInstance = string.IsNullOrEmpty(instanceName)
                    ? computerName
                    : $"{computerName}\\{instanceName}";

                instances.Add(new SqlInstance
                {
                    ComputerName = computerName,
                    InstanceName = instanceName ?? string.Empty,
                    ServerInstance = displayInstance,
                    ServerIp = serverIp,
                    Port = int.TryParse(trailingTcp, out var port) ? port : (int?)null,
                    Version = version ?? string.Empty,
                    IsClustered = IsClusteredValue(isClusteredRaw)
                });
            }
        }

        private static string NormalizeKey(string key)
        {
            return Regex.Replace(key.ToLowerInvariant(), @"[\W]", string.Empty);
        }

        private static bool IsClusteredValue(string value)
        {
            return string.Equals(value, "Yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase);
        }
    }
}
