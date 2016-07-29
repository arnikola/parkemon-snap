
using System.Net;
using Autofac;
using ServiceCommon.Storage.Azure;
using ServiceCommon.Utilities.Serialization;

namespace ServiceCommon
{
    /// <summary>
    ///     The common services.
    /// </summary>
    public class CommonServices : Module
    {
        /// <summary>
        /// The configure container.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        protected override void Load(ContainerBuilder container)
        {
            ServicePointManager.Expect100Continue = false;

            container.RegisterInstance(SerializationSettings.JsonConfig);
            container.RegisterInstance(SerializationSettings.JsonSerializer);
            container.RegisterType<IndexTableFactory>().AsImplementedInterfaces();
            container.RegisterType<LookupTableFactory>().AsImplementedInterfaces();
        }
    }
}