using MongoDB.Bson;
using MongoDB.Driver;
using SmartCampus.Data;
using SmartCampus.DTOs;
using SmartCampus.Models;

namespace SmartCampus.Services;

public class ComplaintService(MongoDbContext context, INotificationService notificationService) : IComplaintService
{
    public async Task<List<ComplaintResponseDto>> GetComplaintsAsync(string? userId = null, string? status = null, string? departmentId = null, string? search = null)
    {
        var b = Builders<Complaint>.Filter;
        var filter = b.Empty;
        if (!string.IsNullOrWhiteSpace(userId)) filter &= b.Eq(c => c.UserId, userId);
        if (!string.IsNullOrWhiteSpace(status)) filter &= b.Eq(c => c.Status, status);
        if (!string.IsNullOrWhiteSpace(departmentId)) filter &= b.Eq(c => c.DepartmentId, departmentId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var reg = new BsonRegularExpression(search.Trim().ToLower(), "i");
            filter &= b.Or(b.Regex(c => c.Title, reg), b.Regex(c => c.Category, reg), b.Regex(c => c.DepartmentName, reg));
        }

        var complaints = await context.Complaints.Find(filter).SortByDescending(c => c.CreatedAt).ToListAsync();
        return complaints.Select(MapComplaint).ToList();
    }

    public async Task<ComplaintResponseDto?> GetComplaintByIdAsync(string id)
    {
        if (await context.Complaints.Find(c => c.Id == id).FirstOrDefaultAsync() is not { } complaint) return null;
        var dto = MapComplaint(complaint);
        dto.History = await GetComplaintHistoryAsync(id);
        return dto;
    }

    public async Task<ComplaintResponseDto> CreateComplaintAsync(string userId, CreateComplaintDto dto, string? imageUrl = null)
    {
        var user = await context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        var dept = await context.Departments.Find(d => d.Id == dto.DepartmentId).FirstOrDefaultAsync();

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
        await context.Complaints.InsertOneAsync(complaint);

        await context.ComplaintHistory.InsertOneAsync(new ComplaintHistory
        {
            ComplaintId = complaint.Id,
            UpdatedBy = complaint.UserName,
            PreviousStatus = "None",
            NewStatus = ComplaintStatuses.Pending,
            Remarks = "Complaint submitted by student.",
            CreatedAt = DateTime.UtcNow
        });

        await notificationService.CreateNotificationAsync(
            userId, "Complaint Submitted", $"Your complaint '{complaint.Title}' has been registered and is pending department review.");

        return MapComplaint(complaint);
    }

    public async Task<bool> UpdateComplaintAsync(string id, UpdateComplaintDto dto)
    {
        var complaint = await context.Complaints.Find(c => c.Id == id).FirstOrDefaultAsync();
        if (complaint == null) return false;

        var dept = await context.Departments.Find(d => d.Id == dto.DepartmentId).FirstOrDefaultAsync();
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

        return (await context.Complaints.UpdateOneAsync(c => c.Id == id, update)).ModifiedCount > 0;
    }

    public async Task<bool> UpdateComplaintStatusAsync(string id, string updatedBy, UpdateComplaintStatusDto dto)
    {
        var complaint = await context.Complaints.Find(c => c.Id == id).FirstOrDefaultAsync();
        if (complaint == null) return false;

        var update = Builders<Complaint>.Update.Set(c => c.Status, dto.Status).Set(c => c.UpdatedAt, DateTime.UtcNow);
        if (!string.IsNullOrWhiteSpace(dto.AssignedTo)) update = update.Set(c => c.AssignedTo, dto.AssignedTo);
        if (!string.IsNullOrWhiteSpace(dto.ResolutionNotes)) update = update.Set(c => c.ResolutionNotes, dto.ResolutionNotes);

        await context.Complaints.UpdateOneAsync(c => c.Id == id, update);

        await context.ComplaintHistory.InsertOneAsync(new ComplaintHistory
        {
            ComplaintId = id,
            UpdatedBy = updatedBy,
            PreviousStatus = complaint.Status,
            NewStatus = dto.Status,
            Remarks = string.IsNullOrWhiteSpace(dto.Remarks) ? $"Status updated to {dto.Status}" : dto.Remarks!,
            CreatedAt = DateTime.UtcNow
        });

        await notificationService.CreateNotificationAsync(
            complaint.UserId, "Complaint Status Update", $"Your complaint '{complaint.Title}' is now '{dto.Status}' by {updatedBy}.");

        return true;
    }

    public async Task<bool> DeleteComplaintAsync(string id, string currentUserId, bool isAdmin)
    {
        var complaint = await context.Complaints.Find(c => c.Id == id).FirstOrDefaultAsync();
        if (complaint == null || (!isAdmin && complaint.UserId != currentUserId)) return false;

        await context.ComplaintHistory.DeleteManyAsync(h => h.ComplaintId == id);
        return (await context.Complaints.DeleteOneAsync(c => c.Id == id)).DeletedCount > 0;
    }

    public async Task<List<ComplaintHistoryDto>> GetComplaintHistoryAsync(string complaintId) =>
        (await context.ComplaintHistory.Find(h => h.ComplaintId == complaintId).SortByDescending(h => h.CreatedAt).ToListAsync())
        .Select(h => new ComplaintHistoryDto
        {
            Id = h.Id, ComplaintId = h.ComplaintId, UpdatedBy = h.UpdatedBy,
            PreviousStatus = h.PreviousStatus, NewStatus = h.NewStatus, Remarks = h.Remarks, CreatedAt = h.CreatedAt
        }).ToList();

    private static ComplaintResponseDto MapComplaint(Complaint c) => new()
    {
        Id = c.Id, UserId = c.UserId, UserName = c.UserName, UserEmail = c.UserEmail,
        DepartmentId = c.DepartmentId ?? string.Empty, DepartmentName = c.DepartmentName, Title = c.Title,
        Category = c.Category, Description = c.Description, ImageUrl = c.ImageUrl,
        Status = c.Status, AssignedTo = c.AssignedTo, ResolutionNotes = c.ResolutionNotes,
        CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt
    };
}
