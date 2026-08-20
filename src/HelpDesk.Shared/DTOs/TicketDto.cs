namespace HelpDesk.Shared.DTOs;

public class TicketDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public bool IsSlaBreach { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ResolutionDueAt { get; set; }
    public Guid SubmittedById { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? SubmittedByName { get; set; }
    public string? AssignedToName { get; set; }
    public string? CategoryName { get; set; }
}