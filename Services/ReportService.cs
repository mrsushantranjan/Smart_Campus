using MongoDB.Driver;
using SmartCampus.Data;
using SmartCampus.DTOs;
using SmartCampus.Models;

namespace SmartCampus.Services;

public class ReportService : IReportService
{
    private readonly MongoDbContext _context;
    private readonly IComplaintService _complaintService;
    private readonly ILibraryService _libraryService;
    private readonly IServiceRequestService _requestService;
    private readonly IAnnouncementService _announcementService;
    private readonly INotificationService _notificationService;

    public ReportService(
        MongoDbContext context,
        IComplaintService complaintService,
        ILibraryService libraryService,
        IServiceRequestService requestService,
        IAnnouncementService announcementService,
        INotificationService notificationService)
    {
        _context = context;
        _complaintService = complaintService;
        _libraryService = libraryService;
        _requestService = requestService;
        _announcementService = announcementService;
        _notificationService = notificationService;
    }

    public async Task<AdminDashboardDto> GetAdminDashboardDataAsync()
    {
        var totalUsers = await _context.Users.CountDocumentsAsync(FilterDefinition<ApplicationUser>.Empty);
        var totalStudents = await _context.Users.CountDocumentsAsync(u => u.Role == UserRoles.Student);
        var totalFaculty = await _context.Users.CountDocumentsAsync(u => u.Role == UserRoles.Faculty);

        var totalComplaints = await _context.Complaints.CountDocumentsAsync(FilterDefinition<Complaint>.Empty);
        var pendingComplaints = await _context.Complaints.CountDocumentsAsync(c => c.Status == ComplaintStatuses.Pending);
        var resolvedComplaints = await _context.Complaints.CountDocumentsAsync(c => c.Status == ComplaintStatuses.Resolved);

        var totalBooks = await _context.Books.CountDocumentsAsync(FilterDefinition<Book>.Empty);
        var activeReservations = await _context.Reservations.CountDocumentsAsync(r => r.Status == ReservationStatuses.Active);
        var totalServiceRequests = await _context.ServiceRequests.CountDocumentsAsync(FilterDefinition<ServiceRequest>.Empty);

        var recentComplaints = await _complaintService.GetComplaintsAsync();
        var recentRequests = await _requestService.GetServiceRequestsAsync();

        return new AdminDashboardDto
        {
            TotalUsers = totalUsers,
            TotalStudents = totalStudents,
            TotalFaculty = totalFaculty,
            TotalComplaints = totalComplaints,
            PendingComplaints = pendingComplaints,
            ResolvedComplaints = resolvedComplaints,
            TotalBooks = totalBooks,
            ActiveReservations = activeReservations,
            TotalServiceRequests = totalServiceRequests,
            RecentComplaints = recentComplaints.Take(5).ToList(),
            RecentRequests = recentRequests.Take(5).ToList()
        };
    }

    public async Task<StudentDashboardDto> GetStudentDashboardDataAsync(string userId)
    {
        var validId = !string.IsNullOrWhiteSpace(userId) && MongoDB.Bson.ObjectId.TryParse(userId, out _);
        var user = validId
            ? await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync() ?? new ApplicationUser()
            : new ApplicationUser();

        var totalComplaints = validId ? await _context.Complaints.CountDocumentsAsync(c => c.UserId == userId) : 0;
        var pendingComplaints = validId ? await _context.Complaints.CountDocumentsAsync(c => c.UserId == userId && c.Status == ComplaintStatuses.Pending) : 0;
        var resolvedComplaints = validId ? await _context.Complaints.CountDocumentsAsync(c => c.UserId == userId && c.Status == ComplaintStatuses.Resolved) : 0;
        var activeRequests = validId ? await _context.ServiceRequests.CountDocumentsAsync(s => s.UserId == userId && s.Status == ServiceRequestStatuses.Pending) : 0;

        var recentComplaints = validId ? await _complaintService.GetComplaintsAsync(userId: userId) : new List<ComplaintResponseDto>();
        var reservations = validId ? await _libraryService.GetReservationsAsync(userId: userId) : new List<ReservationResponseDto>();
        var announcements = await _announcementService.GetAnnouncementsAsync(activeOnly: true);
        var notifications = validId ? await _notificationService.GetUserNotificationsAsync(userId: userId, limit: 5) : new List<NotificationResponseDto>();

        return new StudentDashboardDto
        {
            User = user,
            TotalComplaints = totalComplaints,
            PendingComplaints = pendingComplaints,
            ResolvedComplaints = resolvedComplaints,
            ActiveServiceRequests = activeRequests,
            RecentComplaints = recentComplaints.Take(5).ToList(),
            MyReservations = reservations.Take(5).ToList(),
            LatestAnnouncements = announcements.Take(4).ToList(),
            Notifications = notifications
        };
    }

    public async Task<FacultyDashboardDto> GetFacultyDashboardDataAsync(string userId)
    {
        var validId = !string.IsNullOrWhiteSpace(userId) && MongoDB.Bson.ObjectId.TryParse(userId, out _);
        var user = validId
            ? await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync() ?? new ApplicationUser()
            : new ApplicationUser();

        var assignedComplaints = await _complaintService.GetComplaintsAsync(departmentId: user.DepartmentId);
        var myRequests = validId ? await _requestService.GetServiceRequestsAsync(userId: userId) : new List<ServiceRequestResponseDto>();
        var announcements = await _announcementService.GetAnnouncementsAsync(activeOnly: true);

        return new FacultyDashboardDto
        {
            User = user,
            AssignedComplaintsCount = assignedComplaints.Count(c => c.Status != ComplaintStatuses.Resolved && c.Status != ComplaintStatuses.Rejected),
            TotalServiceRequests = myRequests.Count,
            AssignedComplaints = assignedComplaints.Take(6).ToList(),
            MyRequests = myRequests.Take(5).ToList(),
            LatestAnnouncements = announcements.Take(4).ToList()
        };
    }

    public async Task<ReportsViewModel> GetReportsDataAsync()
    {
        var allComplaints = await _context.Complaints.Find(FilterDefinition<Complaint>.Empty).ToListAsync();
        var allRequests = await _context.ServiceRequests.Find(FilterDefinition<ServiceRequest>.Empty).ToListAsync();
        var allReservations = await _context.Reservations.Find(FilterDefinition<Reservation>.Empty).ToListAsync();

        var deptGroup = allComplaints
            .GroupBy(c => string.IsNullOrWhiteSpace(c.DepartmentName) ? "General" : c.DepartmentName)
            .Select(g => new DepartmentStatItem { DepartmentName = g.Key, Count = g.Count() })
            .OrderByDescending(d => d.Count)
            .ToList();

        var statusGroup = allComplaints
            .GroupBy(c => c.Status)
            .Select(g => new StatusStatItem { Status = g.Key, Count = g.Count() })
            .ToList();

        var now = DateTime.UtcNow;
        var monthlyGroup = allComplaints
            .Where(c => c.CreatedAt >= now.AddMonths(-6))
            .GroupBy(c => c.CreatedAt.ToString("MMM yyyy"))
            .Select(g => new MonthlyStatItem { Month = g.Key, Count = g.Count() })
            .ToList();

        var reqStatusGroup = allRequests
            .GroupBy(s => s.Status)
            .Select(g => new StatusStatItem { Status = g.Key, Count = g.Count() })
            .ToList();

        var resStatusGroup = allReservations
            .GroupBy(r => r.Status)
            .Select(g => new StatusStatItem { Status = g.Key, Count = g.Count() })
            .ToList();

        return new ReportsViewModel
        {
            DepartmentComplaints = deptGroup,
            ComplaintStatuses = statusGroup,
            MonthlyComplaints = monthlyGroup,
            ServiceRequestStatuses = reqStatusGroup,
            ReservationStatuses = resStatusGroup,
            TotalComplaints = allComplaints.Count,
            TotalRequests = allRequests.Count,
            TotalReservations = allReservations.Count
        };
    }
}

