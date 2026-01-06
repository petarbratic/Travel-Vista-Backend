using Explorer.Stakeholders.API.Public;
using Explorer.Stakeholders.Infrastructure.Authentication; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explorer.API.Controllers.Author
{
    [Authorize(Policy = "authorPolicy")]
    [Route("api/club-owner/club-join-request")]
    public class ClubOwnerJoinRequestController : Controller
    {
        private readonly IClubJoinRequestService _requestService;

        public ClubOwnerJoinRequestController(IClubJoinRequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpPost("{id:long}/respond")]
        public ActionResult Respond(long id, [FromQuery] bool accepted)
        {
            _requestService.Respond(User.PersonId(), id, accepted);
            return Ok();
        }
    }
}