// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InMemoryIndexTable.cs" company="Dapr Labs">
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
    /// The in-memory index table.
    /// </summary>
    public class InMemoryIndexTable : IIndexTable
    {
        /// <summary>
        /// The comparer.
        /// </summary>
        private static readonly GuidComparer Comparer = new GuidComparer();

        /// <summary>
        /// The values.
        /// </summary>
        private readonly HashSet<Guid> values = new HashSet<Guid>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryIndexTable"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        public InMemoryIndexTable(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

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
        public Task<IEnumerable<Guid>> GetPage(Guid? after = null, int maxResults = 25)
        {
            return
                Task.FromResult(
                    this.values.OrderBy(_ => _, Comparer)
                        .SkipWhile(_ => after.HasValue && Comparer.Compare(_, after.Value) <= 0)
                        .Take(maxResults));
        }

        /// <summary>
        /// Returns a value indicating whether or not the specified item exists.
        /// </summary>
        /// <param name="itemId">
        /// The item id.
        /// </param>
        /// <returns>
        /// Whether or not the item exists.
        /// </returns>
        public Task<bool> Exists(Guid itemId)
        {
            return Task.FromResult(this.values.Contains(itemId));
        }

        /// <summary>
        /// The delete if exists.
        /// </summary>
        /// <param name="itemId">
        /// The item id.
        /// </param>
        /// <returns>
        /// Whether or not the item was in the index..
        /// </returns>
        public Task<bool> DeleteIfExists(Guid itemId)
        {
            return Task.FromResult(this.values.Remove(itemId));
        }

        /// <summary>
        /// Includes the specified <paramref name="itemId"/> in the index.
        /// </summary>
        /// <param name="itemId">
        /// The item id.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the value was added, <see langword="false"/> if the value already existed.
        /// </returns>
        public Task Insert(Guid itemId)
        {
            this.values.Add(itemId);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Returns the number of entities in the table.
        /// </summary>
        /// <returns>The number of entities in the table.</returns>
        public Task<int> Count()
        {
            return Task.FromResult(this.values.Count);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Reset()
        {
            this.values.Clear();
        }

        /// <summary>
        /// The <see cref="Guid"/> comparer.
        /// </summary>
        private class GuidComparer : IEqualityComparer<Guid>, IComparer<Guid>
        {
            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// </summary>
            /// <param name="x">
            /// The first object to compare.
            /// </param>
            /// <param name="y">
            /// The second object to compare.
            /// </param>
            /// <returns>
            /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>.
            /// </returns>
            public int Compare(Guid x, Guid y)
            {
                return StringComparer.OrdinalIgnoreCase.Compare(x.ToString("N"), y.ToString("N"));
            }

            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <returns>
            /// true if the specified objects are equal; otherwise, false.
            /// </returns>
            /// <param name="x">
            /// The first object to compare.
            /// </param>
            /// <param name="y">
            /// The second object to compare.
            /// </param>
            public bool Equals(Guid x, Guid y)
            {
                return x == y;
            }

            /// <summary>
            /// Returns a hash code for the specified object.
            /// </summary>
            /// <returns>
            /// A hash code for the specified object.
            /// </returns>
            /// <param name="obj">
            /// The <see cref="T:System.Object"/> for which a hash code is to be returned.
            /// </param>
            public int GetHashCode(Guid obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}