namespace HelpDesk.Shared.DTOs;

public class NotificationPreferencesDto
{
    public bool EmailNotifications { get; set; }
    public bool TicketStatusUpdates { get; set; }
    public bool NewCommentNotifications { get; set; }
    public bool WeeklySummary { get; set; }
}