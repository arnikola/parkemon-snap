using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceCommon.Storage
{
    /// <summary>
    /// The lookup interface.
    /// </summary>
    /// <typeparam name="TValue">
    /// The value type.
    /// </typeparam>
    public interface ILookupTable<TValue>
    {
        /// <summary>
        /// Returns the value for the provided key.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The value for the provided key, or <see langword="false"/> if not found.
        /// </returns>
        Task<Tuple<bool, TValue>> Get(string key);

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
        Task<IEnumerable<Tuple<string, TValue>>> GetPage(string afterKey, int maxResults);

        /// <summary>
        /// Returns <see langword="true"/> if the key exists, <see langword="false"/> otherwise.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the key exists, <see langword="false"/> otherwise.
        /// </returns>
        Task<bool> Exists(string key);

        /// <summary>
        /// Deletes the specified entry if it exists.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the work performed.
        /// </returns>
        Task<bool> DeleteIfExists(string key);

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
        Task AddOrReplace(string key, TValue value);
    }
}