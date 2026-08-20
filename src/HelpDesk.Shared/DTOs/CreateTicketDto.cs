namespace HelpDesk.Shared.DTOs;

public class CreateTicketDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid SubmittedById { get; set; }
}