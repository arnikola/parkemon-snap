namespace ServiceCommon.Exceptions
{
    using System;
    using System.Net;

    [AttributeUsage(AttributeTargets.Class)]
    public class HttpExceptionAttribute : Attribute
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool HideStackTrace { get; set; } = true;
    }
}