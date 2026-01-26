using Explorer.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explorer.API.Controllers.Author
{
    [ApiController]
    [Route("api/authors")]
    public class AuthorsPublicController : ControllerBase
    {
        private readonly IAuthorProfileQueryService _service;

        public AuthorsPublicController(IAuthorProfileQueryService service)
        {
            _service = service;
        }

        // Turista vidi top listu
        // npr: /api/authors/top?sort=rating&take=20
        [HttpGet("top")]
        [Authorize] // ili [AllowAnonymous] ako sme svako
        public IActionResult GetTopAuthors([FromQuery] string sort = "rating", [FromQuery] int take = 20)
        {
            return Ok(_service.GetTopAuthors(sort, take));
        }

        // Turista klikne autora i otvori profil
        // /api/authors/123/profile-stats
        [HttpGet("{authorId:long}/profile-stats")]
        [Authorize] // ili [AllowAnonymous]
        public IActionResult GetAuthorProfileStats([FromRoute] long authorId)
        {
            return Ok(_service.GetAuthorStats(authorId));
        }
    }
}

