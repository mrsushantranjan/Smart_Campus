using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Models;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginRequestDto());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequestDto model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _authService.LoginAsync(model.Email, model.Password);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Invalid login attempt.");
                return View(model);
            }

            var user = result.User!;
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role),
                new("DepartmentId", user.DepartmentId ?? ""),
                new("DepartmentName", user.DepartmentName ?? "")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProperties);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToRoleDashboard(user.Role);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LOGIN ERROR] {ex}");
            ModelState.AddModelError(string.Empty, $"Authentication error: {ex.Message}");
            return View(model);
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Register()
    {
        try
        {
            ViewBag.Departments = await _authService.GetDepartmentsAsync();
        }
        catch
        {
            ViewBag.Departments = new List<Department>();
        }
        return View(new RegisterRequestDto { Role = UserRoles.Student });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            try
            {
                ViewBag.Departments = await _authService.GetDepartmentsAsync();
            }
            catch
            {
                ViewBag.Departments = new List<Department>();
            }
            return View(model);
        }

        try
        {
            var result = await _authService.RegisterAsync(model);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Registration failed.");
                try { ViewBag.Departments = await _authService.GetDepartmentsAsync(); } catch { ViewBag.Departments = new List<Department>(); }
                return View(model);
            }

            TempData["SuccessMessage"] = "Registration successful! Please sign in with your credentials.";
            return RedirectToAction(nameof(Login));
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Unable to register account. Ensure MongoDB is running locally on port 27017.");
            ViewBag.Departments = new List<Department>();
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["SuccessMessage"] = "You have been logged out.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction(nameof(Login));

        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null) return NotFound();

        ViewBag.Departments = await _authService.GetDepartmentsAsync();
        ViewBag.DepartmentName = user.DepartmentName ?? "General / Unassigned";
        var model = new UpdateProfileDto
        {
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            StudentId = user.StudentId,
            DepartmentId = user.DepartmentId
        };
        ViewBag.User = user;

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(UpdateProfileDto model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction(nameof(Login));

        if (!ModelState.IsValid)
        {
            ViewBag.Departments = await _authService.GetDepartmentsAsync();
            var currentUser = await _authService.GetUserByIdAsync(userId);
            ViewBag.User = currentUser;
            ViewBag.DepartmentName = currentUser?.DepartmentName ?? "General / Unassigned";
            return View(model);
        }

        var success = await _authService.UpdateProfileAsync(userId, model);
        if (success)
        {
            TempData["SuccessMessage"] = "Profile updated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to update profile.";
        }

        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private IActionResult RedirectToRoleDashboard(string? role = null)
    {
        var userRole = role ?? User.FindFirstValue(ClaimTypes.Role);
        return userRole switch
        {
            UserRoles.Admin => RedirectToAction("Index", "Admin"),
            UserRoles.Faculty => RedirectToAction("Index", "Faculty"),
            _ => RedirectToAction("Index", "Student")
        };
    }
}

