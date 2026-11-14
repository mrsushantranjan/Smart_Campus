using SmartCampus.DTOs;
using SmartCampus.Models;

namespace SmartCampus.Services;

public interface IComplaintService
{
    Task<List<ComplaintResponseDto>> GetComplaintsAsync(string? userId = null, string? status = null, string? departmentId = null, string? search = null);
    Task<ComplaintResponseDto?> GetComplaintByIdAsync(string id);
    Task<ComplaintResponseDto> CreateComplaintAsync(string userId, CreateComplaintDto dto, string? imageUrl = null);
    Task<bool> UpdateComplaintAsync(string id, UpdateComplaintDto dto);
    Task<bool> UpdateComplaintStatusAsync(string id, string updatedBy, UpdateComplaintStatusDto dto);
    Task<bool> DeleteComplaintAsync(string id, string currentUserId, bool isAdmin);
    Task<List<ComplaintHistoryDto>> GetComplaintHistoryAsync(string complaintId);
}

