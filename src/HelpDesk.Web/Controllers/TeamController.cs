using HelpDesk.Web.Models;
using HelpDesk.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Web.Controllers;

[Authorize(Roles = "TeamLead,Admin")]
public class TeamController : BaseController
{
    private readonly ITicketApiService _ticketApiService;
    private static readonly Guid TenantId = Guid.Parse("E9DC1A56-E4FE-448F-93B8-8234B1379D2A");

    public TeamController(ITicketApiService ticketApiService)
    {
        _ticketApiService = ticketApiService;
    }

    public async Task<IActionResult> Index()
    {
        var token = await GetAccessTokenAsync();
        var technicians = await _ticketApiService.GetTechniciansAsync(TenantId, token);
        var currentUser = await GetCurrentUserAsync(_ticketApiService, token);

        var teamMembers = technicians.ToList();
        if (currentUser != null && !teamMembers.Any(t => t.Id == currentUser.Id))
        {
            teamMembers.Insert(0, currentUser);
        }
        var allTickets = await _ticketApiService.GetByTenantAsync(TenantId, token);

        var members = teamMembers.Select(tech =>
        {
            var techTickets = allTickets.Where(t => t.AssignedToId == tech.Id).ToList();
            var resolvedThisWeek = techTickets.Count(t =>
                t.Status == "Resolved" && t.ResolvedAt.HasValue && t.ResolvedAt.Value >= DateTime.UtcNow.AddDays(-7));
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