namespace ServiceCommon.Utilities.Network.Http
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading.Tasks;

    using Microsoft.Owin;

    using Owin;

    /// <summary>
    /// The redirect to http middleware.
    /// </summary>
    public static class RedirectToHttpsMiddleware
    {
        /// <summary>
        /// The HTTPS port.
        /// </summary>
        private const uint HttpsPort = 443;

        /// <summary>
        /// Redirects all HTTP requests to HTTPS.
        /// </summary>
        /// <param name="app">
        /// The app.
        /// </param>
        /// <param name="port">
        /// The port to redirect to.
        /// </param>
        [DebuggerStepThrough]
        public static void RedirectToHttps(this IAppBuilder app, uint port = HttpsPort)
        {
            app.Use((ctx, next) => RedirectToHttps(port, ctx, next));
        }

        /// <summary>
        /// Redirects all HTTP requests to HTTPS.
        /// </summary>
        /// <param name="port">
        /// The port to redirect to.
        /// </param>
        /// <param name="ctx">
        /// The OWIN context.
        /// </param>
        /// <param name="next">
        /// The next middleware in the application pipeline.
        /// </param>
        [DebuggerStepThrough]
        private static Task RedirectToHttps(uint port, IOwinContext ctx, Func<Task> next)
        {
            var request = ctx.Request;
            var scheme = request.Scheme;
            if (IsSecureScheme(scheme))
            {
                return next();
            }

            var protocol = GetSecureScheme(scheme);
            var portString = string.Empty;
            if (port != HttpsPort)
            {
                portString = ":" + port.ToString(CultureInfo.InvariantCulture);
            }

            var redirect = string.Format("{0}://{1}{2}{3}", protocol, request.Uri.Host, portString, request.Uri.PathAndQuery);
            ctx.Response.Redirect(redirect);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Returns <see langword="true"/> if the provided scheme is secure, <see langword="false"/> otherwise.
        /// </summary>
        /// <param name="scheme">
        /// The protocol scheme.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the provided scheme is secure, <see langword="false"/> otherwise.
        /// </returns>
        private static bool IsSecureScheme(string scheme)
        {
            return scheme == "https" || scheme == "wss";
        }

       /// <summary>
       /// Returns the secure form of the provided protocol scheme.
       /// </summary>
       /// <param name="scheme">
       /// The protocol scheme.
       /// </param>
       /// <returns>
       /// The secure protocol scheme.
       /// </returns>
       /// <exception cref="ArgumentOutOfRangeException">
       /// The request scheme was not known.
       /// </exception>
       private static string GetSecureScheme(string scheme)
       {
           string protocol;
           switch (scheme)
           {
               case "https":
               case "http":
                   protocol = "https";
                   break;
               case "wss":
               case "ws":
                   protocol = "wss";
                   break;
               default:
                   throw new ArgumentOutOfRangeException("Unknown protocol scheme: " + scheme);
           }

           return protocol;
       }
    }
}
