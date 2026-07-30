using HelpDesk.Shared.DTOs;
using HelpDesk.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Web.Controllers;

public class BaseController : Controller
{
    protected async Task<string> GetAccessTokenAsync()
    {
        return await HttpContext.GetTokenAsync("access_token") ?? string.Empty;
    }

    protected async Task<UserDto?> GetCurrentUserAsync(ITicketApiService ticketApiService, string token)
    {
        var keycloakId = User.FindFirst("sub")?.Value
            ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        if (string.IsNullOrEmpty(keycloakId)) return null;
        return await ticketApiService.GetCurrentUserAsync(keycloakId, token);
    }
}