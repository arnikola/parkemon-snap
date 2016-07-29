using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using ServiceCommon.Models;
using ServiceCommon.Utilities.Observers;

namespace ServiceCommon.Actors
{
    public interface IParkGrain : IGrainWithGuidKey, IObservableGrain<Update>
    {
        Task<string> WinHackathon();
    }

    public class ParkGrain : Grain<ParkState>, IParkGrain
    {
        //private Logger log;

        public Task<string> WinHackathon()
        {
            return Task.FromResult("Congratulations guys!");
        }

        public Task Resubscribe(Guid id, IGrainObserver<Update> newChannel)
        {
            return Task.FromResult(1);
        }

        public Task Unsubscribe(Guid id)
        {
            return Task.FromResult(1);
        }
    }

    public class ParkState : GrainState
    {
        
    }
}