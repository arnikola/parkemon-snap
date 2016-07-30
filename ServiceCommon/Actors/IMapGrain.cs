using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using ServiceCommon.Models;
using ServiceCommon.Utilities.Observers;

namespace ServiceCommon.Actors
{
    public interface IMapGrain : IGrainWithGuidKey
    {
        Task AddPark(ParkData park);
    }

    public class MapGrain : Grain<MapState>, IMapGrain
    {
        private static List<Location> GetBoundingRectangle(IEnumerable<Location> outline)
        {
            var minLong = double.MaxValue;
            var minLat = double.MaxValue;
            var maxLong = double.MinValue;
            var maxLat = double.MinValue;

            foreach (var loc in outline)
            {
                if (loc.Long > maxLong) maxLong = loc.Long;
                if (loc.Long < minLong) minLong = loc.Long;
                if (loc.Lat > maxLat) maxLat = loc.Lat;
                if (loc.Lat < minLat) minLat = loc.Lat;
            }

            var bounds = new List<Location>
            {
                new Location {Lat = minLat, Long = minLong},
                new Location {Lat = maxLat, Long = maxLong}
            };

            return bounds;
        }

        public async Task AddPark(ParkData park)
        {
            var id = park.Id == default(Guid) ? Guid.NewGuid() : park.Id;
            park.Id = id;
            park.GeoFence = GetBoundingRectangle(park.GeoFence);
            var parkGrain = this.GrainFactory.GetGrain<IParkGrain>(id);
            await parkGrain.Add(park);
        }

        public Task<List<FindResult>> GetClosestParks()
        {


            return Task.FromResult(new List<FindResult>());
        }
    }

    public class MapState : GrainState
    {
        public List<ParkData> Parks { get; set; }
    }
}