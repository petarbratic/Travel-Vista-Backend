using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Explorer.API.Controllers.Tourist;

[Authorize(Policy = "touristPolicy")]
[Route("api/tourist/welcome-bonus")]
public class WelcomeBonusController : ControllerBase
{
    private readonly IWelcomeBonusService _welcomeBonusService;

    public WelcomeBonusController(IWelcomeBonusService welcomeBonusService)
    {
        _welcomeBonusService = welcomeBonusService;
    }

    [HttpGet]
    public ActionResult<WelcomeBonusDto> GetWelcomeBonus()
    {
        try
        {
            var personId = GetPersonId();
            var bonus = _welcomeBonusService.GetWelcomeBonus(personId);
            return Ok(bonus);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Welcome bonus not found." });
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
