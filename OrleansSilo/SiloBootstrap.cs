using System.Threading.Tasks;
using Orleans.Providers;
using Orleans.Runtime;

using ServiceCommon.Actors;
using ServiceCommon.Utilities.Serialization;

namespace OrleansSilo
{
    /// <summary>
    ///     The dependency resolver bootstrap.
    /// </summary>
    public class SiloBootstrap : IBootstrapProvider
    {
        /// <summary>
        /// The provider name.
        /// </summary>
        public const string ProviderName = nameof(SiloBootstrap);

        public Task Close()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Gets the name.
        /// </summary>
        public string Name => ProviderName;

        /// <summary>
        /// Performs initialization tasks.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="providerRuntime">
        /// The provider runtime.
        /// </param>
        /// <param name="providerConfiguration">
        /// The provider configuration.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the work performed.
        /// </returns>
        public async Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration providerConfiguration)
        {
            JsonSerialization.Register();
            RequestContext.PropagateActivityId = true;

            var log = providerRuntime.GetLogger("Bootstrap");
            log.Info("Running bootstrap.");

            await providerRuntime.GrainFactory.GetGrain<ITestGrain>(0).InitializeTestra();

            log.Info("Bootstrap complete.");
        }
    }
}