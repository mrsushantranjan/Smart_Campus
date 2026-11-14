using SmartCampus.DTOs;
using SmartCampus.Models;

namespace SmartCampus.Services;

public interface ILibraryService
{
    Task<List<BookResponseDto>> GetBooksAsync(string? search = null, string? category = null);
    Task<BookResponseDto?> GetBookByIdAsync(string id);
    Task<BookResponseDto> CreateBookAsync(CreateBookDto dto);
    Task<bool> UpdateBookAsync(string id, UpdateBookDto dto);
    Task<bool> DeleteBookAsync(string id);

    Task<(bool Succeeded, string? Error, ReservationResponseDto? Reservation)> ReserveBookAsync(string userId, string bookId);
    Task<(bool Succeeded, string? Error)> CancelReservationAsync(string reservationId, string userId, bool isAdmin);
    Task<List<ReservationResponseDto>> GetReservationsAsync(string? userId = null, string? status = null);
    Task<List<string>> GetCategoriesAsync();
}

