# Smart Campus Service Portal
**C# ASP.NET Core MVC + MongoDB + REST API**

A complete, production-grade university management and campus service portal built using **C#**, **ASP.NET Core MVC**, **MongoDB.Driver**, and **REST Web APIs**.

---

## 1. Technology Stack

- **Backend Framework**: ASP.NET Core MVC + ASP.NET Core Web API on .NET 8/10
- **Database**: MongoDB (`smart_campus_db`) via official `MongoDB.Driver` (No Entity Framework Core, No relational SQL migrations)
- **Architecture**: Service Layer Architecture + Dependency Injection + Repository Pattern handles
- **Authentication**: ASP.NET Core Cookie Authentication with PBKDF2/cryptographic `PasswordHasher<ApplicationUser>` and Role-Based Access Control (RBAC)
- **API Documentation**: OpenAPI / Swagger UI via Swashbuckle
- **Frontend / UI**:
  - Linear, Stripe, and Notion inspired minimal SaaS dashboard
  - Bootstrap 5 + Bootstrap Icons + Inter Typography
  - Chart.js for departmental, monthly, and status analytics
  - Clean, divided, card-focused responsive layout

---

## 2. Default Seed Accounts & Credentials

The portal automatically checks MongoDB on startup and seeds departments, test accounts with secure password hashes, book catalog, sample complaints, service requests, and announcements.

| Role | Email | Password | Access Rights |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@smartcampus.edu` | `Admin@123` | Full administrative control, users, departments, complaints, library, reports, announcements |
| **Faculty** | `faculty@smartcampus.edu` | `Faculty@123` | Department complaints review, status updates, service requests, library access |
| **Student** | `student@smartcampus.edu` | `Student@123` | Lodge complaints with photo upload, reserve books, request certificates/ID cards, campus notices |

*Note: The login page includes a **One-Click Demo Fill** box to effortlessly log in as any role.*

---

## 3. Database Collections (`smart_campus_db`)

All persistence is handled natively by `MongoDB.Driver` using `MongoDbContext`:

1. `Users`: Full user profiles, role assignments, department references, student IDs, and hashed credentials.
2. `Departments`: Campus faculties and departments with unique names.
3. `Complaints`: Student grievances, category, department routing, image attachment URLs, and current statuses (`Pending`, `Assigned`, `In Progress`, `Resolved`, `Rejected`).
4. `ComplaintHistory`: Immutable audit log tracking each status transition, who updated it, timestamp, and staff remarks.
5. `Books`: Campus library book inventory tracking `TotalCopies` and `AvailableCopies`.
6. `Reservations`: Book checkout reservations with atomic inventory deduction and restoration.
7. `ServiceRequests`: Formal applications for Bonafide Certificates, ID Cards, Hostel, Transportation, and Tech Support.
8. `Announcements`: Broadcast circulars and campus notices with active/draft statuses.
9. `Notifications`: In-app alert messages pushed on complaint progress, approval decisions, and announcements.

### Native MongoDB Indexes
- `Users`: Unique index on `Email`, index on `Role`
- `Complaints`: Index on `Status`, index on `UserId`, index on `DepartmentId`
- `Books`: Index on `Title`, index on `Category`
- `Reservations`: Index on `UserId`, index on `BookId`
- `ServiceRequests`: Index on `Status`, index on `UserId`

---

## 4. Key REST API Endpoints

All endpoints support JSON serialization and standard HTTP status codes (`200 OK`, `201 Created`, `400 BadRequest`, `401 Unauthorized`, `403 Forbidden`, `404 NotFound`).

### Authentication (`/api/auth`)
- `POST /api/auth/register` — Register student or faculty account
- `POST /api/auth/login` — Authenticate and receive user details
- `GET /api/auth/me` — Authenticated user profile

### User Directory (`/api/users`)
- `GET /api/users` — List users with role filtering (Admin only)
- `GET /api/users/{id}` — Fetch user by MongoDB ObjectId

### Complaints (`/api/complaints`)
- `GET /api/complaints` — Filter complaints by status, department, and user
- `GET /api/complaints/{id}` — Fetch complaint details and resolution history
- `POST /api/complaints` — Submit new complaint (supports multipart image file upload)
- `PATCH /api/complaints/{id}/status` — Progress complaint status and assign faculty
- `DELETE /api/complaints/{id}` — Delete complaint document (Admin only)

### Library & Atomic Reservations (`/api/books`, `/api/reservations`)
- `GET /api/books` — Search books and check copy availability
- `GET /api/books/{id}` — Book details
- `POST /api/books` — Add book to catalog (Admin)
- `PUT /api/books/{id}` — Update book metadata and copies
- `DELETE /api/books/{id}` — Delete book from catalog
- `GET /api/reservations` — List active/fulfilled reservations
- `POST /api/reservations` — Atomically reserve a book copy (`FindOneAndUpdateAsync` on `AvailableCopies > 0`)
- `PATCH /api/reservations/{id}/cancel` — Cancel reservation and atomically increment available copies

### Service Requests (`/api/service-requests`)
- `GET /api/service-requests` — List service requests
- `POST /api/service-requests` — Submit request (ID Card, Certificate, Hostel, Transportation, Tech Support)
- `PATCH /api/service-requests/{id}/status` — Approve or reject with admin remarks

### Announcements (`/api/announcements`)
- `GET /api/announcements` — List published announcements
- `POST /api/announcements` — Publish campus announcement

### In-App Notifications (`/api/notifications`)
- `GET /api/notifications` — User notifications list
- `GET /api/notifications/unread-count` — Count of unread notifications for navbar badge
- `PATCH /api/notifications/{id}/read` — Mark notification read

---

## 5. Configuration (`appsettings.json`)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "smart_campus_db"
  }
}
```

---

## 6. How to Run

### Prerequisites
- .NET 8 or 10 SDK
- MongoDB Server running locally on `localhost:27017` (or MongoDB Atlas connection string configured in `appsettings.json`)

### Commands
```bash
# Build the project
dotnet build SmartCampus.csproj

# Run tests
dotnet test SmartCampus.Tests/SmartCampus.Tests.csproj

# Start the application
dotnet run --project SmartCampus.csproj
```

Open:
- **Web Portal**: `http://localhost:5000` (or `https://localhost:5001`)
- **Interactive Swagger Documentation**: `http://localhost:5000/swagger`
- **Developer API Reference**: `http://localhost:5000/Home/ApiDocs`
