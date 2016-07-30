using System;
using System.Threading.Tasks;
using System.Web.Http;
using Orleans;
using ServiceCommon.Actors;

namespace Web.Controllers
{
    using ServiceCommon.Models;

    [RoutePrefix("api/user/{userId}")]
    public class UserController : ApiController
    {
        [Route]
        [HttpGet]
        public Task<UserState> Get(string userId) => User(userId).Get();

        [Route("profile")]
        [HttpPost]
        public Task UpdateProfile(string userId, UserProfile profile) => User(userId).Update(profile);

        [Route("report")]
        [HttpPost]
        public Task AddReport(string userId, Report report) => User(userId).AddReport(report);

        private IUserGrain User(string userId) => GrainClient.GrainFactory.GetGrain<IUserGrain>(userId);
    }
}