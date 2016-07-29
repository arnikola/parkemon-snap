using Orleans;

namespace ServiceCommon.Utilities.Observers
{
    public interface IGrainObserver<in T> : IGrainObserver
    {
        void OnNext(T data);
    }
}