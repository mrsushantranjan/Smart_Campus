using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Services;

namespace SmartCampus.Controllers.Api;

[ApiController]
[Route("api/announcements")]
[Produces("application/json")]
public class AnnouncementsApiController : ControllerBase
{
    private readonly IAnnouncementService _announcementService;

    public AnnouncementsApiController(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<AnnouncementResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly, [FromQuery] string? search)
    {
        var list = await _announcementService.GetAnnouncementsAsync(activeOnly, search);
        return Ok(list);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AnnouncementResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var item = await _announcementService.GetAnnouncementByIdAsync(id);
        if (item == null)
        {
            return NotFound(new { message = $"Announcement with ID '{id}' not found." });
        }
        return Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AnnouncementResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAnnouncementDto dto, [FromQuery] string? publishedBy)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var publisher = publishedBy ?? User.Identity?.Name ?? "Admin Office";
        var created = await _announcementService.CreateAnnouncementAsync(publisher, dto);
        return Created($"/api/announcements/{created.Id}", created);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateAnnouncementDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var success = await _announcementService.UpdateAnnouncementAsync(id, dto);
        if (!success)
        {
            return NotFound(new { message = $"Announcement with ID '{id}' not found." });
        }
        return Ok(new { message = "Announcement updated successfully." });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var success = await _announcementService.DeleteAnnouncementAsync(id);
        if (!success)
        {
            return NotFound(new { message = $"Announcement with ID '{id}' not found." });
        }
        return Ok(new { message = "Announcement deleted successfully." });
    }
}

