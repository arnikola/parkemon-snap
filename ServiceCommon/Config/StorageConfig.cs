using ServiceCommon.Utilities.Config;

namespace ServiceCommon.Config
{
    /// <summary>
    /// The service configuration.
    /// </summary>
    public class StorageConfig : IConfiguration
    {
        /// <summary>
        /// The development storage connection string.
        /// </summary>
        private const string DevelopmentStorage = "UseDevelopmentStorage=true";
        private const string MelbStorage = "SECRETSTRING";

        public StorageConfig(HostingEnvironment environment)
        {
            DefaultConfiguration.SetEnvironmentValues(this, environment);
        }

        /// <summary>
        /// Gets or sets the index storage connection string.
        /// </summary>
        [Default(DevelopmentStorage, Environment = Environments.Dev)]
        [Default(MelbStorage, Environment = Environments.Melbourne)]
        public string IndexConnectionString { get; set; }
        
        /// <summary>
        /// Gets or sets the actor state storage connection string.
        /// </summary>
        [Default(DevelopmentStorage, Environment = Environments.Dev)]
        [Default(MelbStorage, Environment = Environments.Melbourne)]
        public string GrainStateConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the name of the grain state Azure Blob Container.
        /// </summary>
        [Default("grains")]
        public string GrainStateContainerName { get; set; }

        /// <summary>
        /// Gets or sets the actor system store connection string.
        /// </summary>
        [Default(DevelopmentStorage, Environment = Environments.Dev)]
        [Default(MelbStorage, Environment = Environments.Melbourne)]
        public string OrleansClusterMembershipConnectionString { get; set; }

        [Default(DevelopmentStorage, Environment = Environments.Dev)]
        [Default(MelbStorage, Environment = Environments.Melbourne)]
        public string ImageBlobStoreConnectionString { get; set; }
        
        [Default("locations")]
        public string LocationLookupTableName { get; set; }

        [Default(DevelopmentStorage, Environment = Environments.Dev)]
        [Default(MelbStorage, Environment = Environments.Melbourne)]
        public string LocationLookupTableConnectionString { get; set; }

        [Default("userPhones")]
        public string PhoneTableName { get; set; }

        [Default(DevelopmentStorage, Environment = Environments.Dev)]
        [Default(MelbStorage, Environment = Environments.Melbourne)]
        public string PhoneTableConnectionString { get; set; }
    }
}
