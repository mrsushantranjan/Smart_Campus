using Microsoft.AspNetCore.Authentication.Cookies;
using SmartCampus.Data;
using SmartCampus.Models;
using SmartCampus.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure MongoDB Settings & Context
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<MongoDbContext>();

// 2. Register Application Services (DI)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IComplaintService, ComplaintService>();
builder.Services.AddScoped<ILibraryService, LibraryService>();
builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<IReportService, ReportService>();

// 3. Configure Authentication & Authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// 4. MVC + API Controllers + Swagger
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Smart Campus Service Portal API",
        Version = "v1",
        Description = "ASP.NET Core Web API + MongoDB backend for Smart Campus Service Portal."
    });
});

var app = builder.Build();

// 5. Initialize MongoDB schema, indexes, and seed records
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    await DbInitializer.InitializeAsync(dbContext);
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "MongoDB initialization skipped or failed: {Message}", ex.Message);
}

// 6. Pipeline configuration
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Campus API v1");
    options.RoutePrefix = "swagger";
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
