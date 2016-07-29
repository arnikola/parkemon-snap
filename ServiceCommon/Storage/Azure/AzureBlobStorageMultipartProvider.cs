// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   The Azure blob storage multipart provider.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ServiceCommon.Storage.Azure
{
    /// <summary>
    /// The Azure blob storage multipart provider.
    /// </summary>
    public class AzureBlobStorageMultipartProvider : MultipartFileStreamProvider
    {
        /// <summary>
        /// The container.
        /// </summary>
        private CloudBlobContainer blobContainer;

        /// <summary>
        /// The function used to get blob names.
        /// </summary>
        private readonly Func<string, Func<MultipartFileData, string>> getBlobName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageMultipartProvider"/> class.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <param name="rootPath">
        /// The root path where files are saved.
        /// </param>
        /// <param name="getBlobName">
        /// The function used to get blob names from file data.
        /// </param>
        ///         ///
        public AzureBlobStorageMultipartProvider(CloudBlobContainer container, string rootPath, Func<string, Func<MultipartFileData, string>> getBlobName)
            : base(rootPath)
        {
            this.getBlobName = getBlobName;
            this.Initialize(container);
        }

        /// <summary>
        /// Gets the azure blobs.
        /// </summary>
        public List<AzureBlobInfo> AzureBlobs { get; private set; }

        /// <summary>
        /// Post-processes form data.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the work performed.
        /// </returns>
        public override async Task ExecutePostProcessingAsync()
        {
            // Upload the files asynchronously to azure blob storage and remove them from local disk when done
            foreach (var fileData in this.FileData)
            {
                // Get the image type. If not png or jpg, throw an exception.
                var contentType = fileData.Headers.ContentType?.MediaType ?? "image/jpeg";
                if (contentType != "image/png" && contentType != "image/jpg" && contentType != "image/jpeg")
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                // Get the blob name from the Content-Disposition header if present
                var blobName = this.getBlobName(contentType.Split('/').Last())(fileData);

                // Retrieve reference to a blob
                var blob = this.blobContainer.GetBlockBlobReference(blobName);

                // Pick content type if present
                blob.Properties.ContentType = contentType;
                
                var blobInfo = new AzureBlobInfo
                               {
                                   ContentType = blob.Properties.ContentType,
                                   Name = blob.Name,
                                   Size = blob.Properties.Length,
                                   Uri = blob.Uri.ToString()
                               };

                // Upload content to blob storage
                using (var fileStream = new FileStream(fileData.LocalFileName, FileMode.Open, FileAccess.Read, FileShare.Read, this.BufferSize, true))
                {
                    // Upload the resized image to storage.
                    await blob.UploadFromStreamAsync(fileStream);
                }

                // Delete local file
                File.Delete(fileData.LocalFileName);
                this.AzureBlobs.Add(blobInfo);
            }

            await base.ExecutePostProcessingAsync();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="container">The blob container.</param>
        private void Initialize(CloudBlobContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            this.blobContainer = container;
            this.AzureBlobs = new List<AzureBlobInfo>();
        }
    }
}