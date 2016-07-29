using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace OrleansSilo
{
    public class SiloServiceLocator
    {
        internal static IContainer Container { private get; set; }
        
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.Update(Container);
            return Container.Resolve<IServiceProvider>();
        }
    }
}