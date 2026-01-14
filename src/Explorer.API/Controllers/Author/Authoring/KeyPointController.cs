using Explorer.BuildingBlocks.Core.UseCases;
using Explorer.Encounters.API.Dtos;
using Explorer.Encounters.API.Public;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Authoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Explorer.API.Controllers.Author.Authoring
{
    [Authorize(Policy = "authorPolicy")]
    [Route("api/keypoints")]
    [ApiController]
    public class KeyPointController : ControllerBase
    {
        private readonly IKeyPointService _keyPointService;
        private readonly IEncounterService _encounterService;

        public KeyPointController(IKeyPointService keyPointService, IEncounterService _encounterservice)
        {
            _keyPointService = keyPointService;
            _encounterService = _encounterservice;
        }

        // GET api/keypoints?page=0&pageSize=10
        [HttpGet]
        [AllowAnonymous]
        public ActionResult<PagedResult<KeyPointDto>> GetPaged(
            [FromQuery] long tourId,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 10)
        {
            var result = _keyPointService.GetPaged(tourId, page, pageSize);
            return Ok(result);
        }

        // GET api/keypoints/5
        [HttpGet("{id:long}")]
        [AllowAnonymous]
        public ActionResult<KeyPointDto> GetById(long id)
        {
            var result = _keyPointService.GetById(id);
            return Ok(result);
        }

        // POST api/keypoints
        // Jedan klik: kreira KeyPoint + (opciono) Encounter vezan za taj KeyPoint
        [HttpPost]
        public ActionResult<KeyPointDto> Create([FromBody] CreateKeyPointRequestDto dto)
        {
            var authorId = GetAuthorId();

            // 1) Kreiraj KeyPoint
            var createdKeyPoint = _keyPointService.Create(dto.KeyPoint, authorId);

            // 2) Ako korisnik nije uneo encounter deo forme -> gotovo
            if (dto.Encounter == null)
            {
                return CreatedAtAction(nameof(GetById), new { id = createdKeyPoint.Id }, createdKeyPoint);
            }

            // 3) Mapiraj Tours DTO -> EncounterDto (lokaciju UVEK uzimamo sa KeyPoint-a)
            var encounterToCreate = new EncounterDto
            {
                Name = dto.Encounter.Name,
                Description = dto.Encounter.Description,
                XP = dto.Encounter.XP,
                Type = dto.Encounter.Type,

                ActionDescription = dto.Encounter.ActionDescription,
                RequiredPeopleCount = dto.Encounter.RequiredPeopleCount,
                RangeInMeters = dto.Encounter.RangeInMeters,
                ImageUrl = dto.Encounter.ImageUrl,

                Latitude = createdKeyPoint.Latitude,
                Longitude = createdKeyPoint.Longitude,

                Status = "Active"
            };

            // 4) Kreiraj Encounter
            var createdEncounter = _encounterService.Create(encounterToCreate);

            // 5) Attach encounter na KeyPoint (upis EncounterId + IsMandatory)
            var updatedKeyPoint = _keyPointService.AttachEncounter(createdKeyPoint.Id, createdEncounter.Id, dto.Encounter.IsMandatory, authorId);

            return CreatedAtAction(nameof(GetById), new { id = updatedKeyPoint.Id }, updatedKeyPoint);
        }

        // PUT api/keypoints/5
        [HttpPut("{id:long}")]
        public ActionResult<KeyPointDto> Update(long id, [FromBody] KeyPointDto dto)
        {
            var existing = _keyPointService.GetById(id);

            // Sačuvaj staru lokaciju (da znamo da li se promenilo)
            var oldLat = existing.Latitude;
            var oldLng = existing.Longitude;

            // (opciono) brisanje stare slike ako je promenjena
            if (!string.IsNullOrWhiteSpace(existing.ImageUrl) &&
                existing.ImageUrl != dto.ImageUrl)
            {
                var fileName = Path.GetFileName(existing.ImageUrl);
                var filePath = Path.Combine("wwwroot", "uploads", "keypoint-images", fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            dto.Id = id;
            var authorId = GetAuthorId();

            // 1) Update keypoint
            var updatedKeyPoint = _keyPointService.Update(dto, authorId);

            // 2) Ako postoji encounter i lokacija se promenila -> sync encounter lokacije
            var locationChanged = oldLat != updatedKeyPoint.Latitude || oldLng != updatedKeyPoint.Longitude;

            if (locationChanged && updatedKeyPoint.EncounterId.HasValue)
            {
                var encounter = _encounterService.Get(updatedKeyPoint.EncounterId.Value);

                // Forsiraj lokaciju encountera = lokacija keypointa
                encounter.Latitude = updatedKeyPoint.Latitude;
                encounter.Longitude = updatedKeyPoint.Longitude;

                _encounterService.Update(encounter);
            }

            return Ok(updatedKeyPoint);
        }


        // DELETE api/keypoints/5
        [HttpDelete("{id:long}")]
        public IActionResult Delete(long id)
        {
            var kp = _keyPointService.GetById(id);

            if (!string.IsNullOrWhiteSpace(kp.ImageUrl))
            {
                var fileName = Path.GetFileName(kp.ImageUrl);
                var filePath = Path.Combine(
                    "wwwroot",
                    "uploads",
                    "keypoint-images",
                    fileName
                );

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            var authorId = GetAuthorId();
            _keyPointService.Delete(id, authorId);
            return NoContent();
        }

        // === IDENTIČNO TourController helper metodi ===
        private long GetAuthorId()
        {
            var claim = User.Claims.FirstOrDefault(c =>
                c.Type == "personId" || c.Type == ClaimTypes.NameIdentifier);

            if (claim == null || !long.TryParse(claim.Value, out var authorId))
            {
                throw new UnauthorizedAccessException("User is not authenticated or personId claim is missing.");
            }

            return authorId;
        }
    }
}
