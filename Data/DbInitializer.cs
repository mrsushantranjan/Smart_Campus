using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using SmartCampus.Models;

namespace SmartCampus.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(MongoDbContext context)
    {
        try
        {
            await NormalizeFieldAsync(context.Database, "Users", "DepartmentId");
            await NormalizeFieldAsync(context.Database, "Complaints", "DepartmentId");
            await NormalizeFieldAsync(context.Database, "Complaints", "UserId");
            await NormalizeFieldAsync(context.Database, "ServiceRequests", "UserId");
            await NormalizeFieldAsync(context.Database, "Notifications", "UserId");
        }
        catch { }

        try { await context.CreateIndexesAsync(); } catch { }

        // 1. Seed Departments
        Department csDept;
        if (await context.Departments.CountDocumentsAsync(FilterDefinition<Department>.Empty) == 0)
        {
            csDept = new() { DepartmentName = "Computer Science & Engineering", Description = "Software engineering, algorithms, AI and cloud systems." };
            await context.Departments.InsertManyAsync([
                csDept,
                new() { DepartmentName = "Electrical & Electronics Engineering", Description = "Embedded hardware, VLSI, microcontrollers and IoT." },
                new() { DepartmentName = "Mechanical Engineering", Description = "Robotics, thermal systems, and CAD automation." },
                new() { DepartmentName = "Civil Engineering", Description = "Structural engineering, GIS, and smart infrastructure." },
                new() { DepartmentName = "Business Administration", Description = "Tech management, marketing, and data analytics." }
            ]);
        }
        else
        {
            csDept = await context.Departments.Find(d => d.DepartmentName.Contains("Computer")).FirstOrDefaultAsync()
                     ?? await context.Departments.Find(FilterDefinition<Department>.Empty).FirstAsync();
        }

        // 2. Seed Users
        ApplicationUser adminUser, facultyUser, studentUser;
        var hasher = new PasswordHasher<ApplicationUser>();
        if (await context.Users.CountDocumentsAsync(FilterDefinition<ApplicationUser>.Empty) == 0)
        {
            adminUser = new()
            {
                FullName = "System Administrator", Email = "admin@smartcampus.edu", Role = UserRoles.Admin,
                DepartmentId = csDept.Id, DepartmentName = csDept.DepartmentName, PhoneNumber = "+1-555-0100",
                IsActive = true, CreatedAt = DateTime.UtcNow
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");

            facultyUser = new()
            {
                FullName = "Dr. Priya Sharma", Email = "faculty@smartcampus.edu", Role = UserRoles.Faculty,
                DepartmentId = csDept.Id, DepartmentName = csDept.DepartmentName, PhoneNumber = "+1-555-0101",
                IsActive = true, CreatedAt = DateTime.UtcNow
            };
            facultyUser.PasswordHash = hasher.HashPassword(facultyUser, "Faculty@123");

            studentUser = new()
            {
                FullName = "Aarav Mehta", Email = "student@smartcampus.edu", Role = UserRoles.Student,
                DepartmentId = csDept.Id, DepartmentName = csDept.DepartmentName, StudentId = "STU-2026-0412",
                PhoneNumber = "+1-555-0102", IsActive = true, CreatedAt = DateTime.UtcNow
            };
            studentUser.PasswordHash = hasher.HashPassword(studentUser, "Student@123");

            await context.Users.InsertManyAsync([adminUser, facultyUser, studentUser]);
        }
        else
        {
            adminUser = await context.Users.Find(u => u.Email == "admin@smartcampus.edu").FirstOrDefaultAsync()
                        ?? await context.Users.Find(FilterDefinition<ApplicationUser>.Empty).FirstAsync();
            studentUser = await context.Users.Find(u => u.Role == UserRoles.Student).FirstOrDefaultAsync() ?? adminUser;
            facultyUser = await context.Users.Find(u => u.Role == UserRoles.Faculty).FirstOrDefaultAsync() ?? adminUser;
        }

        // 3. Seed Library Books
        if (await context.Books.CountDocumentsAsync(FilterDefinition<Book>.Empty) == 0)
        {
            await context.Books.InsertManyAsync([
                new() { Title = "Clean Architecture: A Craftsman's Guide", Author = "Robert C. Martin", ISBN = "978-0134494166", Category = "Software Engineering", Description = "Practical software structure rules and design patterns.", TotalCopies = 5, AvailableCopies = 5, CreatedAt = DateTime.UtcNow },
                new() { Title = "Introduction to Algorithms, 4th Edition", Author = "Thomas H. Cormen", ISBN = "978-0262046305", Category = "Computer Science", Description = "The standard reference work on modern computer algorithms.", TotalCopies = 8, AvailableCopies = 7, CreatedAt = DateTime.UtcNow },
                new() { Title = "Database System Concepts, 7th Edition", Author = "Abraham Silberschatz", ISBN = "978-0078022159", Category = "Databases", Description = "Relational and distributed database systems, SQL, and NoSQL engines.", TotalCopies = 6, AvailableCopies = 6, CreatedAt = DateTime.UtcNow },
                new() { Title = "Designing Data-Intensive Applications", Author = "Martin Kleppmann", ISBN = "978-1449373320", Category = "Systems", Description = "Big data, reliability, scalability, and maintainability in modern distributed systems.", TotalCopies = 4, AvailableCopies = 4, CreatedAt = DateTime.UtcNow },
                new() { Title = "Computer Networking: A Top-Down Approach", Author = "James Kurose & Keith Ross", ISBN = "978-0136681557", Category = "Networks", Description = "Network protocols, cloud infrastructure, and internet architecture.", TotalCopies = 5, AvailableCopies = 5, CreatedAt = DateTime.UtcNow },
                new() { Title = "Artificial Intelligence: A Modern Approach", Author = "Stuart Russell & Peter Norvig", ISBN = "978-0134610993", Category = "Artificial Intelligence", Description = "Comprehensive introduction to the theory and practice of AI.", TotalCopies = 6, AvailableCopies = 6, CreatedAt = DateTime.UtcNow }
            ]);
        }

        // 4. Seed Announcements
        if (await context.Announcements.CountDocumentsAsync(FilterDefinition<Announcement>.Empty) == 0)
        {
            await context.Announcements.InsertManyAsync([
                new() { Title = "Campus Network & Wi-Fi Maintenance Window", Description = "Core network equipment in Engineering Block B will undergo scheduled firmware upgrades this Sunday between 02:00 AM and 04:00 AM.", Category = "Maintenance", PublishedBy = "Campus IT Services", PublishedAt = DateTime.UtcNow.AddDays(-1), IsActive = true },
                new() { Title = "Central Library Extended Reading Room Hours", Description = "The Central Library study pods and reference sections will remain open until 11:30 PM throughout mid-term review weeks.", Category = "Academic", PublishedBy = "Chief Librarian", PublishedAt = DateTime.UtcNow.AddDays(-2), IsActive = true },
                new() { Title = "Semester 5 Capstone Project Registration Deadline", Description = "All student teams must finalize their faculty advisor approval and project synopsis submission by Friday 5:00 PM.", Category = "Advisory", PublishedBy = "Dean of Academics", PublishedAt = DateTime.UtcNow.AddDays(-3), IsActive = true }
            ]);
        }

        // 5. Seed Complaints and History
        if (await context.Complaints.CountDocumentsAsync(FilterDefinition<Complaint>.Empty) == 0)
        {
            var c1 = new Complaint
            {
                UserId = studentUser.Id, UserName = studentUser.FullName, UserEmail = studentUser.Email,
                DepartmentId = csDept.Id, DepartmentName = csDept.DepartmentName,
                Title = "Wi-Fi disconnection in 3rd Floor Computer Lab 304", Category = "Infrastructure",
                Description = "High packet drops and intermittent disconnections on eduroam while executing lab benchmarks.",
                Status = ComplaintStatuses.InProgress, AssignedTo = facultyUser.FullName,
                ResolutionNotes = "IT team dispatched to replace switch PoE injector.",
                CreatedAt = DateTime.UtcNow.AddDays(-3), UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };
            var c2 = new Complaint
            {
                UserId = studentUser.Id, UserName = studentUser.FullName, UserEmail = studentUser.Email,
                DepartmentId = csDept.Id, DepartmentName = csDept.DepartmentName,
                Title = "Projector HDMI Port No Signal in Seminar Hall 2", Category = "Audio/Visual",
                Description = "Display loses signal when connecting via standard HDMI cable during seminar presentations.",
                Status = ComplaintStatuses.Pending, CreatedAt = DateTime.UtcNow.AddHours(-6), UpdatedAt = DateTime.UtcNow.AddHours(-6)
            };
            var c3 = new Complaint
            {
                UserId = studentUser.Id, UserName = studentUser.FullName, UserEmail = studentUser.Email,
                DepartmentId = csDept.Id, DepartmentName = csDept.DepartmentName,
                Title = "Air Conditioning Unit Leaking in Quiet Study Pod", Category = "Maintenance",
                Description = "Condensation drip near desk 4 in Central Library reading wing.",
                Status = ComplaintStatuses.Resolved, AssignedTo = "Campus Operations",
                ResolutionNotes = "Drain line cleared and thermostat sensor recalibrated to 22°C.",
                CreatedAt = DateTime.UtcNow.AddDays(-5), UpdatedAt = DateTime.UtcNow.AddDays(-2)
            };
            await context.Complaints.InsertManyAsync([c1, c2, c3]);

            await context.ComplaintHistory.InsertManyAsync([
                new() { ComplaintId = c1.Id, UpdatedBy = "System", PreviousStatus = "None", NewStatus = ComplaintStatuses.Pending, Remarks = "Complaint logged by student.", CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new() { ComplaintId = c1.Id, UpdatedBy = adminUser.FullName, PreviousStatus = ComplaintStatuses.Pending, NewStatus = ComplaintStatuses.InProgress, Remarks = $"Assigned to {facultyUser.FullName} for technical inspection.", CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new() { ComplaintId = c3.Id, UpdatedBy = "Campus Operations", PreviousStatus = ComplaintStatuses.InProgress, NewStatus = ComplaintStatuses.Resolved, Remarks = "Drain line cleaned; cooling restored.", CreatedAt = DateTime.UtcNow.AddDays(-2) }
            ]);
        }

        // 6. Seed Service Requests
        if (await context.ServiceRequests.CountDocumentsAsync(FilterDefinition<ServiceRequest>.Empty) == 0)
        {
            await context.ServiceRequests.InsertManyAsync([
                new() { UserId = studentUser.Id, UserName = studentUser.FullName, UserEmail = studentUser.Email, UserRole = UserRoles.Student, RequestType = ServiceRequestTypes.BonafideCertificate, Title = "Bonafide Certificate for National Scholarship Portal", Description = "Need certified digital bonafide letter verifying active enrollment in Semester 5 CSE.", Status = ServiceRequestStatuses.Approved, AdminRemarks = "Approved and signed digital copy available at registrar desk.", CreatedAt = DateTime.UtcNow.AddDays(-4), UpdatedAt = DateTime.UtcNow.AddDays(-2) },
                new() { UserId = studentUser.Id, UserName = studentUser.FullName, UserEmail = studentUser.Email, UserRole = UserRoles.Student, RequestType = ServiceRequestTypes.IdCard, Title = "Replacement Smart RFID ID Card", Description = "Original RFID card chip damaged, door access scanner no longer reads RFID badge.", Status = ServiceRequestStatuses.Pending, CreatedAt = DateTime.UtcNow.AddHours(-12), UpdatedAt = DateTime.UtcNow.AddHours(-12) },
                new() { UserId = facultyUser.Id, UserName = facultyUser.FullName, UserEmail = facultyUser.Email, UserRole = UserRoles.Faculty, RequestType = ServiceRequestTypes.TechnicalSupport, Title = "Software License Provisioning for Cloud Computing Lab", Description = "Provisioning 40 workstation seats for Docker and Kubernetes lab curriculum.", Status = ServiceRequestStatuses.Approved, AdminRemarks = "Licenses deployed across Lab B nodes.", CreatedAt = DateTime.UtcNow.AddDays(-6), UpdatedAt = DateTime.UtcNow.AddDays(-3) }
            ]);
        }

        // 7. Seed Notifications
        if (await context.Notifications.CountDocumentsAsync(FilterDefinition<Notification>.Empty) == 0)
        {
            await context.Notifications.InsertManyAsync([
                new() { UserId = studentUser.Id, Title = "Service Request Approved", Message = "Your request for 'Bonafide Certificate' has been approved by the Registrar.", IsRead = false, CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new() { UserId = studentUser.Id, Title = "Complaint Status Update", Message = "Your Wi-Fi complaint #304 status changed to 'In Progress'.", IsRead = false, CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new() { UserId = studentUser.Id, Title = "New Announcement Published", Message = "Campus IT Services published: 'Campus Network Maintenance Window'.", IsRead = true, CreatedAt = DateTime.UtcNow.AddHours(-18) }
            ]);
        }
    }

    private static async Task NormalizeFieldAsync(IMongoDatabase db, string collectionName, string fieldName)
    {
        var collection = db.GetCollection<BsonDocument>(collectionName);
        var docs = await collection.Find(Builders<BsonDocument>.Filter.Type(fieldName, BsonType.ObjectId)).ToListAsync();
        foreach (var doc in docs)
        {
            await collection.UpdateOneAsync(
                Builders<BsonDocument>.Filter.Eq("_id", doc["_id"]),
                Builders<BsonDocument>.Update.Set(fieldName, doc[fieldName].ToString()));
        }
    }
}
