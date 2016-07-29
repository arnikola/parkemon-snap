using System;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using ServiceCommon.Utilities.Config;

namespace ServiceCommon.Config
{
    using System.IO;

    public class OrleansConfiguration
    {
        private readonly StorageConfig storageConfig;

        private readonly HostingEnvironment environment;

        public OrleansConfiguration(HostingEnvironment environment, StorageConfig storageConfig)
        {
            this.storageConfig = storageConfig;
            this.environment = environment;
        }

        public ClientConfiguration GetConfiguration()
        {
            return new ClientConfiguration
            {
                DataConnectionString = this.storageConfig.OrleansClusterMembershipConnectionString,
                DeploymentId = this.environment.Name,
                PropagateActivityId = true,
                DefaultTraceLevel = Severity.Info,
                GatewayProvider = ClientConfiguration.GatewayProviderType.AzureTable,
                TraceToConsole = true,
                StatisticsCollectionLevel = StatisticsLevel.Critical,
                StatisticsLogWriteInterval = TimeSpan.FromDays(6),
                TraceFilePattern = Path.GetTempPath() + "{0}_{1}.log",
                WriteMessagingTraces = false,
                /*TraceLevelOverrides =
                {
                    Tuple.Create("Catalog", Severity.Warning),
                    Tuple.Create("Dispatcher", Severity.Warning),
                    Tuple.Create("Runtime", Severity.Warning),
                    Tuple.Create("Orleans", Severity.Warning)
                },*/
                ResponseTimeout = TimeSpan.FromSeconds(600),
                StatisticsMetricsTableWriteInterval = TimeSpan.FromDays(6),
                StatisticsPerfCountersWriteInterval = TimeSpan.FromDays(6),
            };
        }
    }
}