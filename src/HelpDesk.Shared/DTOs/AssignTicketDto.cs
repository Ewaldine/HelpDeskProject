namespace HelpDesk.Shared.DTOs;

public class AssignTicketDto
{
    public Guid TechnicianId { get; set; }
    public Guid AssignedById { get; set; }
}