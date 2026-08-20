namespace HelpDesk.Web.Models;

public class EmployeeDashboardViewModel
{
    public int OpenTicketCount { get; set; }
    public int ResolvedThisMonth { get; set; }
    public double AvgResolutionHours { get; set; }
    public List<TicketRowViewModel> RecentTickets { get; set; } = new();
}

public class TicketRowViewModel
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}