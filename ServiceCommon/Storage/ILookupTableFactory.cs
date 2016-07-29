namespace ServiceCommon.Storage
{
    /// <summary>
    /// Factory for lookup tables.
    /// </summary>
    public interface ILookupTableFactory
    {
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
        /// The <see cref="ILookupTable{TValue}"/>.
        /// </returns>
        ILookupTable<TValue> Create<TValue>(string tableName);
    }
}