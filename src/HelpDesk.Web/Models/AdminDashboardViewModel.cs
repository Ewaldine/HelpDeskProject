namespace HelpDesk.Web.Models;

public class AdminDashboardViewModel
{
    public int TotalOpenTickets { get; set; }
    public int SlaBreachesWeek { get; set; }
    public int TotalResolved { get; set; }
    public int ActiveTechnicians { get; set; }
    public List<CategoryCountViewModel> TicketsByCategory { get; set; } = new();
    public List<PriorityPercentViewModel> TicketsByPriority { get; set; } = new();
    public List<TechnicianWorkloadViewModel> TechnicianWorkload { get; set; } = new();
}

public class CategoryCountViewModel
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class PriorityPercentViewModel
{
    public string Priority { get; set; } = string.Empty;
    public int Percent { get; set; }
}

public class TechnicianWorkloadViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public int AssignedTickets { get; set; }
    public int ResolvedThisMonth { get; set; }
    public double AvgResolutionHours { get; set; }
}