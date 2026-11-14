using SmartCampus.Models;

namespace SmartCampus.Helpers;

public static class PortalBadgeHelper
{
    public static string RenderBadge(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return "<span class=\"badge bg-secondary\">Unknown</span>";

        var lower = status.Trim().ToLowerInvariant();

        if (lower == "pending")
            return $"<span class=\"badge bg-warning-subtle text-warning border border-warning-subtle\"><i class=\"bi bi-hourglass-split me-1\"></i>{status}</span>";

        if (lower == "in progress" || lower == "assigned")
            return $"<span class=\"badge bg-primary-subtle text-primary border border-primary-subtle\"><i class=\"bi bi-gear-fill me-1\"></i>{status}</span>";

        if (lower == "resolved" || lower == "approved" || lower == "active" || lower == "fulfilled")
            return $"<span class=\"badge bg-success-subtle text-success border border-success-subtle\"><i class=\"bi bi-check-circle-fill me-1\"></i>{status}</span>";

        if (lower == "rejected" || lower == "cancelled")
            return $"<span class=\"badge bg-danger-subtle text-danger border border-danger-subtle\"><i class=\"bi bi-x-circle-fill me-1\"></i>{status}</span>";

        if (lower == "returned")
            return $"<span class=\"badge bg-secondary-subtle text-secondary border border-secondary-subtle\"><i class=\"bi bi-arrow-return-left me-1\"></i>{status}</span>";

        return $"<span class=\"badge bg-light text-dark border\">{status}</span>";
    }

    public static string ComplaintStatusClass(string status) => status switch
    {
        ComplaintStatuses.Pending => "badge-soft-warning",
        ComplaintStatuses.Assigned => "badge-soft-info",
        ComplaintStatuses.InProgress => "badge-soft-primary",
        ComplaintStatuses.Resolved => "badge-soft-success",
        ComplaintStatuses.Rejected => "badge-soft-danger",
        _ => "badge-soft-secondary"
    };

    public static string ServiceRequestStatusClass(string status) => status switch
    {
        ServiceRequestStatuses.Pending => "badge-soft-warning",
        ServiceRequestStatuses.Approved => "badge-soft-success",
        ServiceRequestStatuses.Rejected => "badge-soft-danger",
        _ => "badge-soft-secondary"
    };

    public static string ReservationStatusClass(string status) => status switch
    {
        ReservationStatuses.Active => "badge-soft-success",
        ReservationStatuses.Fulfilled or ReservationStatuses.Returned => "badge-soft-secondary",
        ReservationStatuses.Cancelled => "badge-soft-danger",
        _ => "badge-soft-secondary"
    };

    public static string RoleBadgeClass(string role) => role switch
    {
        UserRoles.Admin => "badge-soft-danger",
        UserRoles.Faculty => "badge-soft-primary",
        UserRoles.Student => "badge-soft-info",
        _ => "badge-soft-secondary"
    };
}
