using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Services;

namespace SmartCampus.Controllers.Api;

[ApiController]
[Route("api/complaints")]
[Produces("application/json")]
public class ComplaintsApiController : ControllerBase
{
    private readonly IComplaintService _complaintService;

    public ComplaintsApiController(IComplaintService complaintService)
    {
        _complaintService = complaintService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ComplaintResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? userId, [FromQuery] string? status, [FromQuery] string? departmentId, [FromQuery] string? search)
    {
        var list = await _complaintService.GetComplaintsAsync(userId, status, departmentId, search);
        return Ok(list);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ComplaintResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var item = await _complaintService.GetComplaintByIdAsync(id);
        if (item == null)
        {
            return NotFound(new { message = $"Complaint with ID '{id}' not found." });
        }
        return Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ComplaintResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateComplaintDto dto, [FromQuery] string? userId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var effectiveUserId = userId ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(effectiveUserId))
        {
            return BadRequest(new { message = "UserId is required (either via authentication cookie or userId query parameter)." });
        }

        var created = await _complaintService.CreateComplaintAsync(effectiveUserId, dto);
        return Created($"/api/complaints/{created.Id}", created);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateComplaintDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var success = await _complaintService.UpdateComplaintAsync(id, dto);
        if (!success)
        {
            return NotFound(new { message = $"Complaint with ID '{id}' not found." });
        }
        return Ok(new { message = "Complaint updated successfully." });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, [FromQuery] string? userId)
    {
        var effectiveUserId = userId ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var isAdmin = User.IsInRole("Admin") || string.IsNullOrWhiteSpace(effectiveUserId);

        var success = await _complaintService.DeleteComplaintAsync(id, effectiveUserId, isAdmin);
        if (!success)
        {
            return NotFound(new { message = $"Complaint with ID '{id}' not found or access denied." });
        }
        return Ok(new { message = "Complaint deleted successfully." });
    }

    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateComplaintStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updatedBy = User.Identity?.Name ?? "Administration";
        var success = await _complaintService.UpdateComplaintStatusAsync(id, updatedBy, dto);
        if (!success)
        {
            return NotFound(new { message = $"Complaint with ID '{id}' not found." });
        }
        return Ok(new { message = $"Complaint status updated to '{dto.Status}'." });
    }
}

