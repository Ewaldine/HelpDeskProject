using HelpDesk.Core.Enums;

namespace HelpDesk.Core.Entities;

public class Ticket : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public bool IsSlaBreach { get; set; } = false;
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime ResponseDueAt { get; set; }
    public DateTime ResolutionDueAt { get; set; }
    public int? SatisfactionRating { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid SubmittedById { get; set; }
    public User SubmittedBy { get; set; } = null!;

    public Guid? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    public ICollection<TicketHistory> History { get; set; } = new List<TicketHistory>();
}