using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using ServiceCommon.Models;
using ServiceCommon.Utilities.Observers;

namespace ServiceCommon.Actors
{
    public interface IAssociationGrain : IGrainWithGuidKey
    {
    }

    public class AssociationGrain : Grain<AssociationState>, IAssociationGrain
    {
    }

    public class AssociationState : GrainState
    {
        
    }
}