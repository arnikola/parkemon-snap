using ServiceCommon.Config;

namespace ServiceCommon.Storage.Azure
{
    /// <summary>
    ///     The lookup table factory.
    /// </summary>
    public class LookupTableFactory : ILookupTableFactory
    {
        /// <summary>
        ///     The connection string.
        /// </summary>
        private readonly string connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="LookupTableFactory"/> class.
        /// </summary>
        /// <param name="storageConfig">
        /// The service configuration.
        /// </param>
        public LookupTableFactory(StorageConfig storageConfig)
        {
            this.connectionString = storageConfig.IndexConnectionString;
        }

        /// <summary>
        /// Creates a new lookup table instance.
        /// </summary>
        /// <typeparam name="TValue">
        /// The value type.
        /// </typeparam>
        /// <param name="tableName">
        /// The table name.
        /// </param>
        /// <returns>
        /// The <see cref="ILookupTable{Value}"/>.
        /// </returns>
        public ILookupTable<TValue> Create<TValue>(string tableName)
        {
            return new LookupTable<TValue>(this.connectionString, tableName);
        }
    }
}