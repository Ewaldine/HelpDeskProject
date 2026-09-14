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
        if (currentUser == null) return RedirectToAction("Login", "Account");

        var notifications = await _ticketApiService.GetNotificationsAsync(currentUser.Id, token);
        return View(notifications);
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllRead()
    {
        var token = await GetAccessTokenAsync();
        var currentUser = await GetCurrentUserAsync(_ticketApiService, token);
        if (currentUser != null)
            await _ticketApiService.MarkAllNotificationsReadAsync(currentUser.Id, token);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> ClearAll()
    {
        var token = await GetAccessTokenAsync();
        var currentUser = await GetCurrentUserAsync(_ticketApiService, token);
        if (currentUser != null)
            await _ticketApiService.ClearAllNotificationsAsync(currentUser.Id, token);
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> ViewTicket(Guid id, Guid ticketId)
    {
        var token = await GetAccessTokenAsync();
        var currentUser = await GetCurrentUserAsync(_ticketApiService, token);
        if (currentUser != null)
            await _ticketApiService.MarkNotificationReadAsync(currentUser.Id, id, token);
        return RedirectToAction("Details", "Tickets", new { id = ticketId });
    }

    [HttpGet]
    public async Task<IActionResult> UnreadCount()
    {
        var token = await GetAccessTokenAsync();
        var currentUser = await GetCurrentUserAsync(_ticketApiService, token);
        if (currentUser == null) return Json(0);
        var counts = await _ticketApiService.GetNotificationCountsAsync(currentUser.Id, token);
        return Json(counts.All);
    }

    [HttpPost]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var token = await GetAccessTokenAsync();
        var currentUser = await GetCurrentUserAsync(_ticketApiService, token);
        if (currentUser != null)
            await _ticketApiService.MarkNotificationReadAsync(currentUser.Id, id, token);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteOne(Guid id)
    {
        var token = await GetAccessTokenAsync();
        var currentUser = await GetCurrentUserAsync(_ticketApiService, token);
        if (currentUser != null)
            await _ticketApiService.DeleteNotificationAsync(currentUser.Id, id, token);
        return RedirectToAction("Index");
    }
}