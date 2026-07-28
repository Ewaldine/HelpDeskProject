using HelpDesk.Core.Entities;

namespace HelpDesk.Core.Interfaces.Services;

public interface INotificationService
{
    Task NotifyTicketAssignedAsync(Ticket ticket);
    Task NotifyTicketUpdatedAsync(Ticket ticket);
    Task NotifySlaBreachAsync(Ticket ticket);
    Task NotifySlaWarningAsync(Ticket ticket);
}