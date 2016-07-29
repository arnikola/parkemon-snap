// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TableHelper.cs" company="Dapr Labs">
//   Copyright © Dapr Labs. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ServiceCommon.Utilities
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// A helper class, for tables.
    /// </summary>
    public static class TableHelper
    {
        /// <summary>
        /// Returns the <see cref="Microsoft.WindowsAzure.Storage.Table.CloudTable"/> with the provided <paramref name="tableName"/>.
        /// </summary>
        /// <param name="tableName">
        /// The table name.
        /// </param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>
        /// The <see cref="Microsoft.WindowsAzure.Storage.Table.CloudTable"/> with the provided <paramref name="tableName"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="tableName"/> is null.
        /// </exception>
        public static async Task<CloudTable> GetTable(string tableName, string connectionString)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("tableName");
            }

            // Retrieve storage account from connection string.
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create the table client 
            var tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve the table client.
            var table = tableClient.GetTableReference(tableName);

            // Create the table if it doesn't already exist
            await table.CreateIfNotExistsAsync().ConfigureAwait(false);
            
            return table;
        }
    }
}