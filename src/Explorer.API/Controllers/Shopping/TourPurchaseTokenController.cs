// src/Explorer.API/Controllers/Shopping/TourPurchaseController.cs
using Explorer.Payments.API.Dtos;
using Explorer.Payments.API.Public.Shopping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Explorer.API.Controllers.Shopping
{
    [Authorize(Policy = "touristPolicy")]
    [Route("api/tourist/purchase")]
    [ApiController]
    public class TourPurchaseController : ControllerBase
    {
        private readonly ITourPurchaseTokenService _purchaseService;

        public TourPurchaseController(ITourPurchaseTokenService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        // POST: api/tourist/purchase/checkout
        [HttpPost("checkout")]
        public ActionResult<CheckoutResultDto> Checkout()
        {
            try
            {
                var personId = GetPersonIdFromToken();
                var result = _purchaseService.Checkout(personId);

                // Ako kupovina nije uspjela (nedovoljno AC-a), vraćamo BadRequest
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                // Ako je uspjela, vraćamo OK sa rezultatom
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error: " + ex.Message });
            }
        }

        // GET: api/tourist/purchase
        [HttpGet]
        public ActionResult<List<TourPurchaseTokenDto>> GetTokens()
        {
            try
            {
                var personId = GetPersonIdFromToken();
                var result = _purchaseService.GetTokens(personId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error: " + ex.Message });
            }
        }

        // Helper metoda – ista logika kao u PersonController-u
        private long GetPersonIdFromToken()
        {
            // Primarni claim
            var personIdClaim = HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == "personId");
            if (personIdClaim != null &&
                long.TryParse(personIdClaim.Value, out var personId))
            {
                return personId;
            }

            // Sekundarni (za testove)
            var userIdClaim = HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null &&
                long.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("Person ID not found in token.");
        }
    }
}