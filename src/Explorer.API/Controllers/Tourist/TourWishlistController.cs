using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Tourist;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Explorer.API.Controllers.Tourist
{
    [Authorize(Policy = "touristPolicy")]
    [Route("api/tourist/wishlist")]
    [ApiController]
    public class TourWishlistController : ControllerBase
    {
        private readonly ITourWishlistService _wishlistService;

        public TourWishlistController(ITourWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        [HttpPost("{tourId}")]
        public ActionResult<TourWishlistDto> AddToWishlist(long tourId)
        {
            try
            {
                long touristId = GetTouristId();
                var result = _wishlistService.AddToWishlist(touristId, tourId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{tourId}")]
        public ActionResult RemoveFromWishlist(long tourId)
        {
            try
            {
                long touristId = GetTouristId();
                _wishlistService.RemoveFromWishlist(touristId, tourId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult<List<TourPreviewDto>> GetWishlistTours()
        {
            try
            {
                long touristId = GetTouristId();
                var result = _wishlistService.GetWishlistTours(touristId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{tourId}/check")]
        public ActionResult<bool> IsInWishlist(long tourId)
        {
            try
            {
                long touristId = GetTouristId();
                var result = _wishlistService.IsInWishlist(touristId, tourId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private long GetTouristId()
        {
            // Proveri sve claim-ove i loguj ih za debugging
            var allClaims = User.Claims.ToList();
            
            var personIdClaim = User.Claims.FirstOrDefault(c => c.Type == "personId");
            if (personIdClaim != null && long.TryParse(personIdClaim.Value, out var personId) && personId != 0)
            {
                return personId;
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId) && userId != 0)
            {
                return userId;
            }

            var idClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (idClaim != null && long.TryParse(idClaim.Value, out var id) && id != 0)
            {
                return id;
            }

            throw new UnauthorizedAccessException("Tourist ID not found in token. Available claims: " + string.Join(", ", allClaims.Select(c => $"{c.Type}={c.Value}")));
        }
    }
}
