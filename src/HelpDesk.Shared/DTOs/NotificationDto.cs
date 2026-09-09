namespace HelpDesk.Shared.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string Type { get; set; } = string.Empty;
    public string IconType { get; set; } = string.Empty;
    public Guid? TicketId { get; set; }
    public string? TicketNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationCountsDto
{
    public int All { get; set; }
    public int Tickets { get; set; }
    public int Comments { get; set; }
    public int Assignments { get; set; }
    public int System { get; set; }
}