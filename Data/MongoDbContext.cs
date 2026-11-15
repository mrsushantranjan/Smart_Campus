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

    public MongoDbContext(IMongoDatabase database) => _database = database;

    public IMongoDatabase Database => _database;
    public IMongoCollection<ApplicationUser> Users => _database.GetCollection<ApplicationUser>("Users");
    public IMongoCollection<Department> Departments => _database.GetCollection<Department>("Departments");
    public IMongoCollection<Complaint> Complaints => _database.GetCollection<Complaint>("Complaints");
    public IMongoCollection<ComplaintHistory> ComplaintHistory => _database.GetCollection<ComplaintHistory>("ComplaintHistory");
    public IMongoCollection<Book> Books => _database.GetCollection<Book>("Books");
    public IMongoCollection<Reservation> Reservations => _database.GetCollection<Reservation>("Reservations");
    public IMongoCollection<ServiceRequest> ServiceRequests => _database.GetCollection<ServiceRequest>("ServiceRequests");
    public IMongoCollection<Announcement> Announcements => _database.GetCollection<Announcement>("Announcements");
    public IMongoCollection<Notification> Notifications => _database.GetCollection<Notification>("Notifications");

    public async Task CreateIndexesAsync()
    {
        await Users.Indexes.CreateManyAsync([
            new CreateIndexModel<ApplicationUser>(Builders<ApplicationUser>.IndexKeys.Ascending(u => u.Email), new() { Unique = true }),
            new CreateIndexModel<ApplicationUser>(Builders<ApplicationUser>.IndexKeys.Ascending(u => u.Role))
        ]);
        await Complaints.Indexes.CreateManyAsync([
            new CreateIndexModel<Complaint>(Builders<Complaint>.IndexKeys.Ascending(c => c.Status)),
            new CreateIndexModel<Complaint>(Builders<Complaint>.IndexKeys.Ascending(c => c.UserId))
        ]);
        await Books.Indexes.CreateOneAsync(new CreateIndexModel<Book>(Builders<Book>.IndexKeys.Ascending(b => b.Title)));
        await Reservations.Indexes.CreateOneAsync(new CreateIndexModel<Reservation>(Builders<Reservation>.IndexKeys.Ascending(r => r.UserId)));
        await ServiceRequests.Indexes.CreateOneAsync(new CreateIndexModel<ServiceRequest>(Builders<ServiceRequest>.IndexKeys.Ascending(s => s.Status)));
    }
}
