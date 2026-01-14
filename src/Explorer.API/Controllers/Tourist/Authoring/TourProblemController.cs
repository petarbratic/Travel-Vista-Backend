using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Tourist;
using Explorer.Tours.Core.Domain.RepositoryInterfaces;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Explorer.API.Controllers.Tourist.Authoring;

[Authorize(Policy = "touristPolicy")]
[Route("api/tour-problems")]
public class TourProblemController : ControllerBase
{
    private readonly ITourProblemService _tourProblemService;
    private readonly ITourRepository _tourRepository;
    private readonly IPersonRepository _personRepository; 

    public TourProblemController(
        ITourProblemService tourProblemService, 
        ITourRepository tourRepository,
        IPersonRepository personRepository)
    {
        _tourProblemService = tourProblemService;
        _tourRepository = tourRepository;
        _personRepository = personRepository; 
    }

    [HttpPost]
    public ActionResult<TourProblemDto> Create([FromBody] TourProblemCreateDto problemDto)
    {
        var touristId = GetTouristId();
        var result = _tourProblemService.Create(problemDto, touristId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("my")]
    public ActionResult<List<TourProblemDto>> GetMyProblems()
    {
        var touristId = GetTouristId();
        var result = _tourProblemService.GetByTouristId(touristId);
        
        // Popuni imena za sve probleme
        foreach (var problem in result)
        {
            EnrichWithNames(problem);
        }
        
        return Ok(result);
    }

    [HttpGet("{id}")]
    public ActionResult<TourProblemDto> GetById(long id)
    {
        var touristId = GetTouristId();
        var result = _tourProblemService.GetById(id, touristId);
        
        // Popuni imena
        EnrichWithNames(result);
        
        return Ok(result);
    }

    [HttpPut("{id}")]
    public ActionResult<TourProblemDto> Update(long id, [FromBody] TourProblemUpdateDto problemDto)
    {
        problemDto.Id = id;
        var touristId = GetTouristId();
        var result = _tourProblemService.Update(problemDto, touristId);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(long id)
    {
        var touristId = GetTouristId();
        _tourProblemService.Delete(id, touristId);
        return NoContent();
    }

  
    [HttpGet("validate-tour/{tourId}")]
    public IActionResult ValidateTour(long tourId)
    {
        try
        {
            var tour = _tourRepository.GetById(tourId);
            return Ok(new { exists = tour != null });
        }
        catch
        {
            return Ok(new { exists = false });
        }
    }

    // Podtask 1
    [HttpPut("{id}/mark-resolved")]
    public ActionResult<TourProblemDto> MarkAsResolved(long id, [FromBody] MarkProblemResolvedDto dto)
    {
        var touristId = GetTouristId();
        var result = _tourProblemService.MarkAsResolved(id, dto.TouristComment, touristId);
        
        // Popuni imena
        EnrichWithNames(result);
        
        return Ok(result);
    }

    [HttpPut("{id}/mark-unresolved")]
    public ActionResult<TourProblemDto> MarkAsUnresolved(long id, [FromBody] MarkProblemResolvedDto dto)
    {
        var touristId = GetTouristId();
        var result = _tourProblemService.MarkAsUnresolved(id, dto.TouristComment, touristId);
        
        // Popuni imena
        EnrichWithNames(result);
        
        return Ok(result);
    }

    [HttpPost("{id}/messages")]
    public ActionResult<TourProblemDto> AddMessage(long id, [FromBody] AddMessageDto dto)
    {
        var touristId = GetTouristId();
        var result = _tourProblemService.AddMessage(id, touristId, dto.Content, 0); // 0 = Tourist
        
        // Popuni imena
        EnrichWithNames(result);
        
        return Ok(result);
    }

    // NOVA HELPER METODA
    private void EnrichWithNames(TourProblemDto dto)
    {
        foreach (var message in dto.Messages)
        {
            var person = _personRepository.GetByUserId(message.AuthorId);
            if (person != null)
            {
                message.SenderName = person.Name;
                message.SenderSurname = person.Surname;
            }
            else
            {
                message.SenderName = "Unknown";
                message.SenderSurname = "User";
            }
        }
    }

    // Helper metoda za ekstrakciju Tourist ID iz JWT tokena
    private long GetTouristId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "personId" || c.Type == ClaimTypes.NameIdentifier);
        if (claim == null || !long.TryParse(claim.Value, out var touristId))
        {
            throw new UnauthorizedAccessException("User is not authenticated or personId claim is missing.");
        }
        return touristId;
    }
}