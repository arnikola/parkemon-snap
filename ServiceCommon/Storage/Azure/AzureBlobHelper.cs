namespace ServiceCommon.Storage.Azure
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// A helper class, for blobs.
    /// </summary>
    public static class AzureBlobHelper
    {
        /// <summary>
        /// Returns the <see cref="Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer"/> with the provided <paramref name="containerName"/>.
        /// </summary>
        /// <param name="containerName">
        /// The container name.
        /// </param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>
        /// The <see cref="Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer"/> with the provided <paramref name="containerName"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="containerName"/> is null.
        /// </exception>
        public static async Task<CloudBlobContainer> GetBlobContainer(string containerName, string connectionString)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException("containerName");
            }

            // Retrieve storage account from connection string.
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create the blob client 
            var blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container. Note that container name must use lower case
            var container = blobClient.GetContainerReference(containerName.ToLowerInvariant());


            // Create the container if it doesn't already exist
            await container.CreateIfNotExistsAsync().ConfigureAwait(false);

            // Enable public access to blob
            var permissions = await container.GetPermissionsAsync().ConfigureAwait(false);
            if (permissions.PublicAccess != BlobContainerPublicAccessType.Blob)
            {
                permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
                await container.SetPermissionsAsync(permissions).ConfigureAwait(false);
            }

            return container;
        }
    }
}