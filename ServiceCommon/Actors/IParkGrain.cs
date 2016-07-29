using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using ServiceCommon.Models;
using ServiceCommon.Utilities.Observers;

namespace ServiceCommon.Actors
{
    public interface IParkGrain : IGrainWithGuidKey
    {
        Task<string> WinHackathon();
    }

    public class ParkGrain : Grain<ParkState>, IParkGrain
    {
        public Task<string> WinHackathon()
        {
            return Task.FromResult("Congratulations guys!");
        }

    }

    public class ParkState : GrainState
    {
        
    }
}