using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ServiceCommon.Utilities.Extensions;

namespace ServiceCommon.Utilities
{
    public static class ExceptionLogging
    {
        /// <summary>
        /// Configures exception logging such that first-chance and unhandled exceptions are logged to the trace listener.
        /// </summary>
        /// <param name="firstChance">
        /// Whether or not to log first-chance exceptions.
        /// </param>
        public static void Setup(bool firstChance = true)
        {
            if (firstChance)
            {
                AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
                {
                    var exception = eventArgs.Exception;
                    if (exception != null)
                    {
                        var message = $"AppDomain.FirstChanceException: {exception.ToDetailedString()}";
                        Trace.TraceWarning(message);
                        ;
                    }
                };
            }

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                var exception = eventArgs.ExceptionObject as Exception;
                if (exception != null)
                {
                    var message = $"AppDomain.UnhandledException: {exception.ToDetailedString()}";
                    Trace.TraceError(message);
                }
            };

            TaskScheduler.UnobservedTaskException += (sender, eventArgs) =>
            {
                if (eventArgs.Exception != null)
                {
                    var message = $"TaskScheduler.UnobservedTaskException: {eventArgs.Exception.ToDetailedString()}";
                    Trace.TraceError(message);
                }
            };
        }
    }
}
