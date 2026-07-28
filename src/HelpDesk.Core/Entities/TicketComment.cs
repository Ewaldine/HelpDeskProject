namespace HelpDesk.Core.Entities;

public class TicketComment : BaseEntity
{
    public string Content { get; set; } = string.Empty;
    public bool IsInternalNote { get; set; } = false;

    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;
}