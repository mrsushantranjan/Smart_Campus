using SmartCampus.DTOs;
using SmartCampus.Models;

namespace SmartCampus.Services;

public interface IAnnouncementService
{
    Task<List<AnnouncementResponseDto>> GetAnnouncementsAsync(bool activeOnly = false, string? search = null);
    Task<AnnouncementResponseDto?> GetAnnouncementByIdAsync(string id);
    Task<AnnouncementResponseDto> CreateAnnouncementAsync(string publishedBy, CreateAnnouncementDto dto);
    Task<bool> UpdateAnnouncementAsync(string id, UpdateAnnouncementDto dto);
    Task<bool> DeleteAnnouncementAsync(string id);
}

