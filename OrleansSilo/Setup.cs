using System;
using System.Diagnostics;
using System.IO;
using Orleans.Runtime.Configuration;
using ServiceCommon.Utilities;
using ServiceCommon.Utilities.Extensions;

namespace OrleansSilo
{
    using ServiceCommon.Utilities.Network;

    internal static class Setup
    {
        public static void OpenFirewallPorts(NodeConfiguration config)
        {
            if (config.IsGatewayNode)
            {
                var port = config.ProxyGatewayEndpoint.Port;
                Firewall.OpenPort("Orleans_Gateway", port, Firewall.Protocol.Tcp);
            }

            Firewall.OpenPort("Orleans_Silo", config.Endpoint.Port, Firewall.Protocol.Tcp);
        }

        public static void InstallPerformanceCounters()
        {
            try
            {
                Trace.TraceInformation("Resetting performance counter settings.");
                new ProcessStartInfo { UseShellExecute = false, FileName = "lodctr.exe", Arguments = "/r" }.Execute(
                    throwOnError: false);
                Trace.TraceInformation("Reset performance counter settings.");
                Trace.TraceInformation("Installing Orleans performance counters.");
                var assemblyPath = AssemblyUtil.GetDirectory(typeof(Setup).Assembly);
                new ProcessStartInfo
                {
                    UseShellExecute = false,
                    FileName = Path.Combine(assemblyPath, "OrleansCounterControl.exe"),
                    WorkingDirectory = assemblyPath
                }.Execute(throwOnError: false);
                Trace.TraceInformation("Installed Orleans performance counters.");
            }
            catch (Exception exception)
            {
                Trace.TraceWarning($"Exception installing performance counters: {exception.ToDetailedString()}");
            }
        }
    }
}
