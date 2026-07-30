namespace HelpDesk.Web.Models;

public class TechnicianDashboardViewModel
{
    public int AssignedTicketCount { get; set; }
    public int ResolvedToday { get; set; }
    public string AvgResponseTime { get; set; } = string.Empty;
    public List<TechnicianTicketRowViewModel> TicketQueue { get; set; } = new();
}

public class TechnicianTicketRowViewModel
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SlaCountdown { get; set; } = string.Empty;
    public bool IsOverdue { get; set; }
    public string SubmittedBy { get; set; } = string.Empty;
}