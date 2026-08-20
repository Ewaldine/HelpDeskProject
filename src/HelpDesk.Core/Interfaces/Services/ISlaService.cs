using HelpDesk.Core.Entities;
using HelpDesk.Core.Enums;

namespace HelpDesk.Core.Interfaces.Services;

public interface ISlaService
{
    Task<DateTime> CalculateResponseDueAsync(TicketPriority priority, Guid tenantId);
    Task<DateTime> CalculateResolutionDueAsync(TicketPriority priority, Guid tenantId);
    Task CheckAndUpdateSlaStatusAsync(Guid tenantId);
    Task EscalateBreachedTicketsAsync(Guid tenantId);
}