namespace HelpDesk.Web.Models;

public class TeamLeadDashboardViewModel
{
    public string TeamLeadFirstName { get; set; } = string.Empty;
    public int UnassignedCount { get; set; }
    public int UrgentUnassignedCount { get; set; }
    public int TeamOpenTickets { get; set; }
    public int SlaBreachesThisWeek { get; set; }
    public double AvgResolutionHours { get; set; }
    public List<UnassignedTicketRowViewModel> UnassignedTickets { get; set; } = new();
    public List<TeamMemberWorkloadViewModel> TeamWorkload { get; set; } = new();
    public List<TeamActivityViewModel> RecentActivity { get; set; } = new();
    public List<HelpDesk.Shared.DTOs.UserDto> Technicians { get; set; } = new();
}

public class UnassignedTicketRowViewModel
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SubmittedBy { get; set; } = string.Empty;
    public string TimeOpen { get; set; } = string.Empty;
}

public class TeamMemberWorkloadViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public int ResolvedThisWeek { get; set; }
    public int CurrentLoad { get; set; }
    public int Capacity { get; set; }
}

public class TeamActivityViewModel
{
    public string Message { get; set; } = string.Empty;
    public string TimeAgo { get; set; } = string.Empty;
    public string IconType { get; set; } = string.Empty; // "resolved", "escalated", "assigned", "updated", "warning"
}