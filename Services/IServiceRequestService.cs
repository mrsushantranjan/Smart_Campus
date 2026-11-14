using SmartCampus.DTOs;
using SmartCampus.Models;

namespace SmartCampus.Services;

public interface IServiceRequestService
{
    Task<List<ServiceRequestResponseDto>> GetServiceRequestsAsync(string? userId = null, string? status = null, string? requestType = null);
    Task<ServiceRequestResponseDto?> GetServiceRequestByIdAsync(string id);
    Task<ServiceRequestResponseDto> CreateServiceRequestAsync(string userId, CreateServiceRequestDto dto);
    Task<bool> UpdateServiceRequestStatusAsync(string id, string status, string? adminRemarks);
    Task<bool> DeleteServiceRequestAsync(string id, string currentUserId, bool isAdmin);
}

