namespace ServiceCommon.Storage.Azure
{
    /// <summary>
    /// Description of an Azure Storage blob.
    /// </summary>
    public class AzureBlobInfo
    {
        /// <summary>
        /// Gets or sets the name without path of the file stored in the blob store.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the size of the blob.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the content type of the blob.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the absolute URI by which the blob can be retrieved from the store.
        /// </summary>
        public string Uri { get; set; }
    }
}
