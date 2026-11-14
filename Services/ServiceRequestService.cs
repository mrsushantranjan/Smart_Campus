using MongoDB.Driver;
using SmartCampus.Data;
using SmartCampus.DTOs;
using SmartCampus.Models;

namespace SmartCampus.Services;

public class ServiceRequestService : IServiceRequestService
{
    private readonly MongoDbContext _context;
    private readonly INotificationService _notificationService;

    public ServiceRequestService(MongoDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<List<ServiceRequestResponseDto>> GetServiceRequestsAsync(string? userId = null, string? status = null, string? requestType = null)
    {
        var builder = Builders<ServiceRequest>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            filter &= builder.Eq(s => s.UserId, userId);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filter &= builder.Eq(s => s.Status, status);
        }

        if (!string.IsNullOrWhiteSpace(requestType))
        {
            filter &= builder.Eq(s => s.RequestType, requestType);
        }

        var list = await _context.ServiceRequests.Find(filter)
            .SortByDescending(s => s.CreatedAt)
            .ToListAsync();

        return list.Select(MapServiceRequest).ToList();
    }

    public async Task<ServiceRequestResponseDto?> GetServiceRequestByIdAsync(string id)
    {
        var req = await _context.ServiceRequests.Find(s => s.Id == id).FirstOrDefaultAsync();
        return req == null ? null : MapServiceRequest(req);
    }

    public async Task<ServiceRequestResponseDto> CreateServiceRequestAsync(string userId, CreateServiceRequestDto dto)
    {
        var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();

        var entity = new ServiceRequest
        {
            UserId = userId,
            UserName = user?.FullName ?? "Unknown User",
            UserEmail = user?.Email ?? "",
            UserRole = user?.Role ?? UserRoles.Student,
            RequestType = dto.RequestType,
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Status = ServiceRequestStatuses.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.ServiceRequests.InsertOneAsync(entity);

        await _notificationService.CreateNotificationAsync(
            userId,
            "Service Request Submitted",
            $"Your service request for '{entity.RequestType}: {entity.Title}' has been submitted for administrative review."
        );

        return MapServiceRequest(entity);
    }

    public async Task<bool> UpdateServiceRequestStatusAsync(string id, string status, string? adminRemarks)
    {
        var req = await _context.ServiceRequests.Find(s => s.Id == id).FirstOrDefaultAsync();
        if (req == null) return false;

        var update = Builders<ServiceRequest>.Update
            .Set(s => s.Status, status)
            .Set(s => s.AdminRemarks, adminRemarks?.Trim())
            .Set(s => s.UpdatedAt, DateTime.UtcNow);

        var result = await _context.ServiceRequests.UpdateOneAsync(s => s.Id == id, update);

        if (result.ModifiedCount > 0)
        {
            await _notificationService.CreateNotificationAsync(
                req.UserId,
                $"Service Request {status}",
                $"Your service request '{req.Title}' has been {status.ToLower()}{(string.IsNullOrWhiteSpace(adminRemarks) ? "." : $": {adminRemarks}")}"
            );
        }

        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteServiceRequestAsync(string id, string currentUserId, bool isAdmin)
    {
        var req = await _context.ServiceRequests.Find(s => s.Id == id).FirstOrDefaultAsync();
        if (req == null) return false;

        if (!isAdmin && req.UserId != currentUserId)
        {
            return false;
        }

        var result = await _context.ServiceRequests.DeleteOneAsync(s => s.Id == id);
        return result.DeletedCount > 0;
    }

    private static ServiceRequestResponseDto MapServiceRequest(ServiceRequest s) => new()
    {
        Id = s.Id,
        UserId = s.UserId,
        UserName = s.UserName,
        UserEmail = s.UserEmail,
        UserRole = s.UserRole,
        RequestType = s.RequestType,
        Title = s.Title,
        Description = s.Description,
        Status = s.Status,
        AdminRemarks = s.AdminRemarks,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}

