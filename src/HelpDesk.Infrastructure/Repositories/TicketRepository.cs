using HelpDesk.Core.Entities;
using HelpDesk.Core.Enums;
using HelpDesk.Core.Interfaces.Repositories;
using HelpDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Repositories;

public class TicketRepository : Repository<Ticket>, ITicketRepository
{
    public TicketRepository(HelpDeskDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Ticket>> GetByTenantIdAsync(Guid tenantId)
    {
        return await _dbSet
            .Where(t => t.TenantId == tenantId)
            .Include(t => t.SubmittedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.Category)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetBySubmittedByIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(t => t.SubmittedById == userId)
            .Include(t => t.Category)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetByAssignedToIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(t => t.AssignedToId == userId)
            .Include(t => t.SubmittedBy)
            .Include(t => t.Category)
            .OrderByDescending(t => t.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetByStatusAsync(TicketStatus status, Guid tenantId)
    {
        return await _dbSet
            .Where(t => t.Status == status && t.TenantId == tenantId)
            .Include(t => t.AssignedTo)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetByPriorityAsync(TicketPriority priority, Guid tenantId)
    {
        return await _dbSet
            .Where(t => t.Priority == priority && t.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetSlaBreachedTicketsAsync(Guid tenantId)
    {
        return await _dbSet
            .Where(t => t.TenantId == tenantId
                && t.IsSlaBreach == true
                && t.Status != TicketStatus.Resolved
                && t.Status != TicketStatus.Closed)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetTicketsApproachingSlaBreachAsync(Guid tenantId)
    {
        var warningThreshold = DateTime.UtcNow.AddHours(2);

        return await _dbSet
            .Where(t => t.TenantId == tenantId
                && t.IsSlaBreach == false
                && t.ResolutionDueAt <= warningThreshold
                && t.Status != TicketStatus.Resolved
                && t.Status != TicketStatus.Closed)
            .ToListAsync();
    }

    public async Task<int> GetOpenTicketCountAsync(Guid tenantId)
    {
        return await _dbSet
            .CountAsync(t => t.TenantId == tenantId
                && t.Status != TicketStatus.Resolved
                && t.Status != TicketStatus.Closed);
    }

    public async Task<double> GetAverageResolutionTimeAsync(Guid tenantId)
    {
        var resolvedTickets = await _dbSet
            .Where(t => t.TenantId == tenantId && t.ResolvedAt != null)
            .Select(t => new { t.CreatedAt, t.ResolvedAt })
            .ToListAsync();

        if (!resolvedTickets.Any())
            return 0;

        return resolvedTickets
            .Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours);
    }
}