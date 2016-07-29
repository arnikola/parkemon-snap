using System;
using WebStreams.Server;
using ServiceCommon.Actors;
using ServiceCommon.Utilities.Observers;

namespace Web.Streams
{
    using ServiceCommon.Models;

    [RoutePrefix("stream/park")]
    public class ParkStreamController
    {
        // Returns a stream of offers from the specified restaurant.
        public IObservable<Update> Get()
            => this.observerHelper.Get<Update, IParkGrain>(new Guid());

        private readonly ObserverHelper observerHelper;

        public ParkStreamController(ObserverHelper observerHelper)
        {
            this.observerHelper = observerHelper;
        }
    }
}