using MongoDB.Driver;
using SmartCampus.Data;
using SmartCampus.DTOs;
using SmartCampus.Models;

namespace SmartCampus.Services;

public class ComplaintService : IComplaintService
{
    private readonly MongoDbContext _context;
    private readonly INotificationService _notificationService;

    public ComplaintService(MongoDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<List<ComplaintResponseDto>> GetComplaintsAsync(string? userId = null, string? status = null, string? departmentId = null, string? search = null)
    {
        var builder = Builders<Complaint>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            filter &= builder.Eq(c => c.UserId, userId);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filter &= builder.Eq(c => c.Status, status);
        }

        if (!string.IsNullOrWhiteSpace(departmentId))
        {
            filter &= builder.Eq(c => c.DepartmentId, departmentId);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            filter &= builder.Or(
                builder.Regex(c => c.Title, new MongoDB.Bson.BsonRegularExpression(searchLower, "i")),
                builder.Regex(c => c.Category, new MongoDB.Bson.BsonRegularExpression(searchLower, "i")),
                builder.Regex(c => c.DepartmentName, new MongoDB.Bson.BsonRegularExpression(searchLower, "i"))
            );
        }

        var complaints = await _context.Complaints.Find(filter)
            .SortByDescending(c => c.CreatedAt)
            .ToListAsync();

        return complaints.Select(MapComplaint).ToList();
    }

    public async Task<ComplaintResponseDto?> GetComplaintByIdAsync(string id)
    {
        var complaint = await _context.Complaints.Find(c => c.Id == id).FirstOrDefaultAsync();
        if (complaint == null) return null;

        var history = await GetComplaintHistoryAsync(id);
        var dto = MapComplaint(complaint);
        dto.History = history;
        return dto;
    }

    public async Task<ComplaintResponseDto> CreateComplaintAsync(string userId, CreateComplaintDto dto, string? imageUrl = null)
    {
        var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        var dept = await _context.Departments.Find(d => d.Id == dto.DepartmentId).FirstOrDefaultAsync();

        var complaint = new Complaint
        {
            UserId = userId,
            UserName = user?.FullName ?? "Unknown User",
            UserEmail = user?.Email ?? "",
            DepartmentId = dto.DepartmentId,
            DepartmentName = dept?.DepartmentName ?? "General Department",
            Title = dto.Title.Trim(),
            Category = dto.Category.Trim(),
            Description = dto.Description.Trim(),
            ImageUrl = imageUrl,
            Status = ComplaintStatuses.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Complaints.InsertOneAsync(complaint);

        // Add initial history entry
        var history = new ComplaintHistory
        {
            ComplaintId = complaint.Id,
            UpdatedBy = complaint.UserName,
            PreviousStatus = "None",
            NewStatus = ComplaintStatuses.Pending,
            Remarks = "Complaint submitted by student.",
            CreatedAt = DateTime.UtcNow
        };
        await _context.ComplaintHistory.InsertOneAsync(history);

        // Notify student of successful submission
        await _notificationService.CreateNotificationAsync(
            userId,
            "Complaint Submitted",
            $"Your complaint '{complaint.Title}' has been registered and is pending department review."
        );

        return MapComplaint(complaint);
    }

    public async Task<bool> UpdateComplaintAsync(string id, UpdateComplaintDto dto)
    {
        var complaint = await _context.Complaints.Find(c => c.Id == id).FirstOrDefaultAsync();
        if (complaint == null) return false;

        var dept = await _context.Departments.Find(d => d.Id == dto.DepartmentId).FirstOrDefaultAsync();

        var update = Builders<Complaint>.Update
            .Set(c => c.Title, dto.Title.Trim())
            .Set(c => c.Category, dto.Category.Trim())
            .Set(c => c.DepartmentId, dto.DepartmentId)
            .Set(c => c.DepartmentName, dept?.DepartmentName ?? complaint.DepartmentName)
            .Set(c => c.Description, dto.Description.Trim())
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        if (!string.IsNullOrWhiteSpace(dto.Status)) update = update.Set(c => c.Status, dto.Status);
        if (dto.AssignedTo != null) update = update.Set(c => c.AssignedTo, dto.AssignedTo);
        if (dto.ResolutionNotes != null) update = update.Set(c => c.ResolutionNotes, dto.ResolutionNotes);

        var result = await _context.Complaints.UpdateOneAsync(c => c.Id == id, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UpdateComplaintStatusAsync(string id, string updatedBy, UpdateComplaintStatusDto dto)
    {
        var complaint = await _context.Complaints.Find(c => c.Id == id).FirstOrDefaultAsync();
        if (complaint == null) return false;

        var prevStatus = complaint.Status;

        var update = Builders<Complaint>.Update
            .Set(c => c.Status, dto.Status)
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        if (!string.IsNullOrWhiteSpace(dto.AssignedTo))
        {
            update = update.Set(c => c.AssignedTo, dto.AssignedTo);
        }

        if (!string.IsNullOrWhiteSpace(dto.ResolutionNotes))
        {
            update = update.Set(c => c.ResolutionNotes, dto.ResolutionNotes);
        }

        await _context.Complaints.UpdateOneAsync(c => c.Id == id, update);

        // Add history record
        var history = new ComplaintHistory
        {
            ComplaintId = id,
            UpdatedBy = updatedBy,
            PreviousStatus = prevStatus,
            NewStatus = dto.Status,
            Remarks = string.IsNullOrWhiteSpace(dto.Remarks) ? $"Status updated to {dto.Status}" : dto.Remarks,
            CreatedAt = DateTime.UtcNow
        };
        await _context.ComplaintHistory.InsertOneAsync(history);

        // Notify user of status update
        await _notificationService.CreateNotificationAsync(
            complaint.UserId,
            "Complaint Status Update",
            $"Your complaint '{complaint.Title}' is now '{dto.Status}' by {updatedBy}."
        );

        return true;
    }

    public async Task<bool> DeleteComplaintAsync(string id, string currentUserId, bool isAdmin)
    {
        var complaint = await _context.Complaints.Find(c => c.Id == id).FirstOrDefaultAsync();
        if (complaint == null) return false;

        if (!isAdmin && complaint.UserId != currentUserId)
        {
            return false;
        }

        await _context.ComplaintHistory.DeleteManyAsync(h => h.ComplaintId == id);
        var result = await _context.Complaints.DeleteOneAsync(c => c.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<List<ComplaintHistoryDto>> GetComplaintHistoryAsync(string complaintId)
    {
        var history = await _context.ComplaintHistory.Find(h => h.ComplaintId == complaintId)
            .SortByDescending(h => h.CreatedAt)
            .ToListAsync();

        return history.Select(h => new ComplaintHistoryDto
        {
            Id = h.Id,
            ComplaintId = h.ComplaintId,
            UpdatedBy = h.UpdatedBy,
            PreviousStatus = h.PreviousStatus,
            NewStatus = h.NewStatus,
            Remarks = h.Remarks,
            CreatedAt = h.CreatedAt
        }).ToList();
    }

    private static ComplaintResponseDto MapComplaint(Complaint c) => new()
    {
        Id = c.Id,
        UserId = c.UserId,
        UserName = c.UserName,
        UserEmail = c.UserEmail,
        DepartmentId = c.DepartmentId,
        DepartmentName = c.DepartmentName,
        Title = c.Title,
        Category = c.Category,
        Description = c.Description,
        ImageUrl = c.ImageUrl,
        Status = c.Status,
        AssignedTo = c.AssignedTo,
        ResolutionNotes = c.ResolutionNotes,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };
}

