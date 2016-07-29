using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Orleans;
using ServiceCommon.Utilities.Extensions;

namespace ServiceCommon.Utilities.Observers
{
    public class ObserverHelper
    {
        private readonly IGrainFactory grainFactory;

        public ObserverHelper(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }
        
        public IObservable<T> Get<T, TGrain>(TGrain grain) where TGrain : IGrain, IObservableGrain<T>  => Observable.Create<T>(async (observer, cancellation) =>
        {
            // Pipe messages from the grain to the user.
            var client = new GrainObserver<T>(this.grainFactory, grain, observer);

            cancellation.Register(client.Dispose);
            try
            {
                // Subscribe for updates.
                await client.Subscribe();
                Trace.TraceInformation($"Created channel: {client.Id}");

                // Periodically, atomically renew the user's subscription with the grain.
                var renewal =
                    Observable.Interval(TimeSpan.FromMinutes(3))
                        .SelectMany(_ => client.Resubscribe().ToObservable())
                        .Do(
                            _ => { },
                            exception =>
                            {
                                Trace.TraceInformation(
                                    $"Error renewing subscription: {exception.ToDetailedString()}");

                                client.OnError(exception);
                            }).Subscribe();

                // Dispose subscriptions on cancellation.
                cancellation.Register(
                    () =>
                    {
                        Trace.TraceInformation($"Cleaning up client {client.Id}");
                        renewal?.Dispose();
                        client.Dispose();
                    });
            }
            catch (Exception exception)
            {
                client.OnError(exception);
            }

            return Disposable.Create(client.Dispose);
        });

        public IObservable<T> Get<T, TGrain>(string grainId) where TGrain : IGrainWithStringKey, IObservableGrain<T>
            => this.Get<T, TGrain>(this.grainFactory.GetGrain<TGrain>(grainId));

        public IObservable<T> Get<T, TGrain>(Guid grainId) where TGrain : IGrainWithGuidKey, IObservableGrain<T>
            => this.Get<T, TGrain>(this.grainFactory.GetGrain<TGrain>(grainId));

        public IObservable<T> Get<T, TGrain>(long grainId) where TGrain : IGrainWithIntegerKey, IObservableGrain<T>
            => this.Get<T, TGrain>(this.grainFactory.GetGrain<TGrain>(grainId));
        
        public IObservable<T> Get<T, TGrain>(Guid grainId, string keyExtension) where TGrain : IGrainWithGuidCompoundKey, IObservableGrain<T>
            => this.Get<T, TGrain>(this.grainFactory.GetGrain<TGrain>(grainId, keyExtension: keyExtension));

        public IObservable<T> Get<T, TGrain>(long grainId, string keyExtension)
            where TGrain : IGrainWithIntegerCompoundKey, IObservableGrain<T>
            => this.Get<T, TGrain>(this.grainFactory.GetGrain<TGrain>(grainId, keyExtension: keyExtension));


        public class GrainObserver<T> : IGrainObserver<T>, IDisposable, IObserver<T>
        {
            private readonly IGrainFactory grainFactory;

            private readonly IObservableGrain<T> grain;

            private IGrainObserver<T> channel;

            private IObserver<T> observer;

            public GrainObserver(IGrainFactory grainFactory, IObservableGrain<T> grain, IObserver<T> observer)
            {
                if (grainFactory == null)
                {
                    throw new ArgumentNullException(nameof(grainFactory));
                }

                if (grain == null)
                {
                    throw new ArgumentNullException(nameof(grain));
                }

                if (observer == null)
                {
                    throw new ArgumentNullException(nameof(observer));
                }

                this.grainFactory = grainFactory;
                this.grain = grain;
                this.observer = observer;
            }

            public Guid Id { get; } = Guid.NewGuid();

            public void OnNext(T data) => this.observer?.OnNext(data);

            public void OnError(Exception exception)
            {
                this.observer?.OnError(exception);
                this.observer = null;
                this.Dispose();
            }

            public void OnCompleted()
            {
                this.observer?.OnCompleted();
                this.observer = null;
                this.Dispose();
            }

            public async Task Subscribe()
            {
                this.channel = await this.grainFactory.CreateObjectReference<IGrainObserver<T>>(this);
                await this.grain.Resubscribe(this.Id, this.channel);
            }

            public async Task Resubscribe()
            {
                var oldChannel = this.channel;
                this.channel = await GrainClient.GrainFactory.CreateObjectReference<IGrainObserver<T>>(this);
                await this.grain.Resubscribe(this.Id, this.channel);

                // Unsubscribe from the old channel
                await this.TryDeleteChannel(oldChannel);
            }

            public async Task Unsubscribe()
            {
                var oldChannel = this.channel;
                this.channel = null;

                await this.grain.Unsubscribe(this.Id);
                await this.TryDeleteChannel(oldChannel);
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this.Unsubscribe().Ignored();
                    this.observer?.OnCompleted();
                    this.observer = null;
                }
            }

            /// <summary>
            /// Attempts to delete the provided channel, suppressing any exceptions.
            /// </summary>
            /// <param name="oldChannel">The channel.</param>
            private async Task TryDeleteChannel(IGrainObserver oldChannel)
            {
                try
                {
                    if (oldChannel != null)
                    {
                        await this.grainFactory.DeleteObjectReference<IGrainObserver<T>>(oldChannel);
                    }
                }
                catch (Exception exception)
                {
                    Trace.TraceWarning(
                        $"Failed to delete channel {oldChannel.GetPrimaryKey()} with exception: {exception.ToDetailedString()}");
                }
            }
        }
    }
}