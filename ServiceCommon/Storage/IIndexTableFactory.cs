namespace ServiceCommon.Storage
{
    /// <summary>
    /// Factory for index tables.
    /// </summary>
    public interface IIndexTableFactory
    {
        /// <summary>
        /// Creates a new index table instance.
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
        IIndexTable Create(string kind, string index, string partitionId);
    }
}