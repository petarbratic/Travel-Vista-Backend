using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Execution;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explorer.API.Controllers.Tourist.Execution;

[Authorize(Policy = "touristPolicy")]
[Route("api/tourist/tour-history")]
[ApiController]
public class TourHistoryController : ControllerBase
{
    private readonly ITourHistoryService _tourHistoryService;

    public TourHistoryController(ITourHistoryService tourHistoryService)
    {
        _tourHistoryService = tourHistoryService;
    }

    [HttpGet]
    public ActionResult<TourHistoryOverviewDto> GetTourHistory()
    {
        long touristId = long.Parse(User.FindFirst("id")!.Value);
        var result = _tourHistoryService.GetTourHistory(touristId);
        return Ok(result);
    }
}
