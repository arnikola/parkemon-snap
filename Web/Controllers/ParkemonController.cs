using System;
using System.Threading.Tasks;
using System.Web.Http;
using Orleans;
using ServiceCommon.Actors;

namespace Web.Controllers
{
    using ServiceCommon.Models;

    [RoutePrefix("api/parkemon")]
    public class ParkemonController : ApiController
    {
        [HttpGet]
        [Route("win")]
<<<<<<< master
        public Task<string> Win() => GrainClient.GrainFactory.GetGrain<IParkGrain>(new Guid()).WinHackathon();
=======
        public Task<string> Win() => GrainClient.GrainFactory.GetGrain<IParkGrain>(0).WinHackathon();

        [HttpGet]
        [Route("")]
        public Task<string> Lol() => Task.FromResult("/lord 9:6");
>>>>>>> local
    }
}