using HelpDesk.Core.Entities;
using HelpDesk.Core.Enums;

namespace HelpDesk.Core.Interfaces.Repositories;

public interface ITicketRepository : IRepository<Ticket>
{
    Task<IEnumerable<Ticket>> GetByTenantIdAsync(Guid tenantId);
    Task<IEnumerable<Ticket>> GetBySubmittedByIdAsync(Guid userId);
    Task<IEnumerable<Ticket>> GetByAssignedToIdAsync(Guid userId);
    Task<IEnumerable<Ticket>> GetByStatusAsync(TicketStatus status, Guid tenantId);
    Task<IEnumerable<Ticket>> GetByPriorityAsync(TicketPriority priority, Guid tenantId);
    Task<IEnumerable<Ticket>> GetSlaBreachedTicketsAsync(Guid tenantId);
    Task<IEnumerable<Ticket>> GetTicketsApproachingSlaBreachAsync(Guid tenantId);
    Task<int> GetOpenTicketCountAsync(Guid tenantId);
    Task<double> GetAverageResolutionTimeAsync(Guid tenantId);
}