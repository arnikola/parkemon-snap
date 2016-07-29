namespace LogRetriever
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///  Simplified file information.
    /// </summary>
    [Serializable]
    [DataContract]
    public class SimpleFileInfo
    {
        /// <summary>
        ///  The file name.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        ///  The file size.
        /// </summary>
        [DataMember]
        public long Size { get; set; }
        
        /// <summary>
        ///  The node the file is on.
        /// </summary>
        [DataMember]
        public string NodeName { get; set; }

        /// <summary>
        /// The modified time.
        /// </summary>
        [DataMember]
        public DateTimeOffset Modified { get; set; }
    }
}
