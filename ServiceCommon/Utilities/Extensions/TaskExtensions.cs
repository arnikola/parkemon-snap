using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ServiceCommon.Utilities.Extensions
{
    /// <summary>
    /// The task extensions.
    /// </summary>
    public static class TaskExtensions
    {
        public static void Ignored(this Task task)
        {
            task.GetAwaiter().UnsafeOnCompleted(() => { });
        }

        /// <summary>
        /// Continues the provided <paramref name="task"/> with a task returning <paramref name="result"/>, propagating
        /// exceptions and cancellation.
        /// </summary>
        /// <typeparam name="TResult">
        /// The resulting task type.
        /// </typeparam>
        /// <param name="task">
        /// The task.
        /// </param>
        /// <param name="result">
        /// The value returned on successful completion of <paramref name="task"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which is a continuation of <paramref name="task"/>, returning the provided
        /// <paramref name="result"/>.
        /// </returns>
        public static Task<TResult> ContinueWithResult<TResult>(this Task task, TResult result)
        {
            var completion = new TaskCompletionSource<TResult>();
            task.ContinueWith(
                antecedent =>
                    {
                        if (task.IsFaulted)
                        {
                            Debug.Assert(task.Exception != null, "task.Exception != null");
                            completion.TrySetException(task.Exception);
                        }
                        else if (task.IsCanceled)
                        {
                            completion.TrySetCanceled();
                        }

                        completion.TrySetResult(result);
                    }, 
                TaskContinuationOptions.ExecuteSynchronously);
            return completion.Task;
        }

        /// <summary>
        /// Continues the provided <paramref name="task"/> with the provided <paramref name="transform"/>, propagating
        /// exceptions and cancellation.
        /// </summary>
        /// <typeparam name="TInput">
        /// The underlying task type.
        /// </typeparam>
        /// <typeparam name="TResult">
        /// The resulting task type.
        /// </typeparam>
        /// <param name="task">
        /// The task.
        /// </param>
        /// <param name="transform">
        /// The transform to apply to the successful completion of <paramref name="task"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which is a continuation of <paramref name="task"/>, returning the transformed result
        /// defined by <paramref name="transform"/>.
        /// </returns>
        public static Task<TResult> ContinueWithSelect<TInput, TResult>(
            this Task<TInput> task, 
            Func<TInput, TResult> transform)
        {
            var completion = new TaskCompletionSource<TResult>();
            task.ContinueWith(
                antecedent =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.Assert(task.Exception != null, "task.Exception != null");
                        completion.TrySetException(task.Exception);
                    }
                    else if (task.IsCanceled)
                    {
                        completion.TrySetCanceled();
                    }
                    
                    try
                    {
                        completion.TrySetResult(transform(task.Result));
                    }
                    catch (Exception exception)
                    {
                        completion.TrySetException(exception);
                    }
                }, 
                TaskContinuationOptions.ExecuteSynchronously);
            return completion.Task;
        }

        /// <summary>
        /// Continues the provided <paramref name="task"/> with the provided <paramref name="func"/>, propagating
        /// exceptions and cancellation.
        /// </summary>
        /// <typeparam name="TResult">
        /// The resulting task type.
        /// </typeparam>
        /// <param name="task">
        /// The task.
        /// </param>
        /// <param name="func">
        /// The transform to apply to the successful completion of <paramref name="task"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which is a continuation of <paramref name="task"/>, returning the transformed result
        /// defined by <paramref name="func"/>.
        /// </returns>
        public static Task<TResult> ContinueWithDelegate<TResult>(
            this Task task, 
            Func<TResult> func)
        {
            var completion = new TaskCompletionSource<TResult>();
            task.ContinueWith(
                antecedent =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.Assert(task.Exception != null, "task.Exception != null");
                        completion.TrySetException(task.Exception);
                    }
                    else if (task.IsCanceled)
                    {
                        completion.TrySetCanceled();
                    }

                    completion.TrySetResult(func());
                }, 
                TaskContinuationOptions.ExecuteSynchronously);
            return completion.Task;
        }

        /// <summary>
        /// Continues the provided <paramref name="task"/> with the provided <paramref name="func"/>, propagating
        /// exceptions and cancellation.
        /// </summary>
        /// <typeparam name="T">
        /// The task type.
        /// </typeparam>
        /// <param name="task">
        /// The task.
        /// </param>
        /// <param name="func">
        /// The transform to apply to the successful completion of <paramref name="task"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the work performed.
        /// </returns>
        public static Task ContinueWithDelegate<T>(
            this Task<T> task, 
            Action<T> func)
        {
            var completion = new TaskCompletionSource<int>();
            task.ContinueWith(
                antecedent =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.Assert(task.Exception != null, "task.Exception != null");
                        completion.TrySetException(task.Exception);
                    }
                    else if (task.IsCanceled)
                    {
                        completion.TrySetCanceled();
                    }

                    try
                    {
                        func(antecedent.Result);
                        completion.TrySetResult(0);
                    }
                    catch (Exception e)
                    {
                        completion.TrySetException(e);
                    }
                }, 
                TaskContinuationOptions.ExecuteSynchronously);
            return completion.Task;
        }

        /// <summary>
        /// Continues the provided <paramref name="task"/> with the provided <paramref name="transform"/>, propagating
        /// exceptions and cancellation.
        /// </summary>
        /// <typeparam name="TInput">
        /// The underlying task type.
        /// </typeparam>
        /// <typeparam name="TResult">
        /// The resulting task type.
        /// </typeparam>
        /// <param name="task">
        /// The task.
        /// </param>
        /// <param name="transform">
        /// The transform to apply to the successful completion of <paramref name="task"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which is a continuation of <paramref name="task"/>, returning the transformed result
        /// defined by <paramref name="transform"/>.
        /// </returns>
        public static Task<TResult> ContinueWithTask<TInput, TResult>(
            this Task<TInput> task, 
            Func<TInput, Task<TResult>> transform)
        {
            var completion = new TaskCompletionSource<TResult>();
            task.ContinueWith(
                antecedent =>
                {
                    if (antecedent.IsFaulted)
                    {
                        Debug.Assert(antecedent.Exception != null, "task.Exception != null");
                        completion.TrySetException(antecedent.Exception);
                    }
                    else if (antecedent.IsCanceled)
                    {
                        completion.TrySetCanceled();
                    }
                    else
                    {
                        try
                        {
                            transform(antecedent.Result).PropagateTo(completion);
                        }
                        catch (Exception exception)
                        {
                            completion.TrySetException(exception);
                        }
                    }
                }, 
                TaskContinuationOptions.ExecuteSynchronously);
            return completion.Task;
        }

        /// <summary>
        /// Continues the provided <paramref name="task"/> with the provided <paramref name="action"/>, propagating
        /// exceptions and cancellation.
        /// </summary>
        /// <typeparam name="T">
        /// The underlying task type.
        /// </typeparam>
        /// <param name="task">
        /// The task.
        /// </param>
        /// <param name="action">
        /// The transform to apply to the successful completion of <paramref name="task"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which is a continuation of <paramref name="task"/>, returning the transformed result
        /// defined by <paramref name="action"/>.
        /// </returns>
        public static Task ContinueWithTask<T>(
            this Task<T> task,
            Func<T, Task> action)
        {
            var completion = new TaskCompletionSource<int>();
            task.ContinueWith(
                antecedent =>
                {
                    if (antecedent.IsFaulted)
                    {
                        Debug.Assert(antecedent.Exception != null, "task.Exception != null");
                        completion.TrySetException(antecedent.Exception);
                    }
                    else if (antecedent.IsCanceled)
                    {
                        completion.TrySetCanceled();
                    }
                    else
                    {
                        try
                        {
                            action(antecedent.Result).PropagateTo(completion);
                        }
                        catch (Exception exception)
                        {
                            completion.TrySetException(exception);
                        }
                    }
                },
                TaskContinuationOptions.ExecuteSynchronously);
            return completion.Task;
        }

        /// <summary>
        /// Continues the provided <paramref name="task"/> with the provided <paramref name="action"/>, propagating
        /// exceptions and cancellation.
        /// </summary>
        /// <typeparam name="T">
        /// The underlying task type.
        /// </typeparam>
        /// <param name="task">
        /// The task.
        /// </param>
        /// <param name="action">
        /// The transform to apply to the successful completion of <paramref name="task"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which is a continuation of <paramref name="task"/>, returning the transformed result
        /// defined by <paramref name="action"/>.
        /// </returns>
        public static Task ContinueWithTask(
            this Task task,
            Func<Task, Task> action)
        {
            var completion = new TaskCompletionSource<int>();
            task.ContinueWith(
                antecedent =>
                {
                    if (antecedent.IsFaulted)
                    {
                        Debug.Assert(antecedent.Exception != null, "task.Exception != null");
                        completion.TrySetException(antecedent.Exception);
                    }
                    else if (antecedent.IsCanceled)
                    {
                        completion.TrySetCanceled();
                    }
                    else
                    {
                        try
                        {
                            action(antecedent).PropagateTo(completion);
                        }
                        catch (Exception exception)
                        {
                            completion.TrySetException(exception);
                        }
                    }
                },
                TaskContinuationOptions.ExecuteSynchronously);
            return completion.Task;
        }

        /// <summary>
        /// Continues the provided <paramref name="task"/> with the provided <paramref name="continuation"/>, propagating
        /// exceptions and cancellation.
        /// </summary>
        /// <typeparam name="TResult">
        /// The resulting task type.
        /// </typeparam>
        /// <param name="task">
        /// The task.
        /// </param>
        /// <param name="continuation">
        /// The continuation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which is a continuation of <paramref name="task"/>.
        /// </returns>
        public static Task<TResult> ContinueWithTask<TResult>(
            this Task task, 
            Func<Task<TResult>> continuation)
        {
            var completion = new TaskCompletionSource<TResult>();
            task.ContinueWith(
                antecedent =>
                {
                    if (antecedent.IsFaulted)
                    {
                        Debug.Assert(antecedent.Exception != null, "task.Exception != null");
                        completion.TrySetException(antecedent.Exception);
                    }
                    else if (antecedent.IsCanceled)
                    {
                        completion.TrySetCanceled();
                    }
                    else
                    {
                        try
                        {
                            continuation().PropagateTo(completion);
                        }
                        catch (Exception exception)
                        {
                            completion.TrySetException(exception);
                        }
                    }
                }, 
                TaskContinuationOptions.ExecuteSynchronously);
            return completion.Task;
        }

        /// <summary>
        /// Propagates the result of <paramref name="task"/> to the provided <paramref name="completion"/>
        /// </summary>
        /// <typeparam name="T">
        /// The underlying task type.
        /// </typeparam>
        /// <param name="task">
        /// The task.
        /// </param>
        /// <param name="completion">
        /// The completion.
        /// </param>
        public static void PropagateTo<T>(this Task<T> task, TaskCompletionSource<T> completion)
        {
            task.ContinueWith(
                innerAntecedent =>
                {
                    if (innerAntecedent.IsFaulted)
                    {
                        Debug.Assert(innerAntecedent.Exception != null, "task.Exception != null");
                        completion.TrySetException(innerAntecedent.Exception);
                    }
                    else if (innerAntecedent.IsCanceled)
                    {
                        completion.TrySetCanceled();
                    }
                    else
                    {
                        completion.TrySetResult(innerAntecedent.Result);
                    }
                }, 
                TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Propagates the completion of <paramref name="task"/> to the provided <paramref name="completion"/>
        /// </summary>
        /// <typeparam name="T">
        /// The underlying task type.
        /// </typeparam>
        /// <param name="task">
        /// The task.
        /// </param>
        /// <param name="completion">
        /// The completion.
        /// </param>
        public static void PropagateTo<T>(this Task task, TaskCompletionSource<T> completion)
        {
            task.ContinueWith(
                innerAntecedent =>
                {
                    if (innerAntecedent.IsFaulted)
                    {
                        Debug.Assert(innerAntecedent.Exception != null, "task.Exception != null");
                        completion.TrySetException(innerAntecedent.Exception);
                    }
                    else if (innerAntecedent.IsCanceled)
                    {
                        completion.TrySetCanceled();
                    }
                    else
                    {
                        completion.TrySetResult(default(T));
                    }
                }, 
                TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}
