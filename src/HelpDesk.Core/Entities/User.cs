using HelpDesk.Core.Enums;

namespace HelpDesk.Core.Entities;

public class User : BaseEntity
{
    public string KeycloakId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public ICollection<Ticket> SubmittedTickets { get; set; } = new List<Ticket>();
    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();

    // Whether the user receives in-app (real-time) notifications
    //public bool InAppNotificationsEnabled { get; set; } = true;

    // Optional profile fields
    public string? PhoneNumber { get; set; }
    public string? Department { get; set; }
    public string? OfficeLocation { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public bool EmailNotifications { get; set; } = true;
    public bool TicketStatusUpdates { get; set; } = true;
    public bool NewCommentNotifications { get; set; } = true;
    public bool WeeklySummary { get; set; } = false;
}