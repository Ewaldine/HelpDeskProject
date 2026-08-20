using HelpDesk.Web.Models;
using HelpDesk.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Web.Controllers;

[Authorize(Roles = "TeamLead,Admin")]
public class TeamController : BaseController
{
    private readonly ITicketApiService _ticketApiService;
    private static readonly Guid TenantId = Guid.Parse("6fc7f192-a64a-49f4-82a6-2bc754631767");

    public TeamController(ITicketApiService ticketApiService)
    {
        _ticketApiService = ticketApiService;
    }

    public async Task<IActionResult> Index()
    {
        var token = await GetAccessTokenAsync();
        var technicians = await _ticketApiService.GetTechniciansAsync(TenantId, token);
        var allTickets = await _ticketApiService.GetByTenantAsync(TenantId, token);

        var members = technicians.Select(tech =>
        {
            var techTickets = allTickets.Where(t => t.AssignedToId == tech.Id).ToList();
            var resolvedThisWeek = techTickets.Count(t =>
                t.Status == "Resolved" && t.CreatedAt >= DateTime.UtcNow.AddDays(-7));
            var openTickets = techTickets.Count(t => t.Status != "Resolved" && t.Status != "Closed");

            return new TeamMemberDetailViewModel
            {
                Id = tech.Id,
                FirstName = tech.FirstName,
                LastName = tech.LastName,
                Email = tech.Email,
                IsOnline = true, // placeholder — no real presence tracking built
                ResolvedThisWeek = resolvedThisWeek,
                AvgResolutionHours = 3.5, // placeholder until calculated from history
                OpenTickets = openTickets,
                Capacity = 10
            };
        }).ToList();

        var model = new TeamViewModel
        {
            ResolvedThisWeek = members.Sum(m => m.ResolvedThisWeek),
            TotalOpenTickets = members.Sum(m => m.OpenTickets),
            TeamAvgResolutionHours = members.Any() ? members.Average(m => m.AvgResolutionHours) : 0,
            Members = members
        };

        return View(model);
    }
}