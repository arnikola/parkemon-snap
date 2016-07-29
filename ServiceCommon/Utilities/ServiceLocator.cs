using Autofac;

namespace ServiceCommon.Utilities
{
    /// <summary>
    ///     The orleans actor services.
    /// </summary>
    public class ServiceLocator
    {
        /// <summary>
        ///     Gets or sets the container.
        /// </summary>
        public static IContainer Container { get; set; }
    }
}