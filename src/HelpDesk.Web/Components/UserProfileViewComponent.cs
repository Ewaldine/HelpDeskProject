using HelpDesk.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Web.Components;

public class UserProfileViewComponent : ViewComponent
{
    private readonly ITicketApiService _ticketApiService;

    public UserProfileViewComponent(ITicketApiService ticketApiService)
    {
        _ticketApiService = ticketApiService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (HttpContext.User?.Identity == null || !HttpContext.User.Identity.IsAuthenticated)
            return Content(string.Empty);

        var token = await HttpContext.GetTokenAsync("access_token") ?? string.Empty;
        var keycloakId = HttpContext.User.FindFirst("sub")?.Value
            ?? HttpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        HelpDesk.Shared.DTOs.UserDto? user = null;
        if (!string.IsNullOrEmpty(keycloakId))
            user = await _ticketApiService.GetCurrentUserAsync(keycloakId, token);

        return View(user);
    }
}