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
}