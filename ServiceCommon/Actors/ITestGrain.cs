using System.Threading.Tasks;
using Orleans;

namespace ServiceCommon.Actors
{
    using System;
    using Orleans.Runtime;

    using ServiceCommon.Config;

    public interface ITestGrain : IGrainWithIntegerKey
    {
        Task InitializeTestra();
    }

    public class TestGrain : Grain, ITestGrain
    {
        public async Task InitializeTestra()
        {
            this.logger.Info("Initializing test data.");
            var win = this.GrainFactory.GetGrain<IParkGrain>(0);
            await win.WinHackathon();
        }

        private Logger logger;


        public TestGrain(StorageConfig storageConfig)
        {
        }

        public override Task OnActivateAsync()
        {
            this.logger = this.GetLogger("TestData");
            return base.OnActivateAsync();
        }
    }
}
