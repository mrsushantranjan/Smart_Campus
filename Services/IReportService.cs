using SmartCampus.DTOs;

namespace SmartCampus.Services;

public interface IReportService
{
    Task<AdminDashboardDto> GetAdminDashboardDataAsync();
    Task<StudentDashboardDto> GetStudentDashboardDataAsync(string userId);
    Task<FacultyDashboardDto> GetFacultyDashboardDataAsync(string userId);
    Task<ReportsViewModel> GetReportsDataAsync();
}

