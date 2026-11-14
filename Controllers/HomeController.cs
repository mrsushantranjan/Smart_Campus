using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.Models;

namespace SmartCampus.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            return role switch
            {
                UserRoles.Admin => RedirectToAction("Index", "Admin"),
                UserRoles.Faculty => RedirectToAction("Index", "Faculty"),
                _ => RedirectToAction("Index", "Student")
            };
        }

        return RedirectToAction("Login", "Account");
    }

    public IActionResult ApiDocs()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
