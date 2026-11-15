namespace SmartCampus.Models;

public static class UserRoles
{
    public const string Admin = "Admin", Student = "Student", Faculty = "Faculty";
    public static readonly string[] All = [Admin, Student, Faculty];
}

public static class ComplaintCategories
{
    public const string Academic = "Academic", Infrastructure = "Infrastructure", Hostel = "Hostel",
        Maintenance = "Maintenance", ITSupport = "IT Support", Other = "Other";
    public static readonly string[] All = [Academic, Infrastructure, Hostel, Maintenance, ITSupport, Other];
}

public static class ComplaintStatuses
{
    public const string Pending = "Pending", Assigned = "Assigned", InProgress = "In Progress",
        Resolved = "Resolved", Rejected = "Rejected";
    public static readonly string[] All = [Pending, Assigned, InProgress, Resolved, Rejected];
}

public static class ServiceRequestTypes
{
    public const string IdCard = "ID Card", IDCard = IdCard, BonafideCertificate = "Bonafide Certificate",
        Hostel = "Hostel", Transportation = "Transportation", TechnicalSupport = "Technical Support", Other = "Other";
    public static readonly string[] All = [IdCard, BonafideCertificate, Hostel, Transportation, TechnicalSupport, Other];
}

public static class ServiceRequestStatuses
{
    public const string Pending = "Pending", Approved = "Approved", Rejected = "Rejected";
    public static readonly string[] All = [Pending, Approved, Rejected];
}

public static class ReservationStatuses
{
    public const string Active = "Active", Fulfilled = "Fulfilled", Returned = "Returned", Cancelled = "Cancelled";
    public static readonly string[] All = [Active, Fulfilled, Returned, Cancelled];
}
