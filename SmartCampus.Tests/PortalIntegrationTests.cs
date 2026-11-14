using MongoDB.Bson;
using SmartCampus.DTOs;
using SmartCampus.Helpers;
using SmartCampus.Models;
using SmartCampus.Services;
using Xunit;

namespace SmartCampus.Tests;

public class SmartCampusUnitTests
{
    [Fact]
    public void PasswordHashing_CorrectlyHashesAndVerifiesPasswords()
    {
        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<ApplicationUser>();
        var user = new ApplicationUser
        {
            Email = "student@smartcampus.edu",
            FullName = "Sample Student",
            Role = UserRoles.Student
        };

        var plainPassword = "SecurePassword@123";
        var hash = hasher.HashPassword(user, plainPassword);

        Assert.NotNull(hash);
        Assert.NotEqual(plainPassword, hash);

        // Verification success
        var resultSuccess = hasher.VerifyHashedPassword(user, hash, plainPassword);
        Assert.Equal(Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success, resultSuccess);

        // Verification failure with wrong password
        var resultFail = hasher.VerifyHashedPassword(user, hash, "WrongPassword@999");
        Assert.Equal(Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed, resultFail);
    }

    [Fact]
    public void Constants_ContainRequiredRolesAndStatuses()
    {
        // Roles
        Assert.Contains(UserRoles.Admin, UserRoles.All);
        Assert.Contains(UserRoles.Student, UserRoles.All);
        Assert.Contains(UserRoles.Faculty, UserRoles.All);

        // Complaint Statuses
        Assert.Contains(ComplaintStatuses.Pending, ComplaintStatuses.All);
        Assert.Contains(ComplaintStatuses.Assigned, ComplaintStatuses.All);
        Assert.Contains(ComplaintStatuses.InProgress, ComplaintStatuses.All);
        Assert.Contains(ComplaintStatuses.Resolved, ComplaintStatuses.All);
        Assert.Contains(ComplaintStatuses.Rejected, ComplaintStatuses.All);

        // Service Request Types
        Assert.Contains(ServiceRequestTypes.IdCard, ServiceRequestTypes.All);
        Assert.Contains(ServiceRequestTypes.BonafideCertificate, ServiceRequestTypes.All);
        Assert.Contains(ServiceRequestTypes.Hostel, ServiceRequestTypes.All);
        Assert.Contains(ServiceRequestTypes.Transportation, ServiceRequestTypes.All);
        Assert.Contains(ServiceRequestTypes.TechnicalSupport, ServiceRequestTypes.All);
        Assert.Contains(ServiceRequestTypes.Other, ServiceRequestTypes.All);

        // Reservation Statuses
        Assert.Contains(ReservationStatuses.Active, ReservationStatuses.All);
        Assert.Contains(ReservationStatuses.Fulfilled, ReservationStatuses.All);
        Assert.Contains(ReservationStatuses.Cancelled, ReservationStatuses.All);
    }

    [Fact]
    public void PortalBadgeHelper_RendersCorrectStatusBadges()
    {
        var pendingBadge = PortalBadgeHelper.RenderBadge("Pending");
        Assert.Contains("bg-warning-subtle", pendingBadge);
        Assert.Contains("Pending", pendingBadge);

        var resolvedBadge = PortalBadgeHelper.RenderBadge("Resolved");
        Assert.Contains("bg-success-subtle", resolvedBadge);

        var rejectedBadge = PortalBadgeHelper.RenderBadge("Rejected");
        Assert.Contains("bg-danger-subtle", rejectedBadge);

        var unknownBadge = PortalBadgeHelper.RenderBadge("");
        Assert.Contains("Unknown", unknownBadge);
    }

    [Fact]
    public void MongoModels_GenerateValidBsonObjectIds()
    {
        var complaint = new Complaint
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            DepartmentId = ObjectId.GenerateNewId().ToString(),
            Title = "Network connection issue",
            Category = "IT Support",
            Description = "Wi-Fi is dropping frequently in library hall.",
            Status = ComplaintStatuses.Pending
        };

        Assert.True(ObjectId.TryParse(complaint.Id, out _));
        Assert.True(ObjectId.TryParse(complaint.UserId, out _));
        Assert.True(ObjectId.TryParse(complaint.DepartmentId, out _));
        Assert.Equal(ComplaintStatuses.Pending, complaint.Status);
    }

    [Fact]
    public void BookModel_EnforcesTotalAndAvailableCopies()
    {
        var book = new Book
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Title = "Introduction to Algorithms",
            Author = "Cormen et al.",
            ISBN = "978-0262033848",
            Category = "Computer Science",
            TotalCopies = 5,
            AvailableCopies = 5
        };

        Assert.True(book.AvailableCopies <= book.TotalCopies);
        Assert.Equal(5, book.AvailableCopies);

        // Simulate reservation decrement
        book.AvailableCopies--;
        Assert.Equal(4, book.AvailableCopies);
        Assert.True(book.AvailableCopies >= 0);
    }

    [Fact]
    public void StringOrObjectIdSerializer_DeserializesBothBsonTypes()
    {
        var rawBsonWithObjectId = new BsonDocument
        {
            { "_id", ObjectId.GenerateNewId() },
            { "FullName", "Test User" },
            { "Email", "test@campus.edu" },
            { "DepartmentId", ObjectId.GenerateNewId() }
        };

        var userWithObjId = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<ApplicationUser>(rawBsonWithObjectId);
        Assert.NotNull(userWithObjId.DepartmentId);
        Assert.True(ObjectId.TryParse(userWithObjId.DepartmentId, out _));

        var rawBsonWithString = new BsonDocument
        {
            { "_id", ObjectId.GenerateNewId() },
            { "FullName", "Test User 2" },
            { "Email", "test2@campus.edu" },
            { "DepartmentId", ObjectId.GenerateNewId().ToString() }
        };

        var userWithString = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<ApplicationUser>(rawBsonWithString);
        Assert.NotNull(userWithString.DepartmentId);
        Assert.True(ObjectId.TryParse(userWithString.DepartmentId, out _));

        var rawBsonWithNull = new BsonDocument
        {
            { "_id", ObjectId.GenerateNewId() },
            { "FullName", "Test User 3" },
            { "Email", "test3@campus.edu" },
            { "DepartmentId", BsonNull.Value }
        };

        var userWithNull = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<ApplicationUser>(rawBsonWithNull);
        Assert.Null(userWithNull.DepartmentId);
    }

    [Fact]
    public void ServiceRequest_DefaultsToPendingAndValidType()
    {
        var request = new ServiceRequest
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            RequestType = ServiceRequestTypes.BonafideCertificate,
            Title = "Need Bonafide for Passport Application",
            Description = "Applying for passport renewal",
            Status = ServiceRequestStatuses.Pending
        };

        Assert.Equal(ServiceRequestStatuses.Pending, request.Status);
        Assert.Equal(ServiceRequestTypes.BonafideCertificate, request.RequestType);
        Assert.Null(request.AdminRemarks);
    }
}
