using System;

namespace HelpDesk.Shared.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public Guid? TicketId { get; set; }
    public DateTime CreatedAt { get; set; }
}
