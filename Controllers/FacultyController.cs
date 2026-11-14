using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.Models;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

[Authorize(Roles = UserRoles.Faculty)]
public class FacultyController : Controller
{
    private readonly IReportService _reportService;

    public FacultyController(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var data = await _reportService.GetFacultyDashboardDataAsync(userId);
        return View(data);
    }
}

