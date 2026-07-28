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
            if (DateTime.UtcNow > ticket.ResolutionDueAt)
            {
                ticket.IsSlaBreach = true;
                breachedCount++;
            }
        }

        if (breachedCount > 0)
        {
            await _context.SaveChangesAsync();
        }
    }

    public async Task EscalateBreachedTicketsAsync(Guid tenantId)
    {
        var breached = await _ticketRepository.GetSlaBreachedTicketsAsync(tenantId);

        foreach (var ticket in breached)
        {
            if (ticket.Status != TicketStatus.Escalated)
            {
                ticket.Status = TicketStatus.Escalated;
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task<SlaPolicy> GetPolicyAsync(TicketPriority priority, Guid tenantId)
    {
        return await _context.SlaPolicies
            .FirstOrDefaultAsync(p => p.Priority == priority && p.TenantId == tenantId)
            ?? throw new InvalidOperationException($"No SLA policy found for priority {priority}");
    }
}