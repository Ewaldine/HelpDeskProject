using HelpDesk.Shared.DTOs;
using HelpDesk.Web.Models;
using HelpDesk.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Web.Controllers;

[Authorize]
public class NotificationsController : BaseController
{
    private readonly ITicketApiService _ticketApiService;

    public NotificationsController(ITicketApiService ticketApiService)
    {
        _ticketApiService = ticketApiService;
    }

    public async Task<IActionResult> Index()
    {
        var token = await GetAccessTokenAsync();
        var currentUser = await GetCurrentUserAsync(_ticketApiService, token);
        if (currentUser == null) return RedirectToAction("Index", "Home");

        var notes = await _ticketApiService.GetNotificationsAsync(currentUser.Id, token);

        var vm = new NotificationViewModel
        {
            Notifications = notes.OrderByDescending(n => n.CreatedAt).ToList()
        };

        // Optionally mark all as read once loaded
        // await _ticketApiService.MarkAllNotificationsReadAsync(currentUser.Id, token);

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllRead()
    {
        var token = await GetAccessTokenAsync();
        var currentUser = await GetCurrentUserAsync(_ticketApiService, token);
        if (currentUser == null) return RedirectToAction("Index", "Home");
        await _ticketApiService.MarkAllNotificationsReadAsync(currentUser.Id, token);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> ClearAll()
    {
        // For now ClearAll behaves the same as MarkAllRead (marks as read).
        var token = await GetAccessTokenAsync();
        var currentUser = await GetCurrentUserAsync(_ticketApiService, token);
        if (currentUser == null) return RedirectToAction("Index", "Home");

        await _ticketApiService.MarkAllNotificationsReadAsync(currentUser.Id, token);
        return RedirectToAction("Index");
    }
}
