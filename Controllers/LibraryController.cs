using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Models;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

[Authorize]
public class LibraryController : Controller
{
    private readonly ILibraryService _libraryService;

    public LibraryController(ILibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? category)
    {
        ViewBag.Search = search;
        ViewBag.SelectedCategory = category;
        ViewBag.Categories = await _libraryService.GetCategoriesAsync();

        var books = await _libraryService.GetBooksAsync(search, category);
        return View(books);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Student)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reserve(string bookId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _libraryService.ReserveBookAsync(userId, bookId);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = $"Book '{result.Reservation!.BookTitle}' reserved successfully. Check 'My Reservations' for pickup details.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error ?? "Failed to reserve book.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> MyReservations(string? status)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        ViewBag.StatusFilter = status;
        var list = await _libraryService.GetReservationsAsync(userId: userId, status: status);
        return View(list);
    }

    [HttpGet]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Reservations(string? status)
    {
        ViewBag.StatusFilter = status;
        var list = await _libraryService.GetReservationsAsync(status: status);
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelReservation(string id, string? returnAction)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole(UserRoles.Admin);

        var result = await _libraryService.CancelReservationAsync(id, userId, isAdmin);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Reservation cancelled successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error ?? "Failed to cancel reservation.";
        }

        return RedirectToAction(returnAction ?? nameof(MyReservations));
    }

    [HttpGet]
    [Authorize(Roles = UserRoles.Admin)]
    public IActionResult CreateBook()
    {
        return View(new CreateBookDto { TotalCopies = 3 });
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBook(CreateBookDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        await _libraryService.CreateBookAsync(dto);
        TempData["SuccessMessage"] = $"Book '{dto.Title}' added to library catalog.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> EditBook(string id)
    {
        var book = await _libraryService.GetBookByIdAsync(id);
        if (book == null) return NotFound();

        var model = new UpdateBookDto
        {
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            Category = book.Category,
            Description = book.Description,
            TotalCopies = book.TotalCopies,
            AvailableCopies = book.AvailableCopies
        };
        ViewBag.BookId = id;
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBook(string id, UpdateBookDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.BookId = id;
            return View(dto);
        }

        var success = await _libraryService.UpdateBookAsync(id, dto);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] =
            success ? "Book updated successfully." : "Failed to update book.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBook(string id)
    {
        var success = await _libraryService.DeleteBookAsync(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] =
            success ? "Book removed from library catalog." : "Failed to remove book.";

        return RedirectToAction(nameof(Index));
    }
}

