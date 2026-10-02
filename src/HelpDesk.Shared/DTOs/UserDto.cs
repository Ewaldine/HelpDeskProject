namespace HelpDesk.Shared.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string KeycloakId { get; set; } = string.Empty;
    public int OpenTicketCount { get; set; }
    // optional profile fields
    public string? ProfilePhotoUrl { get; set; }
    public string? Department { get; set; }
    public string? OfficeLocation { get; set; }
    public string? PhoneNumber { get; set; }
    public bool EmailNotifications { get; set; }
    public bool TicketStatusUpdates { get; set; }
    public bool NewCommentNotifications { get; set; }
    public bool WeeklySummary { get; set; }
}