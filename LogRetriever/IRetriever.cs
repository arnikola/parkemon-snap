// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShortener.cs" company="Dapr Labs">
//   Copyright © Dapr Labs. All rights reserved.
// </copyright>
// <summary>
//   Interface for URL shorteners.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net;

namespace LogRetriever
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.ServiceModel;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for URL shorteners.
    /// </summary>
    [ServiceContract]
    public interface ILogRetriever
    {
        /// <summary>
        /// Returns a list of log files on this node.
        /// </summary>
        /// <returns>A list of log files on this node.</returns>
        [OperationContract]
        List<SimpleFileInfo> GetLocalLogFiles();
        
        /// <summary>
        /// Returns the specified log file on the correct node.
        /// </summary>
        /// <param name="node">
        /// The node. 
        /// </param>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <returns>
        /// The file.
        /// </returns>
        [OperationContract]
        HttpResponseMessage GetLogFile(string node, string file);

        /// <summary>
        /// Subscribes to the specified log file on the correct node.
        /// </summary>
        /// <param name="node">
        /// The node. 
        /// </param>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <returns>
        /// A stream which updates when the log file is changed.
        /// </returns>
        Func<Stream, HttpContent, TransportContext, Task> SubscribeToLogStream(string node, string file);
    }
}