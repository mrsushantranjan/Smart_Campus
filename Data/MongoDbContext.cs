using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SmartCampus.Models;

namespace SmartCampus.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public MongoDbContext(IMongoDatabase database)
    {
        _database = database;
    }

    public IMongoDatabase Database => _database;

    public IMongoCollection<ApplicationUser> Users =>
        _database.GetCollection<ApplicationUser>("Users");

    public IMongoCollection<Department> Departments =>
        _database.GetCollection<Department>("Departments");

    public IMongoCollection<Complaint> Complaints =>
        _database.GetCollection<Complaint>("Complaints");

    public IMongoCollection<ComplaintHistory> ComplaintHistory =>
        _database.GetCollection<ComplaintHistory>("ComplaintHistory");

    public IMongoCollection<Book> Books =>
        _database.GetCollection<Book>("Books");

    public IMongoCollection<Reservation> Reservations =>
        _database.GetCollection<Reservation>("Reservations");

    public IMongoCollection<ServiceRequest> ServiceRequests =>
        _database.GetCollection<ServiceRequest>("ServiceRequests");

    public IMongoCollection<Announcement> Announcements =>
        _database.GetCollection<Announcement>("Announcements");

    public IMongoCollection<Notification> Notifications =>
        _database.GetCollection<Notification>("Notifications");

    public async Task CreateIndexesAsync()
    {
        // 1. User email unique index & role index
        var emailIndexKeys = Builders<ApplicationUser>.IndexKeys.Ascending(u => u.Email);
        var emailIndexOptions = new CreateIndexOptions { Unique = true };
        await Users.Indexes.CreateOneAsync(new CreateIndexModel<ApplicationUser>(emailIndexKeys, emailIndexOptions));

        var roleIndexKeys = Builders<ApplicationUser>.IndexKeys.Ascending(u => u.Role);
        await Users.Indexes.CreateOneAsync(new CreateIndexModel<ApplicationUser>(roleIndexKeys));

        // 2. Complaint status & userId indexes
        var complaintStatusKeys = Builders<Complaint>.IndexKeys.Ascending(c => c.Status);
        await Complaints.Indexes.CreateOneAsync(new CreateIndexModel<Complaint>(complaintStatusKeys));

        var complaintUserKeys = Builders<Complaint>.IndexKeys.Ascending(c => c.UserId);
        await Complaints.Indexes.CreateOneAsync(new CreateIndexModel<Complaint>(complaintUserKeys));

        // 3. Book title index
        var bookTitleKeys = Builders<Book>.IndexKeys.Ascending(b => b.Title);
        await Books.Indexes.CreateOneAsync(new CreateIndexModel<Book>(bookTitleKeys));

        // 4. Reservation userId index
        var reservationUserKeys = Builders<Reservation>.IndexKeys.Ascending(r => r.UserId);
        await Reservations.Indexes.CreateOneAsync(new CreateIndexModel<Reservation>(reservationUserKeys));

        // 5. Service request status index
        var serviceStatusKeys = Builders<ServiceRequest>.IndexKeys.Ascending(s => s.Status);
        await ServiceRequests.Indexes.CreateOneAsync(new CreateIndexModel<ServiceRequest>(serviceStatusKeys));
    }
}

