using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Services;

namespace SmartCampus.Controllers.Api;

[ApiController, Route("api/reservations"), Produces("application/json")]
public class ReservationsApiController(ILibraryService libraryService) : ControllerBase
{
    private string? GetEffectiveUser(string? uid) => uid ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet]
    [ProducesResponseType(typeof(List<ReservationResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? userId, [FromQuery] string? status) =>
        Ok(await libraryService.GetReservationsAsync(userId, status));

    [HttpPost]
    [ProducesResponseType(typeof(ReservationResponseDto), StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateReservationDto dto, [FromQuery] string? userId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (GetEffectiveUser(userId) is not { } uid) return BadRequest(new { message = "UserId is required." });
        var res = await libraryService.ReserveBookAsync(uid, dto.BookId);
        return res.Succeeded ? Created($"/api/reservations/{res.Reservation!.Id}", res.Reservation) : BadRequest(new { message = res.Error });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(string id, [FromQuery] string? userId)
    {
        var uid = GetEffectiveUser(userId) ?? "";
        var res = await libraryService.CancelReservationAsync(id, uid, User.IsInRole("Admin") || string.IsNullOrWhiteSpace(uid));
        return res.Succeeded ? Ok(new { message = "Reservation cancelled successfully." }) : BadRequest(new { message = res.Error });
    }
}
