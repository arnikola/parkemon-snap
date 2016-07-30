using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Orleans;
using ServiceCommon.Actors;

namespace Web.Controllers
{
    using ServiceCommon.Models;

    [RoutePrefix("api/map")]
    public class MapController : ApiController
    {
#warning IMPLEMENT THIS
        [Route]
        [HttpGet]
        public Task<List<FindResult>> Get(double lat, double lon) => Task.FromResult(new List<FindResult>());
    }
}