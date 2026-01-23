using Explorer.Tours.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Explorer.Tours.API.Public.Tourist;

namespace Explorer.API.Controllers.Tourist
{
    [Authorize(Policy = "touristPolicy")]
    [Route("api/group-tours")]
    [ApiController]
    public class GroupTourSessionController : ControllerBase
    {
        private readonly IGroupTourSessionService _groupTourSessionService;

        public GroupTourSessionController(IGroupTourSessionService groupTourSessionService)
        {
            _groupTourSessionService = groupTourSessionService;
        }

        [HttpGet]
        public ActionResult<GroupTourSessionDto> GetSessionById([FromQuery] long sessionId)
        {
            try
            {
                var session = _groupTourSessionService.GetSessionById(sessionId);
                return Ok(session);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("club")]
        public ActionResult<List<GroupTourSessionDto>> GetSessionsByClubId([FromQuery] long clubId)
        {
            try
            {
                var sessions = _groupTourSessionService.GetSessionsByClubId(clubId);
                return Ok(sessions);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("club/active")]
        public ActionResult<List<GroupTourSessionDto>> GetActiveSessionsByClubId([FromQuery] long clubId)
        {
            try
            {
                var sessions = _groupTourSessionService.GetActiveSessionsByClubId(clubId);
                return Ok(sessions);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("club/ended")]
        public ActionResult<List<GroupTourSessionDto>> GetEndedSessionsByClubId([FromQuery] long clubId)
        {
            try
            {
                var sessions = _groupTourSessionService.GetEndedSessionsByClubId(clubId);
                return Ok(sessions);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("participants")]
        public ActionResult<List<GroupTourSessionParticipantDto>> GetOtherGroupParticipantsByTouristId([FromQuery] long touristId)
        {
            try
            {
                var participants = _groupTourSessionService.GetOtherGroupParticipantsByTouristId(touristId);
                return Ok(participants);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult<GroupTourSessionDto> CreateGroupTourSession([FromBody] CreateGroupTourSessionDto createDto)
        {
            try
            {
                var session = _groupTourSessionService.CreateGroupTourSession(createDto, GetStarterId());

                return CreatedAtAction(nameof(GetSessionById), new { id = session.Id }, session);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("leave")]
        public ActionResult<GroupTourSessionDto> LeaveGroupTourSession([FromQuery] long sessionId, [FromQuery] long touristId)
        {
            try
            {
                var session = _groupTourSessionService.LeaveGroupTourSession(sessionId, touristId);
                return Ok(session);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("join")]
        public ActionResult<GroupTourSessionDto> JoinGroupTourSession([FromQuery] long sessionId, [FromQuery] long touristId)
        {
            try
            {
                var session = _groupTourSessionService.JoinGroupTourSession(sessionId, touristId);
                return Ok(session);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private long GetStarterId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "personId" || c.Type == ClaimTypes.NameIdentifier);
            if (claim == null || !long.TryParse(claim.Value, out var starterId))
            {
                throw new UnauthorizedAccessException("User is not authenticated or personId claim is missing.");
            }
            return starterId;
        }
    }
}
