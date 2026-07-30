using HelpDesk.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Web.Controllers;

[Authorize(Roles = "TeamLead,Admin")]
public class ReportsController : BaseController
{
    private readonly ITicketApiService _ticketApiService;
    private static readonly Guid TenantId = Guid.Parse("6fc7f192-a64a-49f4-82a6-2bc754631767");

    public ReportsController(ITicketApiService ticketApiService)
    {
        _ticketApiService = ticketApiService;
    }

    public async Task<IActionResult> Index()
    {
        var token = await GetAccessTokenAsync();
        var reports = await _ticketApiService.GetReportsAsync(TenantId, token);

        return View(reports ?? new HelpDesk.Shared.DTOs.ReportsResultDto());
    }
}