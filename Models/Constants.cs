namespace SmartCampus.Models;

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Student = "Student";
    public const string Faculty = "Faculty";

    public static readonly string[] All = { Admin, Student, Faculty };
}

public static class ComplaintCategories
{
    public const string Academic = "Academic";
    public const string Infrastructure = "Infrastructure";
    public const string Hostel = "Hostel";
    public const string Maintenance = "Maintenance";
    public const string ITSupport = "IT Support";
    public const string Other = "Other";

    public static readonly string[] All = { Academic, Infrastructure, Hostel, Maintenance, ITSupport, Other };
}

public static class ComplaintStatuses
{
    public const string Pending = "Pending";
    public const string Assigned = "Assigned";
    public const string InProgress = "In Progress";
    public const string Resolved = "Resolved";
    public const string Rejected = "Rejected";

    public static readonly string[] All = { Pending, Assigned, InProgress, Resolved, Rejected };
}

public static class ServiceRequestTypes
{
    public const string IdCard = "ID Card";
    public const string IDCard = IdCard;
    public const string BonafideCertificate = "Bonafide Certificate";
    public const string Hostel = "Hostel";
    public const string Transportation = "Transportation";
    public const string TechnicalSupport = "Technical Support";
    public const string Other = "Other";

    public static readonly string[] All = { IdCard, BonafideCertificate, Hostel, Transportation, TechnicalSupport, Other };
}

public static class ServiceRequestStatuses
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";

    public static readonly string[] All = { Pending, Approved, Rejected };
}

public static class ReservationStatuses
{
    public const string Active = "Active";
    public const string Fulfilled = "Fulfilled";
    public const string Returned = "Returned";
    public const string Cancelled = "Cancelled";

    public static readonly string[] All = { Active, Fulfilled, Returned, Cancelled };
}
