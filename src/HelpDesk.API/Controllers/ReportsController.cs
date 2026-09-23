using HelpDesk.Infrastructure.Data;
using HelpDesk.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "TeamLead,Admin")]
public class ReportsController : ControllerBase
{
    private readonly HelpDeskDbContext _context;

    public ReportsController(HelpDeskDbContext context)
    {
        _context = context;
    }

    [HttpGet("tenant/{tenantId}")]
    public async Task<ActionResult<ReportsResultDto>> GetReports(Guid tenantId)
    {
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

        var recentTickets = await _context.Tickets
            .Where(t => t.TenantId == tenantId && t.CreatedAt >= sevenDaysAgo)
            .Select(t => new { t.CreatedAt, t.ResolvedAt })
            .ToListAsync();

        var weeklyActivity = Enumerable.Range(0, 7)
            .Select(offset => DateTime.UtcNow.Date.AddDays(-6 + offset))
            .Select(day => new DailyActivityDto
            {
                Day = day.ToString("ddd"),
                Created = recentTickets.Count(t => t.CreatedAt.Date == day),
                Resolved = recentTickets.Count(t => t.ResolvedAt.HasValue && t.ResolvedAt.Value.Date == day)
            }).ToList();

        var monthlyTickets = await _context.Tickets
            .Where(t => t.TenantId == tenantId && t.CreatedAt >= sixMonthsAgo)
            .Select(t => t.CreatedAt)
            .ToListAsync();

        var monthlyTrend = Enumerable.Range(0, 6)
            .Select(offset => DateTime.UtcNow.AddMonths(-5 + offset))
            .Select(month => new MonthlyTrendDto
            {
                Month = month.ToString("MMM"),
                Count = monthlyTickets.Count(t => t.Month == month.Month && t.Year == month.Year)
            }).ToList();

        var allTenantTickets = await _context.Tickets
            .Where(t => t.TenantId == tenantId)
            .Select(t => new { t.Status, t.CreatedAt, t.ResolvedAt, t.SatisfactionRating })
            .ToListAsync();

        var totalCount = allTenantTickets.Count;
        var resolvedCount = allTenantTickets.Count(t => t.Status == HelpDesk.Core.Enums.TicketStatus.Resolved || t.Status == HelpDesk.Core.Enums.TicketStatus.Closed);
        var resolutionRate = totalCount > 0 ? Math.Round((double)resolvedCount / totalCount * 100, 1) : 0;

        var resolvedWithTimes = allTenantTickets.Where(t => t.ResolvedAt.HasValue).ToList();
        var avgResponseHours = resolvedWithTimes.Any()
            ? Math.Round(resolvedWithTimes.Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours), 1)
            : 0;

        return Ok(new ReportsResultDto
        {
            WeeklyActivity = weeklyActivity,
            MonthlyTrend = monthlyTrend,
            ResolutionRate = resolutionRate,
            AvgResponseHours = avgResponseHours,
        });
    }
    [HttpGet("admin-dashboard/{tenantId}")]
    public async Task<ActionResult<AdminDashboardDto>> GetAdminDashboard(Guid tenantId)
    {
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

        var allTickets = await _context.Tickets
            .Where(t => t.TenantId == tenantId)
            .Select(t => new { t.Status, t.Priority, t.IsSlaBreach, t.CreatedAt, t.CategoryId })
            .ToListAsync();

        var totalOpen = allTickets.Count(t => t.Status != HelpDesk.Core.Enums.TicketStatus.Resolved && t.Status != HelpDesk.Core.Enums.TicketStatus.Closed);
        var slaBreachesWeek = allTickets.Count(t => t.IsSlaBreach && t.CreatedAt >= sevenDaysAgo);
        var totalResolved = allTickets.Count(t => t.Status == HelpDesk.Core.Enums.TicketStatus.Resolved || t.Status == HelpDesk.Core.Enums.TicketStatus.Closed);

        var activeTechnicians = await _context.Users
            .CountAsync(u => u.TenantId == tenantId && u.Role == HelpDesk.Core.Enums.UserRole.Technician && u.IsActive);

        var categoryCounts = await _context.Categories
            .Where(c => !c.IsDeleted)
            .Select(c => new CategoryCountDto
            {
                Category = c.Name,
                Count = c.Tickets.Count(t => t.TenantId == tenantId && !t.IsDeleted)
            })
            .ToListAsync();

        var totalForPriority = allTickets.Count;
        var priorityGroups = allTickets
            .GroupBy(t => t.Priority)
            .Select(g => new PriorityPercentDto
            {
                Priority = g.Key.ToString(),
                Percent = totalForPriority > 0 ? (int)Math.Round((double)g.Count() / totalForPriority * 100) : 0
            }).ToList();

        var technicianWorkload = await _context.Users
            .Where(u => u.TenantId == tenantId && u.Role == HelpDesk.Core.Enums.UserRole.Technician && u.IsActive)
            .Select(tech => new TechnicianWorkloadDto
            {
                Name = tech.FirstName + " " + tech.LastName,
                AssignedTickets = tech.AssignedTickets.Count(t => t.Status != HelpDesk.Core.Enums.TicketStatus.Resolved && t.Status != HelpDesk.Core.Enums.TicketStatus.Closed),
                ResolvedThisMonth = tech.AssignedTickets.Count(t =>
                    t.Status == HelpDesk.Core.Enums.TicketStatus.Resolved &&
                    t.ResolvedAt.HasValue &&
                    t.ResolvedAt.Value.Month == DateTime.UtcNow.Month &&
                    t.ResolvedAt.Value.Year == DateTime.UtcNow.Year)
            })
            .ToListAsync();

        return Ok(new AdminDashboardDto
        {
            TotalOpenTickets = totalOpen,
            SlaBreachesWeek = slaBreachesWeek,
            TotalResolved = totalResolved,
            ActiveTechnicians = activeTechnicians,
            TicketsByCategory = categoryCounts,
            TicketsByPriority = priorityGroups,
            TechnicianWorkload = technicianWorkload
        });
    }

    [HttpGet("teamlead-dashboard/{tenantId}")]
    public async Task<ActionResult<TeamLeadDashboardDto>> GetTeamLeadDashboard(Guid tenantId)
    {
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

        var allTickets = await _context.Tickets
            .Include(t => t.SubmittedBy)
            .Include(t => t.Category)
            .Where(t => t.TenantId == tenantId)
            .ToListAsync();

        var unassigned = allTickets
            .Where(t => t.AssignedToId == null && t.Status != HelpDesk.Core.Enums.TicketStatus.Resolved && t.Status != HelpDesk.Core.Enums.TicketStatus.Closed)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .Take(5)
            .Select(t => new UnassignedTicketDto
            {
                Id = t.Id,
                Title = t.Title,
                Priority = t.Priority.ToString(),
                Category = t.Category?.Name ?? "—",
                SubmittedBy = t.SubmittedBy != null ? $"{t.SubmittedBy.FirstName} {t.SubmittedBy.LastName}" : "—",
                CreatedAt = t.CreatedAt
            }).ToList();

        var teamOpenTickets = allTickets.Count(t => t.Status != HelpDesk.Core.Enums.TicketStatus.Resolved && t.Status != HelpDesk.Core.Enums.TicketStatus.Closed);
        var slaBreachesWeek = allTickets.Count(t => t.IsSlaBreach && t.CreatedAt >= sevenDaysAgo);

        var resolvedWithTimes = allTickets.Where(t => t.ResolvedAt.HasValue).ToList();
        var avgResolutionHours = resolvedWithTimes.Any()
            ? Math.Round(resolvedWithTimes.Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours), 1)
            : 0;

        var technicians = await _context.Users
            .Where(u => u.TenantId == tenantId && u.Role == HelpDesk.Core.Enums.UserRole.Technician && u.IsActive)
            .ToListAsync();

        var teamWorkload = technicians.Select(tech =>
        {
            var techTickets = allTickets.Where(t => t.AssignedToId == tech.Id).ToList();
            return new TeamMemberWorkloadDto
            {
                Name = $"{tech.FirstName} {tech.LastName}",
                ResolvedThisWeek = techTickets.Count(t => t.Status == HelpDesk.Core.Enums.TicketStatus.Resolved && t.ResolvedAt >= sevenDaysAgo),
                CurrentLoad = techTickets.Count(t => t.Status != HelpDesk.Core.Enums.TicketStatus.Resolved && t.Status != HelpDesk.Core.Enums.TicketStatus.Closed),
                Capacity = 10
            };
        }).ToList();

        var recentHistory = await _context.TicketHistories
            .Include(h => h.Ticket)
            .Where(h => h.Ticket.TenantId == tenantId)
            .OrderByDescending(h => h.CreatedAt)
            .Take(6)
            .Select(h => new TeamActivityDto
            {
                Message = $"{h.ChangedByName} {h.Action.ToLower()} ticket #{h.Ticket.Id.ToString().Substring(0, 8)}",
                CreatedAt = h.CreatedAt
            }).ToListAsync();

        return Ok(new TeamLeadDashboardDto
        {
            UnassignedCount = allTickets.Count(t => t.AssignedToId == null && t.Status != HelpDesk.Core.Enums.TicketStatus.Resolved && t.Status != HelpDesk.Core.Enums.TicketStatus.Closed),
            TeamOpenTickets = teamOpenTickets,
            SlaBreachesThisWeek = slaBreachesWeek,
            AvgResolutionHours = avgResolutionHours,
            UnassignedTickets = unassigned,
            TeamWorkload = teamWorkload,
            RecentActivity = recentHistory
        });
    }

    [HttpGet("export/{tenantId}")]
    public async Task<IActionResult> Export(Guid tenantId)
    {
        var tickets = await _context.Tickets
            .Include(t => t.SubmittedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.Category)
            .Where(t => t.TenantId == tenantId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Ticket,Title,Status,Priority,Category,SubmittedBy,AssignedTo,CreatedAt,ResolvedAt,SlaBreached");

        string Csv(string? s) => "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";

        foreach (var t in tickets)
        {
            var ticketRef = "TKT-" + t.Id.ToString().Substring(0, 8).ToUpper();
            sb.AppendLine(string.Join(",",
                Csv(ticketRef), Csv(t.Title), Csv(t.Status.ToString()), Csv(t.Priority.ToString()),
                Csv(t.Category?.Name),
                Csv(t.SubmittedBy != null ? $"{t.SubmittedBy.FirstName} {t.SubmittedBy.LastName}" : ""),
                Csv(t.AssignedTo != null ? $"{t.AssignedTo.FirstName} {t.AssignedTo.LastName}" : "Unassigned"),
                Csv(t.CreatedAt.ToString("u")), Csv(t.ResolvedAt?.ToString("u") ?? ""), Csv(t.IsSlaBreach.ToString())));
        }

        return File(System.Text.Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"helpdesk-report-{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}