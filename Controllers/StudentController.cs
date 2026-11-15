using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.Models;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

[Authorize(Roles = UserRoles.Student)]
public class StudentController(IReportService reportService) : Controller
{
    public async Task<IActionResult> Index() =>
        View(await reportService.GetStudentDashboardDataAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
}
