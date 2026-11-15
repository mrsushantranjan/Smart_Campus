using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Services;

namespace SmartCampus.Controllers.Api;

[ApiController, Route("api/announcements"), Produces("application/json")]
public class AnnouncementsApiController(IAnnouncementService announcementService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<AnnouncementResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly, [FromQuery] string? search) =>
        Ok(await announcementService.GetAnnouncementsAsync(activeOnly, search));

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AnnouncementResponseDto), StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id) =>
        await announcementService.GetAnnouncementByIdAsync(id) is { } item ? Ok(item) : NotFound(new { message = $"Announcement with ID '{id}' not found." });

    [HttpPost]
    [ProducesResponseType(typeof(AnnouncementResponseDto), StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAnnouncementDto dto, [FromQuery] string? publishedBy)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await announcementService.CreateAnnouncementAsync(publishedBy ?? User.Identity?.Name ?? "Admin Office", dto);
        return Created($"/api/announcements/{created.Id}", created);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateAnnouncementDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await announcementService.UpdateAnnouncementAsync(id, dto) ? Ok(new { message = "Announcement updated successfully." }) : NotFound(new { message = $"Announcement with ID '{id}' not found." });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id) =>
        await announcementService.DeleteAnnouncementAsync(id) ? Ok(new { message = "Announcement deleted successfully." }) : NotFound(new { message = $"Announcement with ID '{id}' not found." });
}
