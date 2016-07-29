// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InMemoryIndexTableFactory.cs" company="Dapr Labs">
//   Copyright © Dapr Labs. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace ServiceCommon.Storage.Memory
{
    using System.Collections.Concurrent;

    using ServiceCommon.Storage;

    /// <summary>
    /// The in-memory index table factory.
    /// </summary>
    public class InMemoryIndexTableFactory : IIndexTableFactory
    {
        /// <summary>
        /// The indexes.
        /// </summary>
        private static readonly ConcurrentDictionary<string, InMemoryIndexTable> Indexes = new ConcurrentDictionary<string, InMemoryIndexTable>();

        /// <summary>
        /// Clears all data.
        /// </summary>
        public static void Reset()
        {
            foreach (var index in Indexes.Values)
            {
                index.Reset();
            }
        }

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
        public IIndexTable Create(string kind, string index, string partitionId)
        {
            return Indexes.GetOrAdd(kind + "_" + index + "_" + partitionId, _ => new InMemoryIndexTable(_));
        }
    }
}