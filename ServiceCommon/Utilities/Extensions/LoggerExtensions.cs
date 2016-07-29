// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoggerExtensions.cs" company="Dapr Labs">
//   Copyright © Dapr Labs. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace ServiceCommon.Utilities.Extensions
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;

    using Orleans.Runtime;

    /// <summary>
    /// The logger extensions.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Log a warning about an exception.
        /// </summary>
        /// <param name="log">
        /// The log.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="member">
        /// The member.
        /// </param>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <param name="lineNumber">
        /// The line number.
        /// </param>
        public static void Warn(
            this Logger log,
            string message = null,
            Exception exception = null,
            [CallerMemberName] string member = null,
            [CallerFilePath] string file = null,
            [CallerLineNumber] int lineNumber = 0)
        {
            var resultMessage = new StringBuilder($"Error in {member} ({file}:{lineNumber})");
            if (message != null)
            {
                resultMessage.Append($": {message}.");
            }

            if (exception != null)
            {
                resultMessage.Append($" Exception: {exception.ToDetailedString()}");
            }
            
            var code = exception?.GetType().GetHashCode() ?? -1;
            log.Warn(code, resultMessage.ToString(), exception);
        }

        /// <summary>
        /// Log a warning about an exception.
        /// </summary>
        /// <param name="log">
        /// The log.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="member">
        /// The member.
        /// </param>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <param name="lineNumber">
        /// The line number.
        /// </param>
        public static void Error(
            this Logger log,
            string message = null,
            Exception exception = null,
            [CallerMemberName] string member = null,
            [CallerFilePath] string file = null,
            [CallerLineNumber] int lineNumber = 0)
        {
            var resultMessage = new StringBuilder($"Error in {member} ({file}:{lineNumber})");
            if (message != null)
            {
                resultMessage.Append($": {message}.");
            }

            if (exception != null)
            {
                resultMessage.Append($" Exception: {exception.ToDetailedString()}");
            }

            var code = exception?.GetType().GetHashCode() ?? -1;
            log.Error(code, resultMessage.ToString(), exception);
        }
    }
}
