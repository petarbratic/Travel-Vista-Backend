using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Tourist;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Explorer.API.Controllers.Author.Authoring;

[Authorize(Policy = "authorPolicy")]
[Route("api/author/tour-problems")]
public class AuthorTourProblemController : ControllerBase
{
    private readonly ITourProblemService _tourProblemService;
    private readonly IPersonRepository _personRepository; 

    public AuthorTourProblemController(
        ITourProblemService tourProblemService,
        IPersonRepository personRepository) 
    {
        _tourProblemService = tourProblemService;
        _personRepository = personRepository;
    }

    // Podtask 1
    [HttpGet("my-tours")]
    public ActionResult<List<TourProblemDto>> GetProblemsOnMyTours()
    {
        var authorId = GetAuthorId();
        var result = _tourProblemService.GetByAuthorId(authorId);
        
        // Popuni imena
        foreach (var problem in result)
        {
            EnrichWithNames(problem);
        }
        
        return Ok(result);
    }

    [HttpGet("{id}")]
    public ActionResult<TourProblemDto> GetById(long id)
    {
        var authorId = GetAuthorId();
        var problem = _tourProblemService.GetById(id, authorId);

        // Dodatna validacija 
        if (problem.AuthorId != authorId)
        {
            return Forbid();
        }

        // Popuni imena
        EnrichWithNames(problem);

        return Ok(problem);
    }

    [HttpPost("{id}/messages")]
    public ActionResult<TourProblemDto> AddMessage(long id, [FromBody] AddMessageDto dto)
    {
        var authorId = GetAuthorId();
        var result = _tourProblemService.AddMessage(id, authorId, dto.Content, 1); // 1 = Author
        
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

    // Helper metoda za ekstrakciju Author ID iz JWT tokena
    private long GetAuthorId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "personId" || c.Type == ClaimTypes.NameIdentifier);
        if (claim == null || !long.TryParse(claim.Value, out var authorId))
        {
            throw new UnauthorizedAccessException("User is not authenticated or personId claim is missing.");
        }
        return authorId;
    }
}