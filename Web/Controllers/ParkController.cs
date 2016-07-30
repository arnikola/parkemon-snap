using System;
using System.Threading.Tasks;
using System.Web.Http;
using Orleans;
using ServiceCommon.Actors;

namespace Web.Controllers
{
    using ServiceCommon.Models;

    [RoutePrefix("api/park/{id}")]
    public class ParkController : ApiController
    {
        [Route]
        [HttpGet]
        public Task<ParkState> Get(Guid id) => Park(id).Get();

        private IParkGrain Park(Guid id) => GrainClient.GrainFactory.GetGrain<IParkGrain>(id);
    }
}