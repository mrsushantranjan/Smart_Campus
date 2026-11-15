using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.Models;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

[Authorize(Roles = UserRoles.Faculty)]
public class FacultyController(IReportService reportService) : Controller
{
    public async Task<IActionResult> Index() =>
        View(await reportService.GetFacultyDashboardDataAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
}
