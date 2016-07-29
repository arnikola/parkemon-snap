using System;
using System.Collections.Generic;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using ServiceCommon.Config;
using ServiceCommon.Utilities.Config;

namespace OrleansSilo
{
    using System.IO;

    using OrleansSilo.Storage;

    public class SiloConfig
    {
        private readonly StorageConfig storageConfig;

        private readonly HostingEnvironment environment;

        public SiloConfig(StorageConfig storageConfig, HostingEnvironment environment)
        {
            this.storageConfig = storageConfig;
            this.environment = environment;
        }

        public ClusterConfiguration GetClusterConfiguration()
        {
            var config = new ClusterConfiguration();

            // Configure logging and metrics collection.
            config.Defaults.StartupTypeName = typeof(SiloServiceLocator).AssemblyQualifiedName;
            config.Defaults.TraceFilePattern = Path.GetTempPath() + "{0}_{1}.log";
            config.Defaults.StatisticsCollectionLevel = StatisticsLevel.Info;
            config.Defaults.StatisticsLogWriteInterval = TimeSpan.FromDays(6);
            config.Defaults.TurnWarningLengthThreshold = TimeSpan.FromSeconds(15);
            config.Defaults.TraceToConsole = true;
            config.Defaults.WriteMessagingTraces = false;
            config.Defaults.DefaultTraceLevel = Severity.Info;
            /*config.Defaults.TraceLevelOverrides.Add(Tuple.Create("Orleans", Severity.Warning));
            config.Defaults.TraceLevelOverrides.Add(Tuple.Create("Runtime", Severity.Warning));
            config.Defaults.TraceLevelOverrides.Add(Tuple.Create("Dispatcher", Severity.Warning));
            config.Defaults.TraceLevelOverrides.Add(Tuple.Create("MembershipOracle", Severity.Warning));
            config.Defaults.TraceLevelOverrides.Add(Tuple.Create("DeploymentLoadPublisher", Severity.Warning));
            config.Defaults.TraceLevelOverrides.Add(Tuple.Create("ReminderService", Severity.Warning));
            config.Defaults.TraceLevelOverrides.Add(Tuple.Create("SiloLogStatistics", Severity.Warning));*/
            config.Defaults.TraceLevelOverrides.Add(Tuple.Create("Catalog", Severity.Warning));
            config.Defaults.TraceLevelOverrides.Add(Tuple.Create("sync.SafeTimerBase", Severity.Warning));
            config.Defaults.TraceLevelOverrides.Add(Tuple.Create("asynTask.SafeTimerBase", Severity.Warning));

            // Configure providers
            config.Globals.RegisterBootstrapProvider<SiloBootstrap>(SiloBootstrap.ProviderName);
            config.Globals.RegisterStorageProvider<AzureBlobStorageProvider>(
                "Default",
                new Dictionary<string, string>
                {
                    { "DataConnectionString", this.storageConfig.GrainStateConnectionString },
                    { "UseJsonFormat", true.ToString() }
                });
            config.Globals.ReminderServiceType = GlobalConfiguration.ReminderServiceProviderType.AzureTable;
            config.Globals.LivenessType = GlobalConfiguration.LivenessProviderType.AzureTable;
            config.Globals.DataConnectionString = this.storageConfig.OrleansClusterMembershipConnectionString;

            // Configure clustering.
            config.Globals.DeploymentId = this.environment.Name;
            config.Globals.ResponseTimeout = TimeSpan.FromSeconds(600);
            
            return config;
        }
    }
}
