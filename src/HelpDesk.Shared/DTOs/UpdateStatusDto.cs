namespace HelpDesk.Shared.DTOs;

public class UpdateStatusDto
{
    public string NewStatus { get; set; } = string.Empty;
    public Guid ChangedById { get; set; }
}