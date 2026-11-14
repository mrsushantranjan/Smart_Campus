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
    public List<ComplaintResponseDto> RecentComplaints { get; set; } = new();
    public List<ServiceRequestResponseDto> RecentRequests { get; set; } = new();
}

public class StudentDashboardDto
{
    public ApplicationUser User { get; set; } = null!;
    public long TotalComplaints { get; set; }
    public long PendingComplaints { get; set; }
    public long ResolvedComplaints { get; set; }
    public long ActiveServiceRequests { get; set; }
    public List<ComplaintResponseDto> RecentComplaints { get; set; } = new();
    public List<ReservationResponseDto> MyReservations { get; set; } = new();
    public List<AnnouncementResponseDto> LatestAnnouncements { get; set; } = new();
    public List<NotificationResponseDto> Notifications { get; set; } = new();
}

public class FacultyDashboardDto
{
    public ApplicationUser User { get; set; } = null!;
    public long AssignedComplaintsCount { get; set; }
    public long TotalServiceRequests { get; set; }
    public List<ComplaintResponseDto> AssignedComplaints { get; set; } = new();
    public List<ServiceRequestResponseDto> MyRequests { get; set; } = new();
    public List<AnnouncementResponseDto> LatestAnnouncements { get; set; } = new();
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
    public List<DepartmentStatItem> DepartmentComplaints { get; set; } = new();
    public List<StatusStatItem> ComplaintStatuses { get; set; } = new();
    public List<MonthlyStatItem> MonthlyComplaints { get; set; } = new();
    public List<StatusStatItem> ServiceRequestStatuses { get; set; } = new();
    public List<StatusStatItem> ReservationStatuses { get; set; } = new();
    public long TotalComplaints { get; set; }
    public long TotalRequests { get; set; }
    public long TotalReservations { get; set; }
}

