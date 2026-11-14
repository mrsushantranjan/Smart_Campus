using MongoDB.Driver;
using SmartCampus.Data;
using SmartCampus.DTOs;
using SmartCampus.Models;

namespace SmartCampus.Services;

public class LibraryService : ILibraryService
{
    private readonly MongoDbContext _context;
    private readonly INotificationService _notificationService;

    public LibraryService(MongoDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<List<BookResponseDto>> GetBooksAsync(string? search = null, string? category = null)
    {
        var builder = Builders<Book>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(category))
        {
            filter &= builder.Eq(b => b.Category, category);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            filter &= builder.Or(
                builder.Regex(b => b.Title, new MongoDB.Bson.BsonRegularExpression(searchLower, "i")),
                builder.Regex(b => b.Author, new MongoDB.Bson.BsonRegularExpression(searchLower, "i")),
                builder.Regex(b => b.ISBN, new MongoDB.Bson.BsonRegularExpression(searchLower, "i")),
                builder.Regex(b => b.Category, new MongoDB.Bson.BsonRegularExpression(searchLower, "i"))
            );
        }

        var books = await _context.Books.Find(filter)
            .SortBy(b => b.Title)
            .ToListAsync();

        return books.Select(MapBook).ToList();
    }

    public async Task<BookResponseDto?> GetBookByIdAsync(string id)
    {
        var book = await _context.Books.Find(b => b.Id == id).FirstOrDefaultAsync();
        return book == null ? null : MapBook(book);
    }

    public async Task<BookResponseDto> CreateBookAsync(CreateBookDto dto)
    {
        var book = new Book
        {
            Title = dto.Title.Trim(),
            Author = dto.Author.Trim(),
            ISBN = dto.ISBN.Trim(),
            Category = dto.Category.Trim(),
            Description = dto.Description?.Trim() ?? "",
            TotalCopies = Math.Max(1, dto.TotalCopies),
            AvailableCopies = Math.Max(1, dto.TotalCopies),
            CreatedAt = DateTime.UtcNow
        };

        await _context.Books.InsertOneAsync(book);
        return MapBook(book);
    }

    public async Task<bool> UpdateBookAsync(string id, UpdateBookDto dto)
    {
        var existing = await _context.Books.Find(b => b.Id == id).FirstOrDefaultAsync();
        if (existing == null) return false;

        var available = Math.Min(dto.AvailableCopies, dto.TotalCopies);

        var update = Builders<Book>.Update
            .Set(b => b.Title, dto.Title.Trim())
            .Set(b => b.Author, dto.Author.Trim())
            .Set(b => b.ISBN, dto.ISBN.Trim())
            .Set(b => b.Category, dto.Category.Trim())
            .Set(b => b.Description, dto.Description?.Trim() ?? "")
            .Set(b => b.TotalCopies, dto.TotalCopies)
            .Set(b => b.AvailableCopies, available);

        var result = await _context.Books.UpdateOneAsync(b => b.Id == id, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteBookAsync(string id)
    {
        // Cancel active reservations if any
        await _context.Reservations.UpdateManyAsync(
            r => r.BookId == id && r.Status == ReservationStatuses.Active,
            Builders<Reservation>.Update.Set(r => r.Status, ReservationStatuses.Cancelled)
        );

        var result = await _context.Books.DeleteOneAsync(b => b.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<(bool Succeeded, string? Error, ReservationResponseDto? Reservation)> ReserveBookAsync(string userId, string bookId)
    {
        var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null)
        {
            return (false, "User account not found.", null);
        }

        // Check if user already has an active reservation for this book
        var activeRes = await _context.Reservations.Find(r => r.UserId == userId && r.BookId == bookId && r.Status == ReservationStatuses.Active).FirstOrDefaultAsync();
        if (activeRes != null)
        {
            return (false, "You already have an active reservation for this book.", null);
        }

        // Atomic decrement of available copies: only if AvailableCopies > 0
        var filter = Builders<Book>.Filter.And(
            Builders<Book>.Filter.Eq(b => b.Id, bookId),
            Builders<Book>.Filter.Gt(b => b.AvailableCopies, 0)
        );

        var update = Builders<Book>.Update.Inc(b => b.AvailableCopies, -1);
        var options = new FindOneAndUpdateOptions<Book> { ReturnDocument = ReturnDocument.After };

        var updatedBook = await _context.Books.FindOneAndUpdateAsync(filter, update, options);
        if (updatedBook == null)
        {
            return (false, "Sorry, no copies of this book are currently available for reservation.", null);
        }

        var reservation = new Reservation
        {
            UserId = userId,
            UserName = user.FullName,
            BookId = bookId,
            BookTitle = updatedBook.Title,
            BookAuthor = updatedBook.Author,
            ReservationDate = DateTime.UtcNow,
            Status = ReservationStatuses.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Reservations.InsertOneAsync(reservation);

        await _notificationService.CreateNotificationAsync(
            userId,
            "Book Reserved",
            $"You have successfully reserved '{updatedBook.Title}'. Please collect it from the Central Library desk within 48 hours."
        );

        return (true, null, MapReservation(reservation));
    }

    public async Task<(bool Succeeded, string? Error)> CancelReservationAsync(string reservationId, string userId, bool isAdmin)
    {
        var res = await _context.Reservations.Find(r => r.Id == reservationId).FirstOrDefaultAsync();
        if (res == null)
        {
            return (false, "Reservation not found.");
        }

        if (!isAdmin && res.UserId != userId)
        {
            return (false, "You are not authorized to cancel this reservation.");
        }

        if (res.Status != ReservationStatuses.Active)
        {
            return (false, $"Cannot cancel reservation with status '{res.Status}'.");
        }

        // Mark reservation cancelled
        var updateRes = Builders<Reservation>.Update.Set(r => r.Status, ReservationStatuses.Cancelled);
        await _context.Reservations.UpdateOneAsync(r => r.Id == reservationId, updateRes);

        // Atomic increment of available copies back
        var updateBook = Builders<Book>.Update.Inc(b => b.AvailableCopies, 1);
        await _context.Books.UpdateOneAsync(b => b.Id == res.BookId, updateBook);

        await _notificationService.CreateNotificationAsync(
            res.UserId,
            "Reservation Cancelled",
            $"Your reservation for '{res.BookTitle}' has been cancelled."
        );

        return (true, null);
    }

    public async Task<List<ReservationResponseDto>> GetReservationsAsync(string? userId = null, string? status = null)
    {
        var builder = Builders<Reservation>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            filter &= builder.Eq(r => r.UserId, userId);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filter &= builder.Eq(r => r.Status, status);
        }

        var list = await _context.Reservations.Find(filter)
            .SortByDescending(r => r.CreatedAt)
            .ToListAsync();

        return list.Select(MapReservation).ToList();
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        var categories = await _context.Books.Distinct(b => b.Category, FilterDefinition<Book>.Empty).ToListAsync();
        return categories.Where(c => !string.IsNullOrWhiteSpace(c)).OrderBy(c => c).ToList();
    }

    private static BookResponseDto MapBook(Book b) => new()
    {
        Id = b.Id,
        Title = b.Title,
        Author = b.Author,
        ISBN = b.ISBN,
        Category = b.Category,
        Description = b.Description,
        TotalCopies = b.TotalCopies,
        AvailableCopies = b.AvailableCopies,
        CreatedAt = b.CreatedAt
    };

    private static ReservationResponseDto MapReservation(Reservation r) => new()
    {
        Id = r.Id,
        UserId = r.UserId,
        UserName = r.UserName,
        BookId = r.BookId,
        BookTitle = r.BookTitle,
        BookAuthor = r.BookAuthor,
        ReservationDate = r.ReservationDate,
        Status = r.Status,
        CreatedAt = r.CreatedAt
    };
}

