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
            Department = "Information Technology",
            LastUpdated = DateTime.UtcNow
        };

        return View(model);
    }
}