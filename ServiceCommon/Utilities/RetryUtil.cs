using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceCommon.Utilities
{
    /// <summary>
    /// Methods for retrying actions.
    /// </summary>
    public static class RetryUtil
    {
        /// <summary>
        /// Attempt to execute <paramref name="func"/> until it succeeds or too many attempts have occurred.
        /// </summary>
        /// <param name="func">
        /// The delegate to invoke.
        /// </param>
        /// <param name="maxAttempts">
        /// The maximum number of attempts.
        /// </param>
        /// <param name="secondsBetweenRetries">
        /// The number of seconds between retries.
        /// </param>
        /// <typeparam name="T">
        /// The return type of <paramref name="func"/>.
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        /// <exception cref="TimeoutException">
        /// The operation failed.
        /// </exception>
        public static async Task<T> Retry<T>(Func<Task<T>> func, int maxAttempts = 5, double secondsBetweenRetries = 2)
        {
            var attempt = 0;
            var exception = default(ExceptionDispatchInfo);
            while (attempt < maxAttempts)
            {
                try
                {
                    return await func();
                }
                catch (Exception e)
                {
                    exception = ExceptionDispatchInfo.Capture(e);
                    attempt++;
                }

                if (attempt < maxAttempts)
                {
                    await Task.Delay(TimeSpan.FromSeconds(secondsBetweenRetries));
                }
            }

            if (exception != null)
            {
                exception.Throw();
            }

            throw new TimeoutException("Too many retries.");
        }

        /// <summary>
        /// Attempt to execute <paramref name="func"/> until it succeeds or too many attempts have occurred.
        /// </summary>
        /// <param name="func">
        /// The delegate to invoke.
        /// </param>
        /// <param name="maxAttempts">
        /// The maximum number of attempts
        /// </param>
        /// <param name="secondsBetweenRetries">
        /// The number of seconds between retries.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the work performed.
        /// </returns>
        /// <exception cref="TimeoutException">
        /// The operation failed.
        /// </exception>
        public static async Task Retry(Func<Task> func, int maxAttempts = 5, double secondsBetweenRetries = 2.0)
        {
            var attempt = 0;
            var exception = default(ExceptionDispatchInfo);
            while (attempt < maxAttempts)
            {
                try
                {
                    await func();
                    return;
                }
                catch (Exception e)
                {
                    exception = ExceptionDispatchInfo.Capture(e);
                    attempt++;
                }

                if (attempt < maxAttempts)
                {
                    await Task.Delay(TimeSpan.FromSeconds(secondsBetweenRetries));
                }
            }

            if (exception != null)
            {
                exception.Throw();
            }

            throw new TimeoutException("Too many retries.");
        }

        /// <summary>
        /// Attempt to execute <paramref name="func"/> until it succeeds or too many attempts have occurred.
        /// </summary>
        /// <param name="func">
        /// The delegate to invoke.
        /// </param>
        /// <param name="maxAttempts">
        /// The maximum number of attempts
        /// </param>
        /// <param name="secondsBetweenRetries">
        /// The number of seconds between retries.
        /// </param>
        /// <typeparam name="T">
        /// The return type of <paramref name="func"/>.
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        /// <exception cref="TimeoutException">
        /// The operation failed.
        /// </exception>
        public static T Retry<T>(Func<T> func, int maxAttempts = 5, double secondsBetweenRetries = 2)
        {
            var attempt = 0;
            var exception = default(ExceptionDispatchInfo);
            while (attempt < maxAttempts)
            {
                try
                {
                    return func();
                }
                catch (Exception e)
                {
                    exception = ExceptionDispatchInfo.Capture(e);
                    attempt++;
                }

                if (attempt < maxAttempts)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(secondsBetweenRetries));
                }
            }

            if (exception != null)
            {
                exception.Throw();
            }

            throw new TimeoutException("Too many retries.");
        }

        /// <summary>
        /// Attempt to execute <paramref name="func"/> until it succeeds or too many attempts have occurred.
        /// </summary>
        /// <param name="func">
        /// The delegate to invoke.
        /// </param>
        /// <param name="maxAttempts">
        /// The maximum number of attempts
        /// </param>
        /// <param name="secondsBetweenRetries">
        /// The number of seconds between retries.
        /// </param>
        /// <exception cref="TimeoutException">
        /// The operation failed.
        /// </exception>
        public static void Retry(Action func, int maxAttempts = 5, double secondsBetweenRetries = 2)
        {
            var attempt = 0;
            var exception = default(ExceptionDispatchInfo);
            while (attempt < maxAttempts)
            {
                try
                {
                    func();
                    return;
                }
                catch (Exception e)
                {
                    exception = ExceptionDispatchInfo.Capture(e);
                    attempt++;
                }

                if (attempt < maxAttempts)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(secondsBetweenRetries));
                }
            }

            if (exception != null)
            {
                exception.Throw();
            }

            throw new TimeoutException("Too many retries.");
        }
    }
}
