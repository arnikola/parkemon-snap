// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogRetriever.cs" company="Dapr Labs">
//   Copyright © Dapr Labs. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace LogRetriever
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// The log retrieval service.
    /// </summary>
    public class LogRetriever : ILogRetriever
    {
        private readonly string logDirectory = Path.GetTempPath();

        public string NodeName { get; }

        public LogRetriever()
        {
            this.NodeName = Dns.GetHostName();
        }

        /// <summary>
        /// Returns a list of log files on this node.
        /// </summary>
        /// <returns>
        /// A list of <see cref="SimpleFileInfo"/> describing the files on this node.
        /// </returns>
        public List<SimpleFileInfo> GetLocalLogFiles()
        {
            // Get my local files
            var filesArray = Directory.EnumerateFiles(this.logDirectory);
            var list = filesArray.Select(filepath => new FileInfo(filepath)).ToList();

            return
                list.Select(
                    _ =>
                    new SimpleFileInfo
                    {
                        Name = _.Name,
                        Size = _.Length,
                        Modified = _.LastWriteTimeUtc,
                        NodeName = this.NodeName
                    }).ToList();
        }

        /// <summary>
        /// Returns the context of a particular node.
        /// </summary>
        /// <param name="node">
        /// The node. 
        /// </param>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <returns>
        /// An <see cref="HttpResponseMessage"/> containing the requested log.
        /// </returns>
        public HttpResponseMessage GetLogFile(string node, string file)
        {
            var inStream = new FileStream(
                Path.Combine(this.logDirectory, file),
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);
            var sr = new StreamReader(inStream);
            var result = sr.ReadToEnd();


            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(result, Encoding.UTF8, "text/plaintext")
            };
        }

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
        public Func<Stream, HttpContent, TransportContext, Task> SubscribeToLogStream(string node, string file)
        {
            var dir = this.logDirectory;
            var reader = new FileStream(Path.Combine(dir, file), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            // Start at the end of the file
            var lastMaxOffset = new[] { Math.Max(0, reader.Length - 1024) };
            return async (output, context, transport) =>
            {
                try
                {
                    while (true)
                    {
                        // If the file size has not changed, return.
                        if (reader.Length == lastMaxOffset[0])
                        {
                            await Task.Delay(1000);
                            continue;
                        }

                        // Seek to the last max offset
                        reader.Seek(lastMaxOffset[0], SeekOrigin.Begin);

                        // Read the new bytes
                        const int Size = 1024 * 4;
                        var bytes = new byte[Size];
                        int read;
                        while ((read = await reader.ReadAsync(bytes, 0, Size)) > 0)
                        {
                            await output.WriteAsync(bytes, 0, read);
                            await output.FlushAsync();
                        }

                        // Update the last max offset
                        lastMaxOffset[0] = reader.Position;

                        await output.FlushAsync();
                    }
                }
                finally
                {
                    // Clean up.
                    reader.Dispose();
                    output.Close();
                }
            };
        }
    }
}