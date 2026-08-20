namespace HelpDesk.Shared.DTOs;

public class TicketDetailDto : TicketDto
{
    public List<CommentDto> Comments { get; set; } = new();
    public List<TicketHistoryDto> History { get; set; } = new();
    public List<string> ValidNextStatuses { get; set; } = new();
}

public class CommentDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsInternalNote { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorRole { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TicketHistoryDto
{
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

