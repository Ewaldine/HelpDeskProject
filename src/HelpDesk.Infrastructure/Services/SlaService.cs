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
        // Warning is sent when there is <= 1 hour remaining
        const double WarningWindowHours = 1.0;

        var openTickets = await _context.Tickets
            .Where(t => t.TenantId == tenantId
                && t.Status != TicketStatus.Resolved
                && t.Status != TicketStatus.Closed)
            .Include(t => t.AssignedTo)
            .ToListAsync();

        var changed = false;

        foreach (var ticket in openTickets)
        {
            var timeLeft = ticket.ResolutionDueAt - DateTime.UtcNow;

            // Send a single warning when within the warning window
            if (!ticket.SlaWarningSent && timeLeft.TotalHours <= WarningWindowHours && timeLeft.TotalSeconds > 0)
            {
                ticket.SlaWarningSent = true;
                changed = true;

                // create notification for assigned technician if they accept in-app notifications
                if (ticket.AssignedToId != null)
                {
                    var assigned = await _context.Users.FindAsync(ticket.AssignedToId.Value);
                    if (assigned != null && assigned.InAppNotificationsEnabled)
                    {
                        await _context.Notifications.AddAsync(new HelpDesk.Core.Entities.Notification
                        {
                            UserId = ticket.AssignedToId.Value,
                            TicketId = ticket.Id,
                            Title = "SLA Approaching",
                            Message = $"Ticket '{ticket.Title}' is at risk of SLA breach in {Math.Ceiling(timeLeft.TotalMinutes)} minutes.",
                            IsRead = false
                        });
                    }
                }
            }

            // Mark breached and bump priority when past due
            if (!ticket.IsSlaBreach && DateTime.UtcNow > ticket.ResolutionDueAt)
            {
                ticket.IsSlaBreach = true;
                ticket.SlaWarningSent = true; // ensure warning flag is set
                changed = true;

                // bump priority up by one step (but not beyond Critical)
                if (ticket.Priority < HelpDesk.Core.Enums.TicketPriority.Critical)
                {
                    ticket.Priority = ticket.Priority + 1;
                }

                // notify assigned technician
                if (ticket.AssignedToId != null)
                {
                    var assigned = await _context.Users.FindAsync(ticket.AssignedToId.Value);
                    if (assigned != null && assigned.InAppNotificationsEnabled)
                    {
                        await _context.Notifications.AddAsync(new HelpDesk.Core.Entities.Notification
                        {
                            UserId = ticket.AssignedToId.Value,
                            TicketId = ticket.Id,
                            Title = "SLA Breached",
                            Message = $"Ticket '{ticket.Title}' has breached its SLA and priority has been raised to {ticket.Priority}.",
                            IsRead = false
                        });
                    }
                }

                // notify team leads/admins as well
                var recipients = await _context.Users
                    .Where(u => u.TenantId == tenantId && (u.Role == HelpDesk.Core.Enums.UserRole.TeamLead || u.Role == HelpDesk.Core.Enums.UserRole.Admin) && u.IsActive)
                    .ToListAsync();

                foreach (var recipient in recipients)
                {
                    if (!recipient.InAppNotificationsEnabled) continue;

                    await _context.Notifications.AddAsync(new HelpDesk.Core.Entities.Notification
                    {
                        UserId = recipient.Id,
                        TicketId = ticket.Id,
                        Title = "SLA Breached",
                        Message = $"Ticket '{ticket.Title}' has breached its SLA and was escalated by the system.",
                        IsRead = false
                    });
                }
            }
        }

        if (changed)
        {
            await _context.SaveChangesAsync();
        }
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