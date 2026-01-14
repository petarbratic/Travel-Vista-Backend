using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Administration;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explorer.API.Controllers.Administrator.Administration;

[Authorize(Policy = "administratorPolicy")]
[Route("api/admin/tour-problems")]
public class AdminTourProblemController : ControllerBase
{
    private readonly IAdminTourProblemService _adminTourProblemService;
    private readonly IPersonRepository _personRepository; 

    public AdminTourProblemController(
        IAdminTourProblemService service,
        IPersonRepository personRepository) 
    {
        _adminTourProblemService = service;
        _personRepository = personRepository; 
    }

    [HttpGet]
    public ActionResult<List<AdminTourProblemDto>> GetAll()
    {
        var result = _adminTourProblemService.GetAll();
        
        // Popuni imena za sve probleme
        foreach (var problem in result)
        {
            EnrichWithNames(problem);
        }
        
        return Ok(result);
    }

    [HttpGet("{id}")]
    public ActionResult<AdminTourProblemDto> GetById(long id)
    {
        var result = _adminTourProblemService.GetById(id);
        
        // Popuni imena
        EnrichWithNames(result);
        
        return Ok(result);
    }

    [HttpGet("overdue")]
    public ActionResult<List<AdminTourProblemDto>> GetOverdue([FromQuery] int daysThreshold = 5)
    {
        var result = _adminTourProblemService.GetOverdue(daysThreshold);
        
        // Popuni imena za sve probleme
        foreach (var problem in result)
        {
            EnrichWithNames(problem);
        }
        
        return Ok(result);
    }

    [HttpPost("{id}/deadline")]
    public IActionResult SetDeadline(long id, [FromBody] AdminDeadlineDto dto)
    {
        try
        {
            _adminTourProblemService.SetDeadline(id, dto.Deadline);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/close")]
    public IActionResult CloseProblem(long id)
    {
        _adminTourProblemService.CloseProblem(id);
        return Ok();
    }

    [HttpPost("{id}/penalize")]
    public IActionResult Penalize(long id)
    {
        _adminTourProblemService.PenalizeAuthor(id);
        return Ok();
    }

    [HttpPost("{id}/messages")]
    public ActionResult<AdminTourProblemDto> AddMessage(long id, [FromBody] AddMessageDto dto)
    {
        var adminId = GetAdminId();
        var result = _adminTourProblemService.AddAdminMessage(id, adminId, dto.Content);
        
        // Popuni imena
        EnrichWithNames(result);
        
        return Ok(result);
    }

    // NOVA HELPER METODA
    private void EnrichWithNames(AdminTourProblemDto dto)
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

    private long GetAdminId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "personId" || c.Type == ClaimTypes.NameIdentifier);
        if (claim == null || !long.TryParse(claim.Value, out var adminId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        return adminId;
    }
}
