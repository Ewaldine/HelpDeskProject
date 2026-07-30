namespace HelpDesk.Web.Models;

public class SettingsViewModel
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string OfficeLocation { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }

    public bool EmailNotifications { get; set; } = true;
    public bool TicketStatusUpdates { get; set; } = true;
    public bool NewComments { get; set; } = true;
    public bool WeeklySummary { get; set; } = false;
}