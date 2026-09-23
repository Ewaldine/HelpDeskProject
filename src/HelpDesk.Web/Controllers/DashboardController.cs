using HelpDesk.Web.Models;
using HelpDesk.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Web.Controllers;

[Authorize]
public class DashboardController : BaseController
{
    private readonly ITicketApiService _ticketApiService;
    private static readonly Guid TenantId = Guid.Parse("E9DC1A56-E4FE-448F-93B8-8234B1379D2A");

    public DashboardController(ITicketApiService ticketApiService)
    {
        _ticketApiService = ticketApiService;
    }

    public async Task<IActionResult> Index()
    {
        var token = await GetAccessTokenAsync();

        if (User.IsInRole("Admin"))
        {
            var adminData = await _ticketApiService.GetAdminDashboardAsync(TenantId, token);

            var adminModel = new AdminDashboardViewModel
            {
                TotalOpenTickets = adminData?.TotalOpenTickets ?? 0,
                SlaBreachesWeek = adminData?.SlaBreachesWeek ?? 0,
                TotalResolved = adminData?.TotalResolved ?? 0,
                ActiveTechnicians = adminData?.ActiveTechnicians ?? 0,
                TicketsByCategory = adminData?.TicketsByCategory.Select(c => new CategoryCountViewModel
                {
                    Category = c.Category,
                    Count = c.Count
                }).ToList() ?? new List<CategoryCountViewModel>(),
                TicketsByPriority = adminData?.TicketsByPriority.Select(p => new PriorityPercentViewModel
                {
                    Priority = p.Priority,
                    Percent = p.Percent
                }).ToList() ?? new List<PriorityPercentViewModel>(),
                TechnicianWorkload = adminData?.TechnicianWorkload.Select(t => new TechnicianWorkloadViewModel
                {
                    Name = t.Name,
                    Initials = string.Concat(t.Name.Split(' ').Take(2).Select(n => n.FirstOrDefault())).ToUpper(),
                    AssignedTickets = t.AssignedTickets,
                    ResolvedThisMonth = t.ResolvedThisMonth,
                    AvgResolutionHours = 3.5 // placeholder — calculated separately if needed
                }).ToList() ?? new List<TechnicianWorkloadViewModel>()
            };
            return View("AdminDashboard", adminModel);
        }

        if (User.IsInRole("TeamLead"))
        {
            var tlData = await _ticketApiService.GetTeamLeadDashboardAsync(TenantId, token);

            var teamLeadModel = new TeamLeadDashboardViewModel
            {
                TeamLeadFirstName = User.Identity?.Name ?? "Team Lead",
                UnassignedCount = tlData?.UnassignedCount ?? 0,
                TeamOpenTickets = tlData?.TeamOpenTickets ?? 0,
                SlaBreachesThisWeek = tlData?.SlaBreachesThisWeek ?? 0,
                AvgResolutionHours = tlData?.AvgResolutionHours ?? 0,
                UnassignedTickets = tlData?.UnassignedTickets.Select(t =>
                {
                    var timeOpen = DateTime.UtcNow - t.CreatedAt;
                    var timeOpenText = timeOpen.TotalHours < 1
                        ? $"{(int)timeOpen.TotalMinutes} min ago"
                        : timeOpen.TotalHours < 24
                            ? $"{(int)timeOpen.TotalHours} hr{((int)timeOpen.TotalHours == 1 ? "" : "s")} ago"
                            : $"{(int)timeOpen.TotalDays} day{((int)timeOpen.TotalDays == 1 ? "" : "s")} ago";

                    return new UnassignedTicketRowViewModel
                    {
                        Id = t.Id,
                        TicketNumber = "TKT-" + t.Id.ToString().Substring(0, 8).ToUpper(),
                        Title = t.Title,
                        Priority = t.Priority,
                        Category = t.Category,
                        SubmittedBy = t.SubmittedBy,
                        TimeOpen = timeOpenText
                    };
                }).ToList() ?? new List<UnassignedTicketRowViewModel>(),
                TeamWorkload = tlData?.TeamWorkload.Select(m => new TeamMemberWorkloadViewModel
                {
                    Name = m.Name,
                    Initials = string.Concat(m.Name.Split(' ').Take(2).Select(n => n.FirstOrDefault())).ToUpper(),
                    IsOnline = true,
                    ResolvedThisWeek = m.ResolvedThisWeek,
                    CurrentLoad = m.CurrentLoad,
                    Capacity = m.Capacity
                }).ToList() ?? new List<TeamMemberWorkloadViewModel>(),
                RecentActivity = tlData?.RecentActivity.Select(a => new TeamActivityViewModel
                {
                    Message = a.Message,
                    TimeAgo = GetTimeAgo(a.CreatedAt),
                    IconType = "updated"
                }).ToList() ?? new List<TeamActivityViewModel>()
            };
            teamLeadModel.Technicians = await _ticketApiService.GetTechniciansAsync(TenantId, token);
            return View("TeamLeadDashboard", teamLeadModel);

        }

        if (User.IsInRole("Technician"))
        {
            var currentUser = await GetCurrentUserAsync(token);
            var allTicketsForTech = await _ticketApiService.GetByTenantAsync(TenantId, token);

            var myAssigned = currentUser != null
                ? allTicketsForTech.Where(t => t.AssignedToName == $"{currentUser.FirstName} {currentUser.LastName}").ToList()
                : new List<HelpDesk.Shared.DTOs.TicketDto>();

            var techModel = new TechnicianDashboardViewModel
            {
                AssignedTicketCount = myAssigned.Count(t => t.Status != "Resolved" && t.Status != "Closed"),
                ResolvedToday = myAssigned.Count(t => t.Status == "Resolved"),
                AvgResponseTime = "23 min",
                TicketQueue = myAssigned
                    .OrderByDescending(t => t.Priority)
                    .Select(t => new TechnicianTicketRowViewModel
                    {
                        Id = t.Id,
                        TicketNumber = "#" + t.Id.ToString().Substring(0, 8),
                        Title = t.Title,
                        Priority = t.Priority,
                        Status = t.Status,
                        IsOverdue = t.IsSlaBreach,
                        SlaCountdown = t.ResolutionDueAt > DateTime.UtcNow
                            ? $"{(t.ResolutionDueAt - DateTime.UtcNow).Hours}h {(t.ResolutionDueAt - DateTime.UtcNow).Minutes}m"
                            : "",
                        SubmittedBy = t.SubmittedByName ?? "—"
                    }).ToList()
            };
            return View("TechnicianDashboard", techModel);
        }

        var employeeCurrentUser = await GetCurrentUserAsync(token);
        Console.WriteLine($"[DEBUG] Current user ID: {employeeCurrentUser?.Id}");

        var allTickets = await _ticketApiService.GetByTenantAsync(TenantId, token);
        Console.WriteLine($"[DEBUG] Total tickets fetched: {allTickets.Count}");
        foreach (var t in allTickets)
        {
            Console.WriteLine($"[DEBUG]   Ticket: {t.Title}, SubmittedById: {t.SubmittedById}");
        }

        var myTickets = employeeCurrentUser != null
            ? allTickets.Where(t => t.SubmittedById == employeeCurrentUser.Id).ToList()
            : new List<HelpDesk.Shared.DTOs.TicketDto>();
        Console.WriteLine($"[DEBUG] My tickets count: {myTickets.Count}");

        var resolvedThisMonth = myTickets.Count(t =>
            t.Status == "Resolved" &&
            t.CreatedAt.Month == DateTime.UtcNow.Month &&
            t.CreatedAt.Year == DateTime.UtcNow.Year);

        var model = new EmployeeDashboardViewModel
        {
            OpenTicketCount = myTickets.Count(t => t.Status != "Resolved" && t.Status != "Closed"),
            ResolvedThisMonth = resolvedThisMonth,
            AvgResolutionHours = 4.2, // placeholder until we calculate this properly from history
            RecentTickets = myTickets
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .Select(t => new TicketRowViewModel
                {
                    Id = t.Id,
                    TicketNumber = "#" + t.Id.ToString().Substring(0, 8),
                    Title = t.Title,
                    Status = t.Status,
                    Priority = t.Priority,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.CreatedAt
                }).ToList()
        };
        return View("EmployeeDashboard", model);
    }

    private async Task<HelpDesk.Shared.DTOs.UserDto?> GetCurrentUserAsync(string token)
    {
        var keycloakId = User.FindFirst("sub")?.Value
           ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        if (string.IsNullOrEmpty(keycloakId)) return null;
        return await _ticketApiService.GetCurrentUserAsync(keycloakId, token);
    }

    private static string GetTimeAgo(DateTime dateTime)
    {
        var span = DateTime.UtcNow - dateTime;
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} min ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours} hr{((int)span.TotalHours == 1 ? "" : "s")} ago";
        return $"{(int)span.TotalDays} day{((int)span.TotalDays == 1 ? "" : "s")} ago";
    }
}

