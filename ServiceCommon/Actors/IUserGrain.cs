using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using ServiceCommon.Models;
using ServiceCommon.Utilities.Observers;

namespace ServiceCommon.Actors
{
    public interface IUserGrain : IGrainWithGuidKey
    {
    }

    public class UserGrain : Grain<UserState>, IUserGrain
    {
    }

    public class UserState : GrainState
    {
        
    }
}