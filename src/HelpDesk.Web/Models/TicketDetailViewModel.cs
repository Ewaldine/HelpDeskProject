using HelpDesk.Shared.DTOs;

namespace HelpDesk.Web.Models;

public class TicketDetailViewModel
{
    public TicketDetailDto Ticket { get; set; } = null!;
    public List<CommentViewModel> Comments { get; set; } = new();
    public List<HistoryViewModel> History { get; set; } = new();
    public bool CanAssign { get; set; }
    public bool CanUpdateStatus { get; set; }
    public bool CanAddInternalNote { get; set; }
    public List<UserDto> Technicians { get; set; } = new();
}

public class CommentViewModel
{
    public string Content { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorRole { get; set; } = string.Empty;
    public bool IsInternalNote { get; set; }
    public bool IsMine { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class HistoryViewModel
{
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}