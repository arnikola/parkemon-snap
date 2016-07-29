using Autofac;
using Autofac.Features.ResolveAnything;
using ServiceCommon;
using ServiceCommon.Utilities;
using ServiceCommon.Utilities.Config;
using ServiceCommon.Utilities.Extensions;

namespace OrleansSilo
{
    internal static class Startup
    {
        public static ILifetimeScope GetContainer(HostingEnvironment environment)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(environment);
            builder.RegisterConfigurations();
            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());

            // Load all modules.
            builder.RegisterModule<CommonServices>();

            return SiloServiceLocator.Container = ServiceLocator.Container = builder.Build();
        }
    }
}