using HelpDesk.Shared.DTOs;
using HelpDesk.Web.Models;
using HelpDesk.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Web.Controllers;

[Authorize]
public class SettingsController : BaseController
{
    private readonly ITicketApiService _ticketApiService;

    public SettingsController(ITicketApiService ticketApiService)
    {
        _ticketApiService = ticketApiService;
    }

    public async Task<IActionResult> Index()
    {
        var token = await GetAccessTokenAsync();
        var currentUser = await GetCurrentUserAsync(_ticketApiService, token);

        if (currentUser == null)
            return RedirectToAction("Login", "Account");

        var model = new SettingsViewModel
        {
            UserId = currentUser.Id,
            FirstName = currentUser.FirstName,
            LastName = currentUser.LastName,
            Email = currentUser.Email,
            Role = currentUser.Role,
            ProfilePhotoUrl = currentUser.ProfilePhotoUrl,
            PhoneNumber = currentUser.PhoneNumber ?? "",
            Department = currentUser.Department ?? "",
            OfficeLocation = currentUser.OfficeLocation ?? "",
            LastUpdated = DateTime.UtcNow,
            EmailNotifications = currentUser.EmailNotifications,
            TicketStatusUpdates = currentUser.TicketStatusUpdates,
            NewComments = currentUser.NewCommentNotifications,
            WeeklySummary = currentUser.WeeklySummary
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile(string firstName, string lastName, string phoneNumber, string department, string officeLocation)
    {
        var token = await GetAccessTokenAsync();
        var currentUser = await GetCurrentUserAsync(_ticketApiService, token);

        if (currentUser != null)
        {
            await _ticketApiService.UpdateProfileAsync(currentUser.Id, new UpdateProfileDto
            {
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                Department = department,
                OfficeLocation = officeLocation
            }, token);
        }

        TempData["ProfileSaved"] = "true";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateNotifications(bool emailNotifications, bool ticketStatusUpdates, bool newComments, bool weeklySummary)
    {
        var token = await GetAccessTokenAsync();
        var currentUser = await GetCurrentUserAsync(_ticketApiService, token);

        if (currentUser != null)
        {
            await _ticketApiService.UpdateNotificationPreferencesAsync(currentUser.Id, new NotificationPreferencesDto
            {
                EmailNotifications = emailNotifications,
                TicketStatusUpdates = ticketStatusUpdates,
                NewCommentNotifications = newComments,
                WeeklySummary = weeklySummary
            }, token);
        }

        TempData["PreferencesSaved"] = "true";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> UploadPhoto(IFormFile photo)
    {
        var token = await GetAccessTokenAsync();
        var currentUser = await GetCurrentUserAsync(_ticketApiService, token);
        if (currentUser == null || photo == null || photo.Length == 0)
            return RedirectToAction("Index");

        var uploadsFolder = Path.Combine("wwwroot", "uploads", "avatars");
        Directory.CreateDirectory(uploadsFolder);

        var ext = Path.GetExtension(photo.FileName);
        var fileName = $"{currentUser.Id}{ext}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await photo.CopyToAsync(stream);
        }

        var photoUrl = $"/uploads/avatars/{fileName}";

        await _ticketApiService.UpdateProfileWithPhotoAsync(currentUser.Id, new HelpDesk.Shared.DTOs.UpdateProfileDto
        {
            FirstName = currentUser.FirstName,
            LastName = currentUser.LastName,
            PhoneNumber = currentUser.PhoneNumber,
            Department = currentUser.Department,
            OfficeLocation = currentUser.OfficeLocation,
            ProfilePhotoUrl = photoUrl
        }, token);

        return RedirectToAction("Index");
    }
}