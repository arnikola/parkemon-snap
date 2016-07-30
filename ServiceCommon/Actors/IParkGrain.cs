using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using ServiceCommon.Models;
using ServiceCommon.Utilities.Observers;

namespace ServiceCommon.Actors
{
    public interface IParkGrain : IGrainWithIntegerKey
    {
        Task<string> WinHackathon();
        Task<ParkState> Get();
        Task Add(ParkData park);
    }

    public class ParkGrain : Grain<ParkState>, IParkGrain
    {
        public Task<string> WinHackathon()
        {
            return Task.FromResult("Congratulations guys!");
        }

        public Task<ParkState> Get() => Task.FromResult(State);

        public Task Add(ParkData park)
        {
            this.State.ParkData = park;
            return this.WriteStateAsync();
        }
    }

    public class ParkState : GrainState
    {
        public List<Bounty> Bounties { get; set; }
        public ParkData ParkData { get; set; }
    }
}