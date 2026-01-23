using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Explorer.API.Controllers.Tourist
{
    [Authorize(Policy = "touristPolicy")]
    [Route("api/tourist/rank-rewards")]
    public class RankRewardController : ControllerBase
    {
        private readonly IRankRewardService _rankRewardService;

        public RankRewardController(IRankRewardService rankRewardService)
        {
            _rankRewardService = rankRewardService;
        }

        [HttpPost("claim")]
        public ActionResult<RankRewardClaimResultDto> ClaimRewards()
        {
            try
            {
                var touristId = GetPersonId();
                var result = _rankRewardService.ClaimRankRewards(touristId);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while claiming rewards.", details = ex.Message });
            }
        }

        private long GetPersonId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "personId" || c.Type == ClaimTypes.NameIdentifier);
            if (claim == null || !long.TryParse(claim.Value, out var id))
                throw new UnauthorizedAccessException("personId claim is missing.");
            return id;
        }
    }
}