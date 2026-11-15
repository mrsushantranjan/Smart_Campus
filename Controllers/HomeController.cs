using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.Models;

namespace SmartCampus.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => User.Identity?.IsAuthenticated == true
        ? User.FindFirstValue(ClaimTypes.Role) switch
        {
            UserRoles.Admin => RedirectToAction("Index", "Admin"),
            UserRoles.Faculty => RedirectToAction("Index", "Faculty"),
            _ => RedirectToAction("Index", "Student")
        }
        : RedirectToAction("Login", "Account");

    public IActionResult ApiDocs() => View();
    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
