namespace HelpDesk.Shared.DTOs;

public class TeamLeadDashboardDto
{
    public int UnassignedCount { get; set; }
    public int TeamOpenTickets { get; set; }
    public int SlaBreachesThisWeek { get; set; }
    public double AvgResolutionHours { get; set; }
    public List<UnassignedTicketDto> UnassignedTickets { get; set; } = new();
    public List<TeamMemberWorkloadDto> TeamWorkload { get; set; } = new();
    public List<TeamActivityDto> RecentActivity { get; set; } = new();
}

public class UnassignedTicketDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SubmittedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TeamMemberWorkloadDto
{
    public string Name { get; set; } = string.Empty;
    public int ResolvedThisWeek { get; set; }
    public int CurrentLoad { get; set; }
    public int Capacity { get; set; }
}

public class TeamActivityDto
{
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}