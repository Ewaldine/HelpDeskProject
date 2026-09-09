using HelpDesk.Core.Enums;

namespace HelpDesk.Core.Entities;

public class Notification : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public NotificationType Type { get; set; } = NotificationType.System;
    public string IconType { get; set; } = "system";

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid? TicketId { get; set; }
    public Ticket? Ticket { get; set; }
}