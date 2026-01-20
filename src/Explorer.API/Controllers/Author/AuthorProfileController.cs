using Explorer.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Explorer.API.Controllers.Author
{
    [ApiController]
    [Route("api/authors/me")]
    public class AuthorProfileController : ControllerBase
    {
        private readonly IAuthorProfileQueryService _service;

        public AuthorProfileController(IAuthorProfileQueryService service)
        {
            _service = service;
        }

        [HttpGet("profile-stats")]
        [Authorize]
        public IActionResult GetMyProfileStats()
        {
            long userId = GetUserId();
            return Ok(_service.GetMyStats(userId));
        }

        private long GetUserId()
        {
            var idStr =
                User.FindFirstValue("id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(idStr) || !long.TryParse(idStr, out var id))
                throw new UnauthorizedAccessException("User id claim missing.");

            return id;
        }
    }
}
