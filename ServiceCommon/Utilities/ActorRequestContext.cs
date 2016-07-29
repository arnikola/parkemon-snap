namespace ServiceCommon.Utilities
{
    using System;

    using Orleans.Runtime;

    /// <summary>
    /// Manages the <see cref="Orleans.Runtime.RequestContext"/> dictionary.
    /// </summary>
    public static class ActorRequestContext
    {
        /// <summary>
        /// The user id <see cref="Orleans.Runtime.RequestContext"/> key.
        /// </summary>
        private const string UserId = "uid";

        /// <summary>
        /// The user secret <see cref="Orleans.Runtime.RequestContext"/> key.
        /// </summary>
        private const string UserSecret = "pw";

        /// <summary>
        /// Sets the current user id.
        /// </summary>
        /// <param name="userId">The user id.</param>
        public static void SetUserId(string userId) => RequestContext.Set(UserId, userId);

        /// <summary>
        /// Returns the current user id.
        /// </summary>
        /// <returns>The current user id.</returns>
        public static string GetUserId() => RequestContext.Get(UserId) as string ?? Guid.Empty.ToString("N"); // TODO: Don't default to this user!

        /// <summary>
        /// Sets a value in the context dictionary.
        /// </summary>
        /// <typeparam name="T">
        /// The value type.
        /// </typeparam>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public static void Set<T>(string key, T value) => RequestContext.Set(key, value);

        /// <summary>
        /// Gets a value from the context dictionary.
        /// </summary>
        /// <typeparam name="T">
        /// The value type.
        /// </typeparam>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The value.
        /// </returns>
        public static T Get<T>(string key) where T : class => RequestContext.Get(key) as T;

        /// <summary>
        /// Returns true if this request originates from from within the system, false otherwise.
        /// </summary>
        /// <returns>true if this request originates from within the system, false otherwise.</returns>
        public static bool IsSystem() => RequestContext.Get("uid") == null;

        public static void SetUserSecret(string secret) => RequestContext.Set(UserSecret, secret);

        public static string GetUserSecret() => RequestContext.Get(UserSecret) as string;
    }
}
