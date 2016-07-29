using System.Threading;
using System.Threading.Tasks;

namespace ServiceCommon.Utilities.Extensions
{
    /// <summary>
    /// The cancellation token extensions.
    /// </summary>
    public static class CancellationTokenExtensions
    {
        /// <summary>
        /// Returns a <see cref="Task"/> which completes when the provided <paramref name="cancellationToken"/> is cancelled.
        /// </summary>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which completes when the provided <paramref name="cancellationToken"/> is cancelled.
        /// </returns>
        public static Task WhenCancelled(this CancellationToken cancellationToken)
        {
            var completion = new TaskCompletionSource<int>();
            cancellationToken.Register(_ => ((TaskCompletionSource<int>)_).TrySetResult(0), completion);
            if (cancellationToken.IsCancellationRequested)
            {
                completion.TrySetResult(0);
            }

            return completion.Task;
        }
    }
}
