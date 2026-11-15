using MongoDB.Bson;
using MongoDB.Driver;
using SmartCampus.Data;
using SmartCampus.DTOs;
using SmartCampus.Models;

namespace SmartCampus.Services;

public class LibraryService(MongoDbContext context, INotificationService notificationService) : ILibraryService
{
    public async Task<List<BookResponseDto>> GetBooksAsync(string? search = null, string? category = null)
    {
        var b = Builders<Book>.Filter;
        var filter = b.Empty;
        if (!string.IsNullOrWhiteSpace(category)) filter &= b.Eq(x => x.Category, category);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var reg = new BsonRegularExpression(search.Trim().ToLower(), "i");
            filter &= b.Or(b.Regex(x => x.Title, reg), b.Regex(x => x.Author, reg), b.Regex(x => x.ISBN, reg), b.Regex(x => x.Category, reg));
        }

        var books = await context.Books.Find(filter).SortBy(x => x.Title).ToListAsync();
        return books.Select(MapBook).ToList();
    }

    public async Task<BookResponseDto?> GetBookByIdAsync(string id) =>
        await context.Books.Find(b => b.Id == id).FirstOrDefaultAsync() is { } book ? MapBook(book) : null;

    public async Task<BookResponseDto> CreateBookAsync(CreateBookDto dto)
    {
        var book = new Book
        {
            Title = dto.Title.Trim(), Author = dto.Author.Trim(), ISBN = dto.ISBN.Trim(),
            Category = dto.Category.Trim(), Description = dto.Description?.Trim() ?? "",
            TotalCopies = Math.Max(1, dto.TotalCopies), AvailableCopies = Math.Max(1, dto.TotalCopies), CreatedAt = DateTime.UtcNow
        };
        await context.Books.InsertOneAsync(book);
        return MapBook(book);
    }

    public async Task<bool> UpdateBookAsync(string id, UpdateBookDto dto)
    {
        var existing = await context.Books.Find(b => b.Id == id).FirstOrDefaultAsync();
        if (existing == null) return false;

        var update = Builders<Book>.Update
            .Set(b => b.Title, dto.Title.Trim())
            .Set(b => b.Author, dto.Author.Trim())
            .Set(b => b.ISBN, dto.ISBN.Trim())
            .Set(b => b.Category, dto.Category.Trim())
            .Set(b => b.Description, dto.Description?.Trim() ?? "")
            .Set(b => b.TotalCopies, dto.TotalCopies)
            .Set(b => b.AvailableCopies, Math.Min(dto.AvailableCopies, dto.TotalCopies));

        return (await context.Books.UpdateOneAsync(b => b.Id == id, update)).ModifiedCount > 0;
    }

    public async Task<bool> DeleteBookAsync(string id)
    {
        await context.Reservations.UpdateManyAsync(
            r => r.BookId == id && r.Status == ReservationStatuses.Active,
            Builders<Reservation>.Update.Set(r => r.Status, ReservationStatuses.Cancelled));
        return (await context.Books.DeleteOneAsync(b => b.Id == id)).DeletedCount > 0;
    }

    public async Task<(bool Succeeded, string? Error, ReservationResponseDto? Reservation)> ReserveBookAsync(string userId, string bookId)
    {
        var user = await context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null) return (false, "User account not found.", null);

        if (await context.Reservations.Find(r => r.UserId == userId && r.BookId == bookId && r.Status == ReservationStatuses.Active).AnyAsync())
            return (false, "You already have an active reservation for this book.", null);

        var filter = Builders<Book>.Filter.And(Builders<Book>.Filter.Eq(b => b.Id, bookId), Builders<Book>.Filter.Gt(b => b.AvailableCopies, 0));
        var update = Builders<Book>.Update.Inc(b => b.AvailableCopies, -1);
        var updatedBook = await context.Books.FindOneAndUpdateAsync(filter, update, new() { ReturnDocument = ReturnDocument.After });
        if (updatedBook == null)
            return (false, "Sorry, no copies of this book are currently available for reservation.", null);

        var res = new Reservation
        {
            UserId = userId, UserName = user.FullName, BookId = bookId,
            BookTitle = updatedBook.Title, BookAuthor = updatedBook.Author,
            ReservationDate = DateTime.UtcNow, Status = ReservationStatuses.Active, CreatedAt = DateTime.UtcNow
        };
        await context.Reservations.InsertOneAsync(res);

        await notificationService.CreateNotificationAsync(
            userId, "Book Reserved", $"You have successfully reserved '{updatedBook.Title}'. Please collect it from the Central Library desk within 48 hours.");

        return (true, null, MapReservation(res));
    }

    public async Task<(bool Succeeded, string? Error)> CancelReservationAsync(string reservationId, string userId, bool isAdmin)
    {
        var res = await context.Reservations.Find(r => r.Id == reservationId).FirstOrDefaultAsync();
        if (res == null) return (false, "Reservation not found.");
        if (!isAdmin && res.UserId != userId) return (false, "You are not authorized to cancel this reservation.");
        if (res.Status != ReservationStatuses.Active) return (false, $"Cannot cancel reservation with status '{res.Status}'.");

        await context.Reservations.UpdateOneAsync(r => r.Id == reservationId, Builders<Reservation>.Update.Set(r => r.Status, ReservationStatuses.Cancelled));
        await context.Books.UpdateOneAsync(b => b.Id == res.BookId, Builders<Book>.Update.Inc(b => b.AvailableCopies, 1));

        await notificationService.CreateNotificationAsync(
            res.UserId, "Reservation Cancelled", $"Your reservation for '{res.BookTitle}' has been cancelled.");

        return (true, null);
    }

    public async Task<List<ReservationResponseDto>> GetReservationsAsync(string? userId = null, string? status = null)
    {
        var b = Builders<Reservation>.Filter;
        var filter = b.Empty;
        if (!string.IsNullOrWhiteSpace(userId)) filter &= b.Eq(r => r.UserId, userId);
        if (!string.IsNullOrWhiteSpace(status)) filter &= b.Eq(r => r.Status, status);

        var list = await context.Reservations.Find(filter).SortByDescending(r => r.CreatedAt).ToListAsync();
        return list.Select(MapReservation).ToList();
    }

    public async Task<List<string>> GetCategoriesAsync() =>
        (await context.Books.Distinct(b => b.Category, FilterDefinition<Book>.Empty).ToListAsync())
        .Where(c => !string.IsNullOrWhiteSpace(c)).OrderBy(c => c).ToList();

    private static BookResponseDto MapBook(Book b) => new()
    {
        Id = b.Id, Title = b.Title, Author = b.Author, ISBN = b.ISBN, Category = b.Category,
        Description = b.Description, TotalCopies = b.TotalCopies, AvailableCopies = b.AvailableCopies, CreatedAt = b.CreatedAt
    };

    private static ReservationResponseDto MapReservation(Reservation r) => new()
    {
        Id = r.Id, UserId = r.UserId, UserName = r.UserName, BookId = r.BookId,
        BookTitle = r.BookTitle, BookAuthor = r.BookAuthor, ReservationDate = r.ReservationDate,
        Status = r.Status, CreatedAt = r.CreatedAt
    };
}
