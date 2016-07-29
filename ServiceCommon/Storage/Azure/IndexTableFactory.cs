using ServiceCommon.Config;

namespace ServiceCommon.Storage.Azure
{
    /// <summary>
    ///     The index table factory.
    /// </summary>
    public class IndexTableFactory : IIndexTableFactory
    {
        /// <summary>
        ///     The connection string.
        /// </summary>
        private readonly string connectionString;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="IndexTableFactory"/> class.
        /// </summary>
        /// <param name="storageConfig">
        /// The service configuration.
        /// </param>
        public IndexTableFactory(StorageConfig storageConfig)
        {
            this.connectionString = storageConfig.IndexConnectionString;
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="kind">
        /// The kind.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <param name="partitionId">
        /// The partition id.
        /// </param>
        /// <returns>
        /// The <see cref="IIndexTable"/>.
        /// </returns>
        public IIndexTable Create(string kind, string index, string partitionId)
        {
            return new IndexTable(kind, index, partitionId, this.connectionString);
        }
    }
}