using HelpDesk.Core.Entities;
using HelpDesk.Core.Enums;

namespace HelpDesk.Core.Interfaces.Services;

public interface ITicketService
{
    Task<Ticket> CreateTicketAsync(Ticket ticket, Guid tenantId);
    Task<Ticket> AssignTicketAsync(Guid ticketId, Guid technicianId, Guid assignedById);
    Task<Ticket> UpdateStatusAsync(Guid ticketId, TicketStatus newStatus, Guid changedById);
    Task<Ticket> EscalateTicketAsync(Guid ticketId, Guid escalatedById);
    Task AddCommentAsync(Guid ticketId, TicketComment comment);
    Task<Ticket> RateTicketAsync(Guid ticketId, int rating);
}