using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using ServiceCommon.Models;
using ServiceCommon.Utilities.Observers;

namespace ServiceCommon.Actors
{
    public interface IUserGrain : IGrainWithStringKey
    {
        Task<UserState> Get();
        Task AddReport(Report report);
        Task Update(UserProfile profile);
    }

    public class UserGrain : Grain<UserState>, IUserGrain
    {
        public Task<UserState> Get() => Task.FromResult(State);

        public Task Update(UserProfile profile)
        {
            this.State.Profile = profile;
            return this.WriteStateAsync();
        }

        public Task AddReport(Report report)
        {
            this.State.Reports = this.State.Reports ?? new List<Report>();
            this.State.Reports.Add(report);
            return this.WriteStateAsync();
        }
    }

    public class UserState : GrainState
    {
        public int GBP { get; set; }
        public UserProfile Profile { get; set; }
        public List<Report> Reports { get; set; }
    }

    public class UserProfile
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Email { get; set; }

    }
}