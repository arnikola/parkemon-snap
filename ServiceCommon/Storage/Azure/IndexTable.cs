using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace ServiceCommon.Storage.Azure
{
    /// <summary>
    ///     Methods for storing and retrieving a simple index of identifiers.
    /// </summary>
    public class IndexTable : IIndexTable
    {
        /// <summary>
        /// The request headers.
        /// </summary>
        private static readonly Dictionary<string, string> RequestHeaders = new Dictionary<string, string> { { "Prefer", "return-no-content" } };

        /// <summary>
        /// The partition key column.
        /// </summary>
        private static readonly string[] PartitionKeyColumn = { "PartitionKey" };

        /// <summary>
        /// The request options.
        /// </summary>
        private static readonly TableRequestOptions RequestOptions = new TableRequestOptions
        {
            PayloadFormat = TablePayloadFormat.JsonNoMetadata
        };

        /// <summary>
        ///     The connection string.
        /// </summary>
        private readonly string connectionString;

        /// <summary>
        ///     The partition id.
        /// </summary>
        private readonly string partitionId;

        /// <summary>
        ///     The table name.
        /// </summary>
        private MetadataTableEntity metadata;

        /// <summary>
        ///     The storage table.
        /// </summary>
        private CloudTable cloudTable;
        

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexTable"/> class.
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
        /// <param name="connectionString">
        /// The connection string.
        /// </param>
        public IndexTable(string kind, string index, string partitionId, string connectionString)
        {
            this.TableName = $"IX{kind.ToLowerInvariant()}X{index?.ToLowerInvariant() ?? string.Empty}";
            this.connectionString = connectionString;
            this.partitionId = partitionId;
        }

        /// <summary>
        ///     Gets the table name.
        /// </summary>
        public string TableName { get; }

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
        public async Task<IEnumerable<Guid>> GetPage(Guid? after = default(Guid?), int maxResults = 25)
        {
            var table = await this.GetTable();

            var rowkey = (after ?? Guid.Empty).ToString("N");
            var partitionKey = this.partitionId;
            var query =
                new TableQuery<IndexTableEntity>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey), 
                        TableOperators.And, 
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, rowkey)))
                    .Take(maxResults);
            IEnumerable<IndexTableEntity> result =
                (await table.ExecuteQuerySegmentedAsync(query, default(TableContinuationToken))).Results;

            return result.Where(
                _ =>
                {
                    Guid itemId;
                    return Guid.TryParse(_.RowKey, out itemId);
                }).Select(_ => _.ItemId);
        }

        /// <summary>
        /// Returns <see langword="true"/> if the item exists in the table, <see langword="false"/> otherwise.
        /// </summary>
        /// <param name="itemId">
        /// The item id.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the item exists in the table, <see langword="false"/> otherwise.
        /// </returns>
        public async Task<bool> Exists(Guid itemId)
        {
            var table = await this.GetTable();

            var rowkey = itemId.ToString("N");
            var partitionKey = this.partitionId;
            var query =
                new TableQuery<IndexTableEntity>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey), 
                        TableOperators.And, 
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowkey)))
                    .Select(PartitionKeyColumn);
            var result =
                await
                table.ExecuteQuerySegmentedAsync(
                    query, 
                    default(TableContinuationToken), 
                    RequestOptions, 
                    new OperationContext { UserHeaders = RequestHeaders });
            return result.Results.Count > 0;
        }
        

        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="itemId">
        /// The item id.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the item existed in the table, <see langword="false"/> otherwise.
        /// </returns>
        public async Task<bool> DeleteIfExists(Guid itemId)
        {
            var table = await this.GetTable();
            var entity = new IndexTableEntity(itemId, this.partitionId) { ETag = "*" };
            var context = new OperationContext { UserHeaders = RequestHeaders };
            //this.logger.Information("Deleting item from table {Name}, {ItemId}.", this.TableName, itemId);

            try
            {
                var operation = await this.WithMetadataUpdate(TableOperation.Delete(entity), -1);
                await table.ExecuteBatchAsync(operation, RequestOptions, context);
                //this.logger.Information("Successfully deleted item from table {Name}, {ItemId}.", this.TableName, itemId);

                return true;
            }
            catch (StorageException exception)
            {
                //this.logger.Information("Storage exception on deleting data from table {Name}, {ItemId}.", this.TableName, itemId);
                this.metadata = null;

                if (exception.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
                {
                    //this.logger.Information("Item not found {Name}, {ItemId}.", this.TableName, itemId);
                    return false;
                }

                throw;
            }
            catch (Exception)
            {
                //this.logger.Information("Generic exception on deleting data from table {Name}, {ItemId}.", this.TableName, itemId);

                this.metadata = null;
                throw;
            }
        }

        /// <summary>
        /// Inserts an item in the index.
        /// </summary>
        /// <param name="itemId">
        /// The item id.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the value was inserted, <see langword="false"/> if the value already existed.
        /// </returns>
        public async Task Insert(Guid itemId)
        {
            var table = await this.GetTable();
            var entity = new IndexTableEntity(itemId, this.partitionId);
            var context = new OperationContext { UserHeaders = RequestHeaders };

            //this.logger.Information("Inserting item into IndexTable {Name}, {ItemId}.", this.TableName, itemId);

            try
            {
                var results =
                    await
                    table.ExecuteBatchAsync(
                        await this.WithMetadataUpdate(TableOperation.Insert(entity), 1),
                        RequestOptions,
                        context);
                //this.logger.Information("Inserted: " + itemId);

                this.metadata.ETag = results[1].Etag;
                //this.logger.Information("Inserted item in IndexTable. {Name}, {ItemId}, {ETag}", this.TableName, itemId, this.metadata.ETag);
            }
            catch (StorageException exception)
            {
                //this.logger.Information("StorageException in IndexTable.Insert {Name}, {ItemId}.", this.TableName, itemId);
                this.metadata = null;
                var info = exception.RequestInformation;
                var error = info.ExtendedErrorInformation;
                if (info.HttpStatusCode == (int)HttpStatusCode.Conflict
                    && string.Equals(error.ErrorCode, "EntityAlreadyExists", StringComparison.OrdinalIgnoreCase))
                {
                    //this.logger.Information("Entity already exists. {Name}, {ItemId}.", this.TableName, itemId);
                    return;
                }
                throw;
            }
            catch (Exception)
            {
                //this.logger.Information("Exception in IndexTable.Insert {Name}, {ItemId}.", this.TableName, itemId);
                this.metadata = null;
                throw;
            }
        }

        /// <summary>
        /// Returns the number of entities in the table.
        /// </summary>
        /// <returns>The number of entities in the table.</returns>
        public async Task<int> Count()
        {
            if (this.metadata == null)
            {
                var table = await this.GetTable();
                this.metadata = await this.ReadMetadata(table);
            }

            return this.metadata?.Count ?? 0;
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
                    TableCache.GetTable(this.TableName, this.connectionString)
                        .ContinueWith(
                            task =>
                            {
                                return this.cloudTable = task.Result;
                            }, 
                            TaskContinuationOptions.ExecuteSynchronously);
            }

            return Task.FromResult(result);
        }

        /// <summary>
        /// Creates and returns an entity that can be used to update the metadata table row.
        ///     The intended use is to prevent interleaved writes to the table for this partitionid.
        /// </summary>
        /// <param name="operation">
        /// The operation being executed.
        /// </param>
        /// <param name="countDelta">
        /// The change in item count after the operation is successfully executed.
        /// </param>
        /// <returns>
        /// The table.
        /// </returns>
        private async Task<TableBatchOperation> WithMetadataUpdate(TableOperation operation, int countDelta)
        {
            var table = await this.GetTable();

            // Get the existing metadata or instantiate new metadata.
            TableOperation metadataUpdate;
            if (this.metadata == null)
            {
                this.metadata = await this.ReadMetadata(table);
                if (this.metadata == null)
                {
                    this.metadata = new MetadataTableEntity(this.partitionId);
                    metadataUpdate = TableOperation.Insert(this.metadata);
                }
                else
                {
                    metadataUpdate = TableOperation.Replace(this.metadata);
                }
            }
            else
            {
                metadataUpdate = TableOperation.Replace(this.metadata);
            }

            // Increment the verion
            this.metadata.Version++;
            this.metadata.Count += countDelta;

            return new TableBatchOperation { operation, metadataUpdate };
        }

        /// <summary>
        /// The read metadata.
        /// </summary>
        /// <param name="table">
        /// The table.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the work performed.
        /// </returns>
        private async Task<MetadataTableEntity> ReadMetadata(CloudTable table)
        {
            // Read the metadata row.
            var partitionKey = this.partitionId;
            var query =
                new TableQuery<MetadataTableEntity>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey), 
                        TableOperators.And, 
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, MetadataTableEntity.RowId)));
            var result = await table.ExecuteQuerySegmentedAsync(query, default(TableContinuationToken));

            // Get the first result.
            return result.Results.Count > 0 ? result.Results[0] : null;
        }

        /// <summary>
        ///     The MetaData entity.
        /// </summary>
        [DataContract]
        public class MetadataTableEntity : TableEntity
        {
            /// <summary>
            /// The row key.
            /// </summary>
            public const string RowId = "Version";

            /// <summary>
            ///     Initializes a new instance of the <see cref="MetadataTableEntity"/> class.
            /// </summary>
            public MetadataTableEntity()
            {
                this.RowKey = RowId;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="MetadataTableEntity"/> class.
            /// </summary>
            /// <param name="partitionId">
            /// The partition id.
            /// </param>
            public MetadataTableEntity(string partitionId)
            {
                this.RowKey = RowId;
                this.PartitionKey = partitionId;
            }

            /// <summary>
            /// Gets or sets the version;
            /// </summary>
            [DataMember]
            public int Version { get; set; }

            /// <summary>
            /// Gets or sets the number of entities.
            /// </summary>
            [DataMember]
            public int Count { get; set; }
        }

        /// <summary>
        ///     The table entity.
        /// </summary>
        [DataContract]
        public class IndexTableEntity : TableEntity
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="IndexTableEntity"/> class.
            /// </summary>
            public IndexTableEntity()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="IndexTableEntity"/> class.
            /// </summary>
            /// <param name="itemId">
            /// The item id.
            /// </param>
            /// <param name="ownerId">
            /// The id of the owner.
            /// </param>
            public IndexTableEntity(Guid itemId, string ownerId)
            {
                this.RowKey = itemId.ToString("N");
                this.PartitionKey = ownerId;
            }

            /// <summary>
            ///     Gets the item id.
            /// </summary>
            [IgnoreDataMember]
            public Guid ItemId => Guid.Parse(this.RowKey);
        }
    }
}