using MongoDB.Bson;
using MongoDB.Driver;
using SmartCampus.Data;
using SmartCampus.DTOs;
using SmartCampus.Models;

namespace SmartCampus.Services;

public class ReportService(
    MongoDbContext context,
    IComplaintService complaintService,
    ILibraryService libraryService,
    IServiceRequestService requestService,
    IAnnouncementService announcementService,
    INotificationService notificationService) : IReportService
{
    public async Task<AdminDashboardDto> GetAdminDashboardDataAsync() => new()
    {
        TotalUsers = await context.Users.CountDocumentsAsync(FilterDefinition<ApplicationUser>.Empty),
        TotalStudents = await context.Users.CountDocumentsAsync(u => u.Role == UserRoles.Student),
        TotalFaculty = await context.Users.CountDocumentsAsync(u => u.Role == UserRoles.Faculty),
        TotalComplaints = await context.Complaints.CountDocumentsAsync(FilterDefinition<Complaint>.Empty),
        PendingComplaints = await context.Complaints.CountDocumentsAsync(c => c.Status == ComplaintStatuses.Pending),
        ResolvedComplaints = await context.Complaints.CountDocumentsAsync(c => c.Status == ComplaintStatuses.Resolved),
        TotalBooks = await context.Books.CountDocumentsAsync(FilterDefinition<Book>.Empty),
        ActiveReservations = await context.Reservations.CountDocumentsAsync(r => r.Status == ReservationStatuses.Active),
        TotalServiceRequests = await context.ServiceRequests.CountDocumentsAsync(FilterDefinition<ServiceRequest>.Empty),
        RecentComplaints = (await complaintService.GetComplaintsAsync()).Take(5).ToList(),
        RecentRequests = (await requestService.GetServiceRequestsAsync()).Take(5).ToList()
    };

    public async Task<StudentDashboardDto> GetStudentDashboardDataAsync(string userId)
    {
        var validId = !string.IsNullOrWhiteSpace(userId) && ObjectId.TryParse(userId, out _);
        var user = validId ? await context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync() ?? new() : new();

        return new StudentDashboardDto
        {
            User = user,
            TotalComplaints = validId ? await context.Complaints.CountDocumentsAsync(c => c.UserId == userId) : 0,
            PendingComplaints = validId ? await context.Complaints.CountDocumentsAsync(c => c.UserId == userId && c.Status == ComplaintStatuses.Pending) : 0,
            ResolvedComplaints = validId ? await context.Complaints.CountDocumentsAsync(c => c.UserId == userId && c.Status == ComplaintStatuses.Resolved) : 0,
            ActiveServiceRequests = validId ? await context.ServiceRequests.CountDocumentsAsync(s => s.UserId == userId && s.Status == ServiceRequestStatuses.Pending) : 0,
            RecentComplaints = validId ? (await complaintService.GetComplaintsAsync(userId: userId)).Take(5).ToList() : [],
            MyReservations = validId ? (await libraryService.GetReservationsAsync(userId: userId)).Take(5).ToList() : [],
            LatestAnnouncements = (await announcementService.GetAnnouncementsAsync(activeOnly: true)).Take(4).ToList(),
            Notifications = validId ? await notificationService.GetUserNotificationsAsync(userId: userId, limit: 5) : []
        };
    }

    public async Task<FacultyDashboardDto> GetFacultyDashboardDataAsync(string userId)
    {
        var validId = !string.IsNullOrWhiteSpace(userId) && ObjectId.TryParse(userId, out _);
        var user = validId ? await context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync() ?? new() : new();
        var assigned = await complaintService.GetComplaintsAsync(departmentId: user.DepartmentId);
        var myRequests = validId ? await requestService.GetServiceRequestsAsync(userId: userId) : [];

        return new FacultyDashboardDto
        {
            User = user,
            AssignedComplaintsCount = assigned.Count(c => c.Status != ComplaintStatuses.Resolved && c.Status != ComplaintStatuses.Rejected),
            TotalServiceRequests = myRequests.Count,
            AssignedComplaints = assigned.Take(6).ToList(),
            MyRequests = myRequests.Take(5).ToList(),
            LatestAnnouncements = (await announcementService.GetAnnouncementsAsync(activeOnly: true)).Take(4).ToList()
        };
    }

    public async Task<ReportsViewModel> GetReportsDataAsync()
    {
        var allComplaints = await context.Complaints.Find(FilterDefinition<Complaint>.Empty).ToListAsync();
        var allRequests = await context.ServiceRequests.Find(FilterDefinition<ServiceRequest>.Empty).ToListAsync();
        var allReservations = await context.Reservations.Find(FilterDefinition<Reservation>.Empty).ToListAsync();
        var now = DateTime.UtcNow;

        return new ReportsViewModel
        {
            DepartmentComplaints = allComplaints
                .GroupBy(c => string.IsNullOrWhiteSpace(c.DepartmentName) ? "General" : c.DepartmentName)
                .Select(g => new DepartmentStatItem { DepartmentName = g.Key, Count = g.Count() })
                .OrderByDescending(d => d.Count).ToList(),
            ComplaintStatuses = allComplaints.GroupBy(c => c.Status).Select(g => new StatusStatItem { Status = g.Key, Count = g.Count() }).ToList(),
            MonthlyComplaints = allComplaints
                .Where(c => c.CreatedAt >= now.AddMonths(-6))
                .GroupBy(c => c.CreatedAt.ToString("MMM yyyy"))
                .Select(g => new MonthlyStatItem { Month = g.Key, Count = g.Count() }).ToList(),
            ServiceRequestStatuses = allRequests.GroupBy(s => s.Status).Select(g => new StatusStatItem { Status = g.Key, Count = g.Count() }).ToList(),
            ReservationStatuses = allReservations.GroupBy(r => r.Status).Select(g => new StatusStatItem { Status = g.Key, Count = g.Count() }).ToList(),
            TotalComplaints = allComplaints.Count,
            TotalRequests = allRequests.Count,
            TotalReservations = allReservations.Count
        };
    }
}
