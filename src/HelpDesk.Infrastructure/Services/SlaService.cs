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
        var now = DateTime.UtcNow;

        // --- Resolution SLA: warning + breach/escalation ---
        var openTickets = await _context.Tickets
            .Where(t => t.TenantId == tenantId
                && t.Status != TicketStatus.Resolved
                && t.Status != TicketStatus.Closed
                && t.Status != TicketStatus.Escalated)
            .ToListAsync();

        foreach (var ticket in openTickets)
        {
            var minutesLeft = (ticket.ResolutionDueAt - now).TotalMinutes;

            if (minutesLeft <= 0 && !ticket.IsSlaBreach)
            {
                var wasAlreadyCritical = ticket.Priority == TicketPriority.Critical;
                var oldPriority = ticket.Priority;

                ticket.IsSlaBreach = true;
                ticket.Status = TicketStatus.Escalated;

                if (!wasAlreadyCritical)
                    ticket.Priority = ticket.Priority + 1;

                await _context.TicketHistories.AddAsync(new TicketHistory
                {
                    TicketId = ticket.Id,
                    Action = "AutoEscalated",
                    OldValue = oldPriority.ToString(),
                    NewValue = ticket.Priority.ToString(),
                    ChangedByName = "SLA Engine (automatic)",
                    ChangedById = ticket.AssignedToId ?? ticket.SubmittedById
                });

                var ticketRef = $"TKT-{ticket.Id.ToString().Substring(0, 8).ToUpper()}";
                var title = wasAlreadyCritical ? "URGENT: Critical ticket breached SLA" : "SLA breached — ticket auto-escalated";
                var message = wasAlreadyCritical
                    ? $"{ticketRef} \"{ticket.Title}\" is Critical priority and has breached its SLA. It cannot be escalated further — immediate action required."
                    : $"{ticketRef} \"{ticket.Title}\" breached its SLA and was automatically escalated to {ticket.Priority}.";

                foreach (var recipientId in await GetLeadRecipientsAsync(tenantId, ticket))
                {
                    await _context.Notifications.AddAsync(new Notification
                    {
                        UserId = recipientId,
                        TicketId = ticket.Id,
                        Type = NotificationType.Ticket,
                        IconType = "escalated",
                        Title = title,
                        Message = message
                    });
                }
            }
            else if (minutesLeft > 0 && minutesLeft <= 15 && !ticket.SlaWarningSent)
            {
                ticket.SlaWarningSent = true;
                var ticketRef = $"TKT-{ticket.Id.ToString().Substring(0, 8).ToUpper()}";

                foreach (var recipientId in await GetLeadRecipientsAsync(tenantId, ticket))
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

        // --- Assignment SLA: tickets sitting unassigned too long ---
        var unassignedTickets = await _context.Tickets
            .Where(t => t.TenantId == tenantId
                && t.AssignedToId == null
                && t.Status != TicketStatus.Resolved
                && t.Status != TicketStatus.Closed
                && !t.UnassignedWarningSent)
            .ToListAsync();

        foreach (var ticket in unassignedTickets)
        {
            var minutesOpen = (now - ticket.CreatedAt).TotalMinutes;
            var thresholdMinutes = GetAssignmentSlaMinutes(ticket.Priority);

            if (minutesOpen >= thresholdMinutes)
            {
                ticket.UnassignedWarningSent = true;
                var ticketRef = $"TKT-{ticket.Id.ToString().Substring(0, 8).ToUpper()}";

                var leads = await _context.Users
                    .Where(u => u.TenantId == tenantId && (u.Role == UserRole.TeamLead || u.Role == UserRole.Admin) && u.IsActive)
                    .Select(u => u.Id)
                    .ToListAsync();

                foreach (var leadId in leads)
                {
                    await _context.Notifications.AddAsync(new Notification
                    {
                        UserId = leadId,
                        TicketId = ticket.Id,
                        Type = NotificationType.Assignment,
                        IconType = "warning",
                        Title = "Ticket needs assignment",
                        Message = $"{ticketRef} \"{ticket.Title}\" ({ticket.Priority}) has been unassigned for over {thresholdMinutes} minutes. Please assign it to a technician."
                    });
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task EscalateBreachedTicketsAsync(Guid tenantId)
    {
        // No-op — escalation now happens immediately inside CheckAndUpdateSlaStatusAsync.
        await Task.CompletedTask;
    }

    private static int GetAssignmentSlaMinutes(TicketPriority priority) => priority switch
    {
        TicketPriority.Critical => 15,
        TicketPriority.High => 60,
        TicketPriority.Medium => 240,
        TicketPriority.Low => 480,
        _ => 240
    };

    private async Task<List<Guid>> GetLeadRecipientsAsync(Guid tenantId, Ticket ticket)
    {
        var recipientIds = new List<Guid>();
        if (ticket.AssignedToId.HasValue) recipientIds.Add(ticket.AssignedToId.Value);

        var leads = await _context.Users
            .Where(u => u.TenantId == tenantId && (u.Role == UserRole.TeamLead || u.Role == UserRole.Admin) && u.IsActive)
            .Select(u => u.Id)
            .ToListAsync();

        recipientIds.AddRange(leads);
        return recipientIds.Distinct().ToList();
    }

    private async Task<SlaPolicy> GetPolicyAsync(TicketPriority priority, Guid tenantId)
    {
        return await _context.SlaPolicies
            .FirstOrDefaultAsync(p => p.Priority == priority && p.TenantId == tenantId)
            ?? throw new InvalidOperationException($"No SLA policy found for priority {priority}");
    }
}