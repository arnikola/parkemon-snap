// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InMemoryLookupTableFactory.cs" company="Dapr Labs">
//   Copyright © Dapr Labs. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace ServiceCommon.Storage.Memory
{
    using System;
    using System.Collections.Concurrent;

    using ServiceCommon.Storage;

    /// <summary>
    /// The in-memory lookup table factory.
    /// </summary>
    internal class InMemoryLookupTableFactory : ILookupTableFactory
    {
        /// <summary>
        /// The lookup tables.
        /// </summary>
        private readonly ConcurrentDictionary<Tuple<string, Type>, object> lookupTables = new ConcurrentDictionary<Tuple<string, Type>, object>();

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
        public ILookupTable<TValue> Create<TValue>(string tableName)
        {
            return
                (ILookupTable<TValue>)
                this.lookupTables.GetOrAdd(
                    Tuple.Create(tableName, typeof(TValue)), 
                    _ => new InMemoryLookupTable<TValue>());
        }
    }
}