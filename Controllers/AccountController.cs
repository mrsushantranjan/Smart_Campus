using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Models;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

public class AccountController(IAuthService authService) : Controller
{
    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginRequestDto());
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequestDto model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var res = await authService.LoginAsync(model.Email, model.Password);
            if (!res.Succeeded)
            {
                ModelState.AddModelError(string.Empty, res.Error ?? "Invalid login attempt.");
                return View(model);
            }

            var u = res.User!;
            List<Claim> claims = [
                new(ClaimTypes.NameIdentifier, u.Id),
                new(ClaimTypes.Name, u.FullName),
                new(ClaimTypes.Email, u.Email),
                new(ClaimTypes.Role, u.Role),
                new("DepartmentId", u.DepartmentId ?? ""),
                new("DepartmentName", u.DepartmentName ?? "")
            ];

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = model.RememberMe, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) });

            return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToRoleDashboard(u.Role);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Authentication error: {ex.Message}");
            return View(model);
        }
    }

    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> Register()
    {
        ViewBag.Departments = await SafeGetDepartmentsAsync();
        return View(new RegisterRequestDto { Role = UserRoles.Student });
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequestDto model)
    {
        if (!ModelState.IsValid) { ViewBag.Departments = await SafeGetDepartmentsAsync(); return View(model); }

        try
        {
            var res = await authService.RegisterAsync(model);
            if (!res.Succeeded)
            {
                ModelState.AddModelError(string.Empty, res.Error ?? "Registration failed.");
                ViewBag.Departments = await SafeGetDepartmentsAsync();
                return View(model);
            }

            TempData["SuccessMessage"] = "Registration successful! Please sign in with your credentials.";
            return RedirectToAction(nameof(Login));
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Unable to register account. Ensure MongoDB is running.");
            ViewBag.Departments = await SafeGetDepartmentsAsync();
            return View(model);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["SuccessMessage"] = "You have been logged out.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet, Authorize]
    public async Task<IActionResult> Profile()
    {
        if (CurrentUserId is not { } uid) return RedirectToAction(nameof(Login));
        if (await authService.GetUserByIdAsync(uid) is not { } u) return NotFound();

        ViewBag.Departments = await authService.GetDepartmentsAsync();
        ViewBag.DepartmentName = u.DepartmentName ?? "General / Unassigned";
        ViewBag.User = u;
        return View(new UpdateProfileDto { FullName = u.FullName, PhoneNumber = u.PhoneNumber, StudentId = u.StudentId, DepartmentId = u.DepartmentId });
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(UpdateProfileDto model)
    {
        if (CurrentUserId is not { } uid) return RedirectToAction(nameof(Login));

        if (!ModelState.IsValid)
        {
            var curr = await authService.GetUserByIdAsync(uid);
            ViewBag.Departments = await authService.GetDepartmentsAsync();
            ViewBag.User = curr;
            ViewBag.DepartmentName = curr?.DepartmentName ?? "General / Unassigned";
            return View(model);
        }

        var ok = await authService.UpdateProfileAsync(uid, model);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Profile updated successfully." : "Failed to update profile.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    private async Task<List<Department>> SafeGetDepartmentsAsync()
    {
        try { return await authService.GetDepartmentsAsync(); } catch { return []; }
    }

    private IActionResult RedirectToRoleDashboard(string? role = null) => (role ?? User.FindFirstValue(ClaimTypes.Role)) switch
    {
        UserRoles.Admin => RedirectToAction("Index", "Admin"),
        UserRoles.Faculty => RedirectToAction("Index", "Faculty"),
        _ => RedirectToAction("Index", "Student")
    };
}
