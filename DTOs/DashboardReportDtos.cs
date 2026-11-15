using SmartCampus.Models;

namespace SmartCampus.DTOs;

public class AdminDashboardDto
{
    public long TotalUsers { get; set; }
    public long TotalStudents { get; set; }
    public long TotalFaculty { get; set; }
    public long TotalComplaints { get; set; }
    public long PendingComplaints { get; set; }
    public long ResolvedComplaints { get; set; }
    public long TotalBooks { get; set; }
    public long ActiveReservations { get; set; }
    public long TotalServiceRequests { get; set; }
    public List<ComplaintResponseDto> RecentComplaints { get; set; } = [];
    public List<ServiceRequestResponseDto> RecentRequests { get; set; } = [];
}

public class StudentDashboardDto
{
    public ApplicationUser User { get; set; } = null!;
    public long TotalComplaints { get; set; }
    public long PendingComplaints { get; set; }
    public long ResolvedComplaints { get; set; }
    public long ActiveServiceRequests { get; set; }
    public List<ComplaintResponseDto> RecentComplaints { get; set; } = [];
    public List<ReservationResponseDto> MyReservations { get; set; } = [];
    public List<AnnouncementResponseDto> LatestAnnouncements { get; set; } = [];
    public List<NotificationResponseDto> Notifications { get; set; } = [];
}

public class FacultyDashboardDto
{
    public ApplicationUser User { get; set; } = null!;
    public long AssignedComplaintsCount { get; set; }
    public long TotalServiceRequests { get; set; }
    public List<ComplaintResponseDto> AssignedComplaints { get; set; } = [];
    public List<ServiceRequestResponseDto> MyRequests { get; set; } = [];
    public List<AnnouncementResponseDto> LatestAnnouncements { get; set; } = [];
}

public class DepartmentStatItem
{
    public string DepartmentName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class StatusStatItem
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class MonthlyStatItem
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ReportsViewModel
{
    public List<DepartmentStatItem> DepartmentComplaints { get; set; } = [];
    public List<StatusStatItem> ComplaintStatuses { get; set; } = [];
    public List<MonthlyStatItem> MonthlyComplaints { get; set; } = [];
    public List<StatusStatItem> ServiceRequestStatuses { get; set; } = [];
    public List<StatusStatItem> ReservationStatuses { get; set; } = [];
    public long TotalComplaints { get; set; }
    public long TotalRequests { get; set; }
    public long TotalReservations { get; set; }
}
