using HelpDesk.Core.Entities;
using HelpDesk.Core.Enums;
using HelpDesk.Core.Interfaces.Repositories;
using HelpDesk.Core.Interfaces.Services;
using HelpDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Services;

public class SlaService : ISlaService
{
    private readonly HelpDeskDbContext _context;
    private readonly ITicketRepository _ticketRepository;

    public SlaService(HelpDeskDbContext context, ITicketRepository ticketRepository)
    {
        _context = context;
        _ticketRepository = ticketRepository;
    }

    public async Task<DateTime> CalculateResponseDueAsync(TicketPriority priority, Guid tenantId)
    {
        var policy = await GetPolicyAsync(priority, tenantId);
        return DateTime.UtcNow.AddHours(policy.ResponseTimeHours);
    }

    public async Task<DateTime> CalculateResolutionDueAsync(TicketPriority priority, Guid tenantId)
    {
        var policy = await GetPolicyAsync(priority, tenantId);
        return DateTime.UtcNow.AddHours(policy.ResolutionTimeHours);
    }

    public async Task CheckAndUpdateSlaStatusAsync(Guid tenantId)
    {
        var openTickets = await _context.Tickets
            .Where(t => t.TenantId == tenantId
                && t.Status != TicketStatus.Resolved
                && t.Status != TicketStatus.Closed
                && !t.IsSlaBreach)
            .ToListAsync();

        var breachedCount = 0;

        foreach (var ticket in openTickets)
        {
            var minutesLeft = (ticket.ResolutionDueAt - DateTime.UtcNow).TotalMinutes;

            if (minutesLeft <= 0)
            {
                ticket.IsSlaBreach = true;
                breachedCount++;
            }
            else if (minutesLeft <= 30 && !ticket.SlaWarningSent)
            {
                ticket.SlaWarningSent = true;
                var ticketRef = $"TKT-{ticket.Id.ToString().Substring(0, 8).ToUpper()}";
                var recipientIds = new List<Guid>();
                if (ticket.AssignedToId.HasValue) recipientIds.Add(ticket.AssignedToId.Value);

                var leads = await _context.Users
                    .Where(u => u.TenantId == tenantId && (u.Role == UserRole.TeamLead || u.Role == UserRole.Admin) && u.IsActive)
                    .Select(u => u.Id)
                    .ToListAsync();
                recipientIds.AddRange(leads);

                foreach (var recipientId in recipientIds.Distinct())
                {
                    await _context.Notifications.AddAsync(new Notification
                    {
                        UserId = recipientId,
                        TicketId = ticket.Id,
                        Type = NotificationType.Ticket,
                        IconType = "warning",
                        Title = "SLA breach warning",
                        Message = $"Ticket {ticketRef} \"{ticket.Title}\" is approaching its SLA deadline in {(int)minutesLeft} minutes."
                    });
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task EscalateBreachedTicketsAsync(Guid tenantId)
    {
        // Manual escalation is no longer used; SLA handling now increases priority and notifies.
        await Task.CompletedTask;
    }

    private async Task<SlaPolicy> GetPolicyAsync(TicketPriority priority, Guid tenantId)
    {
        return await _context.SlaPolicies
            .FirstOrDefaultAsync(p => p.Priority == priority && p.TenantId == tenantId)
            ?? throw new InvalidOperationException($"No SLA policy found for priority {priority}");
    }
}