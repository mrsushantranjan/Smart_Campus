using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Services;

namespace SmartCampus.Controllers.Api;

[ApiController]
[Route("api/service-requests")]
[Produces("application/json")]
public class ServiceRequestsApiController : ControllerBase
{
    private readonly IServiceRequestService _requestService;

    public ServiceRequestsApiController(IServiceRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ServiceRequestResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? userId, [FromQuery] string? status, [FromQuery] string? requestType)
    {
        var list = await _requestService.GetServiceRequestsAsync(userId, status, requestType);
        return Ok(list);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ServiceRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var item = await _requestService.GetServiceRequestByIdAsync(id);
        if (item == null)
        {
            return NotFound(new { message = $"Service request with ID '{id}' not found." });
        }
        return Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ServiceRequestResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateServiceRequestDto dto, [FromQuery] string? userId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var effectiveUserId = userId ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(effectiveUserId))
        {
            return BadRequest(new { message = "UserId is required (either via authentication cookie or userId query parameter)." });
        }

        var created = await _requestService.CreateServiceRequestAsync(effectiveUserId, dto);
        return Created($"/api/service-requests/{created.Id}", created);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateServiceRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var success = await _requestService.UpdateServiceRequestStatusAsync(id, dto.Status, dto.AdminRemarks);
        if (!success)
        {
            return NotFound(new { message = $"Service request with ID '{id}' not found." });
        }
        return Ok(new { message = $"Service request status updated to '{dto.Status}'." });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, [FromQuery] string? userId)
    {
        var effectiveUserId = userId ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var isAdmin = User.IsInRole("Admin") || string.IsNullOrWhiteSpace(effectiveUserId);

        var success = await _requestService.DeleteServiceRequestAsync(id, effectiveUserId, isAdmin);
        if (!success)
        {
            return NotFound(new { message = $"Service request with ID '{id}' not found or access denied." });
        }
        return Ok(new { message = "Service request deleted successfully." });
    }
}

