using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using ServiceCommon.Utilities.Serialization;

namespace ServiceCommon.Storage.Azure
{
    /// <summary>
    /// Methods for interacting with a simple lookup table.
    /// </summary>
    /// <typeparam name="TValue">
    /// The value type.
    /// </typeparam>
    public class LookupTable<TValue> : ILookupTable<TValue>
    {
        /// <summary>
        ///     The connection string.
        /// </summary>
        private readonly string connectionString;
        

        /// <summary>
        ///     The table name.
        /// </summary>
        private readonly string tableName;

        /// <summary>
        ///     The storage table.
        /// </summary>
        private CloudTable cloudTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="LookupTable{TValue}"/> class.
        /// </summary>
        /// <param name="connectionString">
        /// The connection string.
        /// </param>
        /// <param name="tableName">
        /// The table name.
        /// </param>
        public LookupTable(string connectionString, string tableName)
        {
            this.tableName = tableName;
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Returns the value for the provided key.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The value for the provided key, or <see langword="false"/> if not found.
        /// </returns>
        public async Task<Tuple<bool, TValue>> Get(string key)
        {
            var table = await this.GetTable();

            var query =
                new TableQuery<LookupTableEntity>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, key), 
                        TableOperators.And, 
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, key)));
            IEnumerable<LookupTableEntity> result =
                (await table.ExecuteQuerySegmentedAsync(query, default(TableContinuationToken))).Results;
            
            var entity = result.FirstOrDefault();
            return entity == null ? Tuple.Create(false, default(TValue)) : Tuple.Create(true, entity.GetEntity());
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
        public async Task<IEnumerable<Tuple<string, TValue>>> GetPage(string afterKey, int maxResults)
        {
            var table = await this.GetTable();

            var query =
                new TableQuery<LookupTableEntity>().Where(
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, afterKey));
            var result = (await table.ExecuteQuerySegmentedAsync(query, default(TableContinuationToken))).Results;
            if (result == null)
            {
                return Enumerable.Empty<Tuple<string, TValue>>();
            }

            return result.Select(val => Tuple.Create(val.RowKey, val.GetEntity()));
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
        public async Task<bool> Exists(string key)
        {
            var table = await this.GetTable();

            var query =
                new TableQuery<IndexTable.IndexTableEntity>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, key), 
                        TableOperators.And, 
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, key)));
            var result = await table.ExecuteQuerySegmentedAsync(query, default(TableContinuationToken));
            return result.Results.Count > 0;
        }

        /// <summary>
        /// The delete if exists.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the work performed.
        /// </returns>
        public async Task<bool> DeleteIfExists(string key)
        {
            var table = await this.GetTable();
            var entity = new LookupTableEntity(key) { ETag = "*" };
            try
            {
                var result = await table.ExecuteAsync(TableOperation.Delete(entity));
                return result.HttpStatusCode == (int)HttpStatusCode.NoContent;
            }
            catch (StorageException exception)
            {
                if (exception.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
                {
                    return false;
                }

                throw;
            }
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
        public async Task AddOrReplace(string key, TValue value)
        {
            var table = await this.GetTable();
            var entity = new LookupTableEntity(key) { ETag = "*" };
            entity.SetValue(value);
            await table.ExecuteAsync(TableOperation.InsertOrReplace(entity));
        }

        /// <summary>
        ///     Returns the table.
        /// </summary>
        /// <returns>
        ///     The table.
        /// </returns>
        public Task<CloudTable> GetTable()
        {
            var result = this.cloudTable;
            if (result == null)
            {
                return
                    TableCache.GetTable(this.tableName, this.connectionString)
                        .ContinueWith(
                            task => { return this.cloudTable = task.Result; }, 
                            TaskContinuationOptions.ExecuteSynchronously);
            }

            return Task.FromResult(result);
        }

        /// <summary>
        ///     The lookup table entity.
        /// </summary>
        [DataContract]
        public class LookupTableEntity : TableEntity
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LookupTableEntity"/> class.
            /// </summary>
            public LookupTableEntity()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="LookupTableEntity"/> class.
            /// </summary>
            /// <param name="key">
            /// The key.
            /// </param>
            public LookupTableEntity(string key)
            {
                this.RowKey = this.PartitionKey = key;
            }

            /// <summary>
            /// Gets or sets the raw value.
            /// </summary>
            [DataMember]
            public string Raw { get; set; }

            /// <summary>
            /// Gets the entity.
            /// </summary>
            /// <returns>
            /// The entity.
            /// </returns>
            public TValue GetEntity()
            {
                if (string.IsNullOrEmpty(this.Raw))
                {
                    return default(TValue);
                }

                return JsonConvert.DeserializeObject<TValue>(this.Raw, SerializationSettings.JsonConfig);
            }

            /// <summary>
            /// Sets the entity.
            /// </summary>
            /// <param name="value">
            /// The new entity.
            /// </param>
            public void SetValue(TValue value)
            {
                this.Raw = JsonConvert.SerializeObject(value, SerializationSettings.JsonConfig);
            }
        }
    }
}