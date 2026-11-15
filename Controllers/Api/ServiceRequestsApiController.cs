using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Services;

namespace SmartCampus.Controllers.Api;

[ApiController, Route("api/service-requests"), Produces("application/json")]
public class ServiceRequestsApiController(IServiceRequestService requestService) : ControllerBase
{
    private string? GetEffectiveUser(string? uid) => uid ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet]
    [ProducesResponseType(typeof(List<ServiceRequestResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? userId, [FromQuery] string? status, [FromQuery] string? requestType) =>
        Ok(await requestService.GetServiceRequestsAsync(userId, status, requestType));

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ServiceRequestResponseDto), StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id) =>
        await requestService.GetServiceRequestByIdAsync(id) is { } item ? Ok(item) : NotFound(new { message = $"Service request with ID '{id}' not found." });

    [HttpPost]
    [ProducesResponseType(typeof(ServiceRequestResponseDto), StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateServiceRequestDto dto, [FromQuery] string? userId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (GetEffectiveUser(userId) is not { } uid) return BadRequest(new { message = "UserId is required." });
        var created = await requestService.CreateServiceRequestAsync(uid, dto);
        return Created($"/api/service-requests/{created.Id}", created);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateServiceRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await requestService.UpdateServiceRequestStatusAsync(id, dto.Status, dto.AdminRemarks)
            ? Ok(new { message = $"Service request status updated to '{dto.Status}'." })
            : NotFound(new { message = $"Service request with ID '{id}' not found." });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, [FromQuery] string? userId)
    {
        var uid = GetEffectiveUser(userId) ?? "";
        return await requestService.DeleteServiceRequestAsync(id, uid, User.IsInRole("Admin") || string.IsNullOrWhiteSpace(uid))
            ? Ok(new { message = "Service request deleted successfully." })
            : NotFound(new { message = $"Service request with ID '{id}' not found or access denied." });
    }
}
