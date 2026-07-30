namespace HelpDesk.Web.Models;

public class TeamViewModel
{
    public int ResolvedThisWeek { get; set; }
    public int TotalOpenTickets { get; set; }
    public double TeamAvgResolutionHours { get; set; }
    public List<TeamMemberDetailViewModel> Members { get; set; } = new();
}

public class TeamMemberDetailViewModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public int ResolvedThisWeek { get; set; }
    public double AvgResolutionHours { get; set; }
    public int OpenTickets { get; set; }
    public int Capacity { get; set; } = 10;
}