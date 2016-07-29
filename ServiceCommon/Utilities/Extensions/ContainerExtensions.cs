using System;
using System.Linq;
using Autofac;
using ServiceCommon.Utilities.Config;

namespace ServiceCommon.Utilities.Extensions
{
    public static class ContainerExtensions
    {
        /// <summary>
        /// Registers all configuration types.
        /// </summary>
        /// <param name="container">The container to register implementations with.</param>
        public static void RegisterConfigurations(this ContainerBuilder container)
        {
            // Get types which can be registered.
            var types =
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(_ => _.GetTypes())
                    .Where(
                        _ =>
                        !_.IsInterface && !_.IsAbstract && typeof(IConfiguration).IsAssignableFrom(_));

            // Register all types for each interface they implement as well as the type itself.
            foreach (var type in types)
            {
                container.RegisterType(type).SingleInstance().AsImplementedInterfaces().AsSelf();
            }
        }
    }
}
