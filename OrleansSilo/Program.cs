using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using Autofac;
using Orleans.Runtime.Host;
using ServiceCommon.Utilities.Config;

namespace OrleansSilo
{
    using System.Threading.Tasks;

    using Orleans.Runtime;
    using Orleans.Runtime.Configuration;

    using PowerArgs;

    using ServiceCommon.Utilities.Extensions;
    using ServiceCommon.Utilities.Network;
    
    public static class Program
    {
        [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
        public class Arguments
        {
            [ArgExample(Environments.Dev, "Set the environment to dev."), ArgDefaultValue(Environments.Dev)]
            public string Environment { get; set; }

            [ArgDefaultValue(30000), ArgExample("30000", "Use port 30000")]
            public ushort GatewayPort { get; set; }

            [ArgDefaultValue(11111), ArgExample("11111", "Use port 11111")]
            public ushort SiloPort { get; set; }
        }

        public static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    Run(Args.Parse<Arguments>(args)).Wait();
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"FATAL Exception:\n{exception.ToDetailedString()}");
                }

                if (!Environment.UserInteractive)
                {
                    return;
                }

                Console.WriteLine("Press Esc to exit, or any other key to restart.");
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                {
                    break;
                }
            }
        }
        private static async Task Run(Arguments args)
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = Environment.ProcessorCount * 32;
            ServicePointManager.Expect100Continue = false;

            // Get the current environment.
            var envName = args.Environment ?? ConfigurationManager.AppSettings["Environment"];
            var hostingEnvironment = new HostingEnvironment(envName);

            var container = Startup.GetContainer(hostingEnvironment);
            var config = container.Resolve<SiloConfig>().GetClusterConfiguration();

            var nodeAddress = await NetworkUtil.GetNodeAddress();
            config.Defaults.HostNameOrIPAddress = nodeAddress.ToString();
            config.Defaults.Port = args.SiloPort;

            var process = Process.GetCurrentProcess();
            var name = Environment.MachineName + "_" + process.Id + Guid.NewGuid().ToString("N").Substring(3);
            var silo = new SiloHost(name, config);

            try
            {
                // Configure the silo for the current environment.
                var generation = SiloAddress.AllocateNewGeneration();
                silo.SetSiloType(Silo.SiloType.Secondary);
                silo.SetSiloLivenessType(GlobalConfiguration.LivenessProviderType.AzureTable);
                silo.SetReminderServiceType(GlobalConfiguration.ReminderServiceProviderType.AzureTable);
                silo.SetDeploymentId(config.Globals.DeploymentId, config.Globals.DataConnectionString);

                silo.SetSiloEndpoint(new IPEndPoint(nodeAddress, args.SiloPort), generation);
                silo.SetProxyEndpoint(new IPEndPoint(nodeAddress, args.GatewayPort));

                Setup.InstallPerformanceCounters();
                Setup.OpenFirewallPorts(silo.NodeConfig);

                Trace.TraceInformation("Silo configuration: \n" + silo.Config.ToString(name));

                silo.InitializeOrleansSilo();
                Trace.TraceInformation(
                    "Successfully initialized Orleans silo '{0}' as a {1} node.",
                    silo.Name,
                    silo.Type);
                Trace.TraceInformation("Starting Orleans silo '{0}' as a {1} node.", silo.Name, silo.Type);

                if (silo.StartOrleansSilo())
                {
                    silo.WaitForOrleansSiloShutdown();
                }
            }
            finally
            {
                silo.UnInitializeOrleansSilo();
                silo.Dispose();
            }
        }
    }
}
