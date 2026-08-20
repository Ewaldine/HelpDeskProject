using HelpDesk.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Web.Controllers;

[Authorize(Roles = "Admin")]
public class CategoriesController : BaseController
{
    private readonly ITicketApiService _ticketApiService;

    public CategoriesController(ITicketApiService ticketApiService)
    {
        _ticketApiService = ticketApiService;
    }

    public async Task<IActionResult> Index()
    {
        var token = await GetAccessTokenAsync();
        var categories = await _ticketApiService.GetCategoriesAsync(token);
        return View(categories);
    }
}