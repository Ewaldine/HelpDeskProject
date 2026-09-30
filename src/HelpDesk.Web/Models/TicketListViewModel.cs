using HelpDesk.Shared.DTOs;

namespace HelpDesk.Web.Models;

public class TicketListViewModel
{
    public string PageTitle { get; set; } = string.Empty;
    public string PageSubtitle { get; set; } = string.Empty;
    public List<TicketDto> Tickets { get; set; } = new();
    public bool ShowSubmitterFilter { get; set; }
    public List<HelpDesk.Shared.DTOs.UserDto> Technicians { get; set; } = new();
}