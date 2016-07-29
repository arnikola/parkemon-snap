using System;
using System.Threading.Tasks;
using Orleans;

namespace ServiceCommon.Utilities.Observers
{
    public interface IObservableGrain<out T> : IGrain
    {
        Task Resubscribe(Guid id, IGrainObserver<T> newChannel);
        Task Unsubscribe(Guid id);
    }
}