namespace HelpDesk.Shared.DTOs;

public class AddCommentDto
{
    public string Content { get; set; } = string.Empty;
    public bool IsInternalNote { get; set; } = false;
    public Guid AuthorId { get; set; }
}