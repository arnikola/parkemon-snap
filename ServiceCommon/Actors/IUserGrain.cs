using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using ServiceCommon.Models;
using ServiceCommon.Utilities.Observers;

namespace ServiceCommon.Actors
{
    public interface IUserGrain : IGrainWithStringKey
    {
    }

    public class UserGrain : Grain<UserState>, IUserGrain
    {
    }

    public class UserState : GrainState
    {
        public int GBP { get; set; }
        public string Name { get; set; }

        public string ImageUrl { get; set; }
        public string Email { get; set; }
    }
}