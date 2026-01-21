using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Explorer.API.Controllers.Author;

[Authorize(Policy = "authorPolicy")]
[Route("api/author/equipment")]
[ApiController]
public class AuthorEquipmentController : ControllerBase
{
    private readonly IEquipmentService _equipmentService;

    public AuthorEquipmentController(IEquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }

    [HttpGet]
    public ActionResult<List<EquipmentDto>> GetAll()
    {
        return Ok(_equipmentService.GetAll());
    }
}