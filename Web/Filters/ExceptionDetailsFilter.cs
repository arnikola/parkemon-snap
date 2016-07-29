namespace Web.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Web.Http.Filters;

    using ServiceCommon.Exceptions;
    using ServiceCommon.Utilities.Extensions;

    /// <summary>
    /// Outputs exceptions in a human readable format.
    /// </summary>
    public class ExceptionDetailsFilter : ActionFilterAttribute
    {
        /// <summary>
        /// Outputs exceptions in a human readable format.
        /// </summary>
        /// <param name="ctx">
        /// The action execution context.
        /// </param>
        public override void OnActionExecuted(HttpActionExecutedContext ctx)
        {
            if (ctx.Exception == null)
            {
                return;
            }

            ctx.Exception = Flatten(ctx.Exception);
            var statusCode = GetExceptionStatusCode(ctx.Exception);
            var message = GetExceptionMessage(ctx.Exception);

            ctx.Response = ctx.Response ?? ctx.Request.CreateResponse(statusCode);
            ctx.Response.Content = new StringContent(message, Encoding.UTF8, "text/plain");
            ctx.Response.StatusCode = statusCode;
        }

        private static Exception Flatten(Exception result)
        {
            var exception = result as AggregateException;
            while (exception != null)
            {
                exception.Flatten();

                if (exception.InnerException == null)
                {
                    break;
                }

                result = exception.InnerException;
                exception = result as AggregateException;
            }
            return result;
        }

        private static HttpStatusCode GetExceptionStatusCode(Exception exception)
        {
            var attribute = exception.GetType().GetCustomAttribute<HttpExceptionAttribute>();
            if (attribute != null)
            {
                return attribute.StatusCode;
            }

            if (exception is KeyNotFoundException)
            {
                return HttpStatusCode.NotFound;
            }

            return HttpStatusCode.InternalServerError;
        }

        private static string GetExceptionMessage(Exception exception)
        {
            var attribute = exception.GetType().GetCustomAttribute<HttpExceptionAttribute>();
            if (attribute != null && attribute.HideStackTrace)
            {
                return exception.Message;
            }

            return exception.ToDetailedString();
        }
    }
}
