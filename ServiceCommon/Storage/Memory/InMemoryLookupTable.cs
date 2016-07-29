// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InMemoryLookupTable.cs" company="Dapr Labs">
//   Copyright © Dapr Labs. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace ServiceCommon.Storage.Memory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ServiceCommon.Storage;

    /// <summary>
    /// The in-memory lookup table.
    /// </summary>
    /// <typeparam name="TValue">
    /// The value type.
    /// </typeparam>
    internal class InMemoryLookupTable<TValue> : ILookupTable<TValue>
    {
        /// <summary>
        /// The values.
        /// </summary>
        private readonly Dictionary<string, TValue> values = new Dictionary<string, TValue>();

        /// <summary>
        /// Returns the value for the provided key.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The value for the provided key, or <see langword="false"/> if not found.
        /// </returns>
        public Task<Tuple<bool, TValue>> Get(string key)
        {
            TValue result;
            var exists = this.values.TryGetValue(key, out result);
            return Task.FromResult(Tuple.Create(exists, result));
        }

        /// <summary>
        /// Returns a collection of values matching the provided criteria.
        /// </summary>
        /// <param name="afterKey">
        /// The key to begin the query at. All results will have a key lexically greater than, or equal to, this value.
        /// </param>
        /// <param name="maxResults">
        /// The maximum number of results to return.
        /// </param>
        /// <returns>
        /// A collection matching the provided criteria.
        /// </returns>
        public Task<IEnumerable<Tuple<string, TValue>>> GetPage(string afterKey, int maxResults)
        {
            var results =
                this.values.SkipWhile(_ => StringComparer.OrdinalIgnoreCase.Compare((string)_.Key, afterKey) <= 0)
                    .Take(maxResults)
                    .Select(_ => Tuple.Create(_.Key, _.Value));

            return Task.FromResult(results);
        }

        /// <summary>
        /// Returns <see langword="true"/> if the key exists, <see langword="false"/> otherwise.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the key exists, <see langword="false"/> otherwise.
        /// </returns>
        public Task<bool> Exists(string key)
        {
            return Task.FromResult(this.values.ContainsKey(key));
        }

        /// <summary>
        /// Deletes the specified entry if it exists.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the work performed.
        /// </returns>
        public Task<bool> DeleteIfExists(string key)
        {
            return Task.FromResult(this.values.Remove(key));
        }

        /// <summary>
        /// Adds or updates a key in the table.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the work performed.
        /// </returns>
        public Task AddOrReplace(string key, TValue value)
        {
            this.values[key] = value;
            return Task.FromResult(0);
        }
    }
}