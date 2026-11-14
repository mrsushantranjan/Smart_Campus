using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Services;

namespace SmartCampus.Controllers.Api;

[ApiController]
[Route("api/reservations")]
[Produces("application/json")]
public class ReservationsApiController : ControllerBase
{
    private readonly ILibraryService _libraryService;

    public ReservationsApiController(ILibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ReservationResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? userId, [FromQuery] string? status)
    {
        var list = await _libraryService.GetReservationsAsync(userId, status);
        return Ok(list);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReservationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateReservationDto dto, [FromQuery] string? userId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var effectiveUserId = userId ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(effectiveUserId))
        {
            return BadRequest(new { message = "UserId is required (either via authentication cookie or userId query parameter)." });
        }

        var result = await _libraryService.ReserveBookAsync(effectiveUserId, dto.BookId);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = result.Error });
        }

        return Created($"/api/reservations/{result.Reservation!.Id}", result.Reservation);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(string id, [FromQuery] string? userId)
    {
        var effectiveUserId = userId ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var isAdmin = User.IsInRole("Admin") || string.IsNullOrWhiteSpace(effectiveUserId);

        var result = await _libraryService.CancelReservationAsync(id, effectiveUserId, isAdmin);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = "Reservation cancelled successfully." });
    }
}

