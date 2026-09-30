namespace HelpDesk.Shared.DTOs;

public class AdminDashboardDto
{
    public int TotalOpenTickets { get; set; }
    public int SlaBreachesWeek { get; set; }
    public int TotalResolved { get; set; }
    public int ActiveTechnicians { get; set; }
    public List<CategoryCountDto> TicketsByCategory { get; set; } = new();
    public List<PriorityPercentDto> TicketsByPriority { get; set; } = new();
    public List<TechnicianWorkloadDto> TechnicianWorkload { get; set; } = new();
}

public class CategoryCountDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class PriorityPercentDto
{
    public string Priority { get; set; } = string.Empty;
    public int Percent { get; set; }
}

public class TechnicianWorkloadDto
{
    public string Name { get; set; } = string.Empty;
    public int AssignedTickets { get; set; }
    public int ResolvedThisMonth { get; set; }
    public double AvgResolutionHours { get; set; }
}