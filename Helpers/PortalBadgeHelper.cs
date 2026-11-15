namespace SmartCampus.Helpers;

public static class PortalBadgeHelper
{
    public static string RenderBadge(string? status) => (status?.Trim().ToLowerInvariant()) switch
    {
        "pending" => $"<span class=\"badge bg-warning-subtle text-warning border border-warning-subtle\"><i class=\"bi bi-hourglass-split me-1\"></i>{status}</span>",
        "in progress" or "assigned" => $"<span class=\"badge bg-primary-subtle text-primary border border-primary-subtle\"><i class=\"bi bi-gear-fill me-1\"></i>{status}</span>",
        "resolved" or "approved" or "active" or "fulfilled" => $"<span class=\"badge bg-success-subtle text-success border border-success-subtle\"><i class=\"bi bi-check-circle-fill me-1\"></i>{status}</span>",
        "rejected" or "cancelled" => $"<span class=\"badge bg-danger-subtle text-danger border border-danger-subtle\"><i class=\"bi bi-x-circle-fill me-1\"></i>{status}</span>",
        "returned" => $"<span class=\"badge bg-secondary-subtle text-secondary border border-secondary-subtle\"><i class=\"bi bi-arrow-return-left me-1\"></i>{status}</span>",
        null or "" => "<span class=\"badge bg-secondary\">Unknown</span>",
        _ => $"<span class=\"badge bg-light text-dark border\">{status}</span>"
    };

    public static string ComplaintStatusClass(string s) => s switch { "Pending" => "badge-soft-warning", "Assigned" => "badge-soft-info", "In Progress" => "badge-soft-primary", "Resolved" => "badge-soft-success", "Rejected" => "badge-soft-danger", _ => "badge-soft-secondary" };
    public static string ServiceRequestStatusClass(string s) => s switch { "Pending" => "badge-soft-warning", "Approved" => "badge-soft-success", "Rejected" => "badge-soft-danger", _ => "badge-soft-secondary" };
    public static string ReservationStatusClass(string s) => s switch { "Active" => "badge-soft-success", "Fulfilled" or "Returned" => "badge-soft-secondary", "Cancelled" => "badge-soft-danger", _ => "badge-soft-secondary" };
    public static string RoleBadgeClass(string r) => r switch { "Admin" => "badge-soft-danger", "Faculty" => "badge-soft-primary", "Student" => "badge-soft-info", _ => "badge-soft-secondary" };
}
