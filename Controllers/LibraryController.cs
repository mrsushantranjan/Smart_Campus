using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Models;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

[Authorize]
public class LibraryController(ILibraryService libraryService) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? category)
    {
        ViewBag.Search = search;
        ViewBag.SelectedCategory = category;
        ViewBag.Categories = await libraryService.GetCategoriesAsync();
        return View(await libraryService.GetBooksAsync(search, category));
    }

    [HttpPost, Authorize(Roles = UserRoles.Student), ValidateAntiForgeryToken]
    public async Task<IActionResult> Reserve(string bookId)
    {
        var res = await libraryService.ReserveBookAsync(UserId, bookId);
        TempData[res.Succeeded ? "SuccessMessage" : "ErrorMessage"] = res.Succeeded ? $"Book '{res.Reservation!.BookTitle}' reserved successfully." : res.Error ?? "Failed to reserve book.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> MyReservations(string? status)
    {
        ViewBag.StatusFilter = status;
        return View(await libraryService.GetReservationsAsync(userId: UserId, status: status));
    }

    [HttpGet, Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Reservations(string? status)
    {
        ViewBag.StatusFilter = status;
        return View(await libraryService.GetReservationsAsync(status: status));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelReservation(string id, string? returnAction)
    {
        var res = await libraryService.CancelReservationAsync(id, UserId, User.IsInRole(UserRoles.Admin));
        TempData[res.Succeeded ? "SuccessMessage" : "ErrorMessage"] = res.Succeeded ? "Reservation cancelled successfully." : res.Error ?? "Failed to cancel reservation.";
        return RedirectToAction(returnAction ?? nameof(MyReservations));
    }

    [HttpGet, Authorize(Roles = UserRoles.Admin)]
    public IActionResult CreateBook() => View(new CreateBookDto { TotalCopies = 3 });

    [HttpPost, Authorize(Roles = UserRoles.Admin), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBook(CreateBookDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await libraryService.CreateBookAsync(dto);
        TempData["SuccessMessage"] = $"Book '{dto.Title}' added to library catalog.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> EditBook(string id)
    {
        if (await libraryService.GetBookByIdAsync(id) is not { } b) return NotFound();
        ViewBag.BookId = id;
        return View(new UpdateBookDto { Title = b.Title, Author = b.Author, ISBN = b.ISBN, Category = b.Category, Description = b.Description, TotalCopies = b.TotalCopies, AvailableCopies = b.AvailableCopies });
    }

    [HttpPost, Authorize(Roles = UserRoles.Admin), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBook(string id, UpdateBookDto dto)
    {
        if (!ModelState.IsValid) { ViewBag.BookId = id; return View(dto); }
        var ok = await libraryService.UpdateBookAsync(id, dto);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Book updated successfully." : "Failed to update book.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Roles = UserRoles.Admin), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBook(string id)
    {
        var ok = await libraryService.DeleteBookAsync(id);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Book removed from library catalog." : "Failed to remove book.";
        return RedirectToAction(nameof(Index));
    }
}
