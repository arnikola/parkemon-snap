// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BatchController.cs" company="Dapr Labs">
//   Copyright © Dapr Labs. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Net.Http.Headers;

namespace LogRetriever
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;

    /// <summary>
    ///     The Item controller.
    /// </summary>
    [RoutePrefix("api/logs")]
    public class LogRetrieverController : ApiController
    {
        private readonly ILogRetriever logRetriever;

        public LogRetrieverController(ILogRetriever logRetriever)
        {
            this.logRetriever = logRetriever;
        }

        /// <summary>
        /// Returns a list of the log files on the current node.
        /// </summary>
        /// <returns>
        /// A list of <see cref="SimpleFileInfo"/> items describing all logs on the selected node.
        /// </returns>
        [HttpGet]
        [Route("local")]
        public Task<List<SimpleFileInfo>> GetLocalLogs()
        {
            return Task.FromResult(this.logRetriever.GetLocalLogFiles());
        }

        /// <summary>
        /// Returns the requested log.
        /// </summary>
        /// <param name="nodename">
        /// The name of the node we want to get a file from.
        /// </param>
        /// <param name="filename">
        /// The name of the file we want to retrieve.
        /// </param>
        /// <returns>
        /// The file.
        /// </returns>
        [HttpGet]
        [Route("{nodename}/{filename}")]
        public HttpResponseMessage GetLog(string nodename, string filename)
        {
            return this.logRetriever.GetLogFile(nodename, filename);
        }

        /// <summary>
        /// Returns a stream that will update when the requested log changes.
        /// </summary>
        /// <param name="nodename">
        /// The name of the node we want to get a file from.
        /// </param>
        /// <param name="filename">
        /// The name of the file we want to retrieve.
        /// </param>
        /// <returns>
        /// The stream.
        /// </returns>
        [HttpGet]
        [Route("{nodename}/{filename}/subscribe")]
        public HttpResponseMessage SubscribeToLog(string nodename, string filename)
        {
            var response = this.Request.CreateResponse();
            response.Content = new PushStreamContent(this.logRetriever.SubscribeToLogStream(nodename, filename), new MediaTypeHeaderValue("text/plain"));
            response.Headers.TransferEncodingChunked = true;

            return response;
        }
    }
}