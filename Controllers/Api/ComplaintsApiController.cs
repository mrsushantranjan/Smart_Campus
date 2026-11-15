using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Services;

namespace SmartCampus.Controllers.Api;

[ApiController, Route("api/complaints"), Produces("application/json")]
public class ComplaintsApiController(IComplaintService complaintService) : ControllerBase
{
    private string? GetEffectiveUser(string? uid) => uid ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet]
    [ProducesResponseType(typeof(List<ComplaintResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? userId, [FromQuery] string? status, [FromQuery] string? departmentId, [FromQuery] string? search) =>
        Ok(await complaintService.GetComplaintsAsync(userId, status, departmentId, search));

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ComplaintResponseDto), StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id) =>
        await complaintService.GetComplaintByIdAsync(id) is { } item ? Ok(item) : NotFound(new { message = $"Complaint with ID '{id}' not found." });

    [HttpPost]
    [ProducesResponseType(typeof(ComplaintResponseDto), StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateComplaintDto dto, [FromQuery] string? userId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (GetEffectiveUser(userId) is not { } uid) return BadRequest(new { message = "UserId is required." });
        var created = await complaintService.CreateComplaintAsync(uid, dto);
        return Created($"/api/complaints/{created.Id}", created);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateComplaintDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await complaintService.UpdateComplaintAsync(id, dto) ? Ok(new { message = "Complaint updated successfully." }) : NotFound(new { message = $"Complaint with ID '{id}' not found." });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, [FromQuery] string? userId)
    {
        var uid = GetEffectiveUser(userId) ?? "";
        return await complaintService.DeleteComplaintAsync(id, uid, User.IsInRole("Admin") || string.IsNullOrWhiteSpace(uid))
            ? Ok(new { message = "Complaint deleted successfully." })
            : NotFound(new { message = $"Complaint with ID '{id}' not found or access denied." });
    }

    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateComplaintStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await complaintService.UpdateComplaintStatusAsync(id, User.Identity?.Name ?? "Administration", dto)
            ? Ok(new { message = $"Complaint status updated to '{dto.Status}'." })
            : NotFound(new { message = $"Complaint with ID '{id}' not found." });
    }
}
