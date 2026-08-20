namespace HelpDesk.Core.Entities;

public class TicketHistory : BaseEntity
{
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangedByName { get; set; } = string.Empty;

    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public Guid ChangedById { get; set; }
    public User ChangedBy { get; set; } = null!;
}