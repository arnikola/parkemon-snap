using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceCommon.Storage
{
    /// <summary>
    ///     The IndexTable interface.
    /// </summary>
    public interface IIndexTable
    {
        /// <summary>
        /// Returns a page of items from the index.
        /// </summary>
        /// <param name="after">
        /// The id prior to the first item returned.
        /// </param>
        /// <param name="maxResults">
        /// The maximum number of results.
        /// </param>
        /// <returns>
        /// A page of items from the index.
        /// </returns>
        Task<IEnumerable<Guid>> GetPage(Guid? after = default(Guid?), int maxResults = 25);

        /// <summary>
        /// Returns a value indicating whether or not the specified item exists.
        /// </summary>
        /// <param name="itemId">
        /// The item id.
        /// </param>
        /// <returns>
        /// Whether or not the item exists.
        /// </returns>
        Task<bool> Exists(Guid itemId);

        /// <summary>
        /// The delete if exists.
        /// </summary>
        /// <param name="itemId">
        /// The item id.
        /// </param>
        /// <returns>
        /// Whether or not the item was in the index.
        /// </returns>
        Task<bool> DeleteIfExists(Guid itemId);

        /// <summary>
        /// Includes the specified <paramref name="itemId"/> in the index.
        /// </summary>
        /// <param name="itemId">
        /// The item id.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the work performed.
        /// </returns>
        Task Insert(Guid itemId);

        /// <summary>
        /// Returns the number of entities in the table.
        /// </summary>
        /// <returns>The number of entities in the table.</returns>
        Task<int> Count();
    }
}