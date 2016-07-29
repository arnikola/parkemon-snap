using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace ServiceCommon.Storage.Azure
{
    /// <summary>
    /// A cache of Azure storage tables.
    /// </summary>
    public static class TableCache
    {
        /// <summary>
        ///     The table clients.
        /// </summary>
        public static readonly ConcurrentDictionary<string, CloudTableClient> TableClients =
            new ConcurrentDictionary<string, CloudTableClient>();

        /// <summary>
        ///     The tables.
        /// </summary>
        public static readonly ConcurrentDictionary<string, CloudTable> Tables =
            new ConcurrentDictionary<string, CloudTable>();
        
        /// <summary>
        /// Returns the <see cref="CloudTable"/> with the provided <paramref name="tableName"/>.
        /// </summary>
        /// <param name="tableName">
        /// The container name.
        /// </param>
        /// <param name="connectionString">
        /// The connection string.
        /// </param>
        /// <returns>
        /// The <see cref="CloudTable"/> with the provided <paramref name="tableName"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="tableName"/> is null.
        /// </exception>
        public static Task<CloudTable> GetTable(string tableName, string connectionString)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("tableName");
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("connectionString");
            }

            // Retrieve a reference to a table.
            var tableClient = GetTableClient(connectionString);
            var table = GetTable(tableClient, tableName);

            // Create the table if it doesn't already exist
            return table.CreateIfNotExistsAsync().ContinueWith(
                _ =>
                    {
                        // Wait to propagate exceptions. This will not block.
                        _.Wait();
                        return table;
                    }, 
                TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// The get table.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="tableName">
        /// The table name.
        /// </param>
        /// <returns>
        /// The <see cref="CloudTable"/>.
        /// </returns>
        public static CloudTable GetTable(CloudTableClient client, string tableName)
        {
            return Tables.GetOrAdd(tableName, client.GetTableReference);
        }

        /// <summary>
        /// The get table client.
        /// </summary>
        /// <param name="connectionString">
        /// The connection string.
        /// </param>
        /// <returns>
        /// The <see cref="CloudTableClient"/>.
        /// </returns>
        private static CloudTableClient GetTableClient(string connectionString)
        {
            return TableClients.GetOrAdd(
                connectionString, 
                _ =>
                    {
                        // Retrieve storage account from connection string.
                        var storageAccount = CloudStorageAccount.Parse(connectionString);

                        // Create the table client 
                        var tableClient = storageAccount.CreateCloudTableClient();
                        tableClient.DefaultRequestOptions.PayloadFormat = TablePayloadFormat.JsonNoMetadata;
                        return tableClient;
                    });
        }
    }
}