namespace HelpDesk.Shared.DTOs;

public class ReportsResultDto
{
    public List<DailyActivityDto> WeeklyActivity { get; set; } = new();
    public List<MonthlyTrendDto> MonthlyTrend { get; set; } = new();
    public double ResolutionRate { get; set; }
    public double AvgResponseHours { get; set; }
}

public class DailyActivityDto
{
    public string Day { get; set; } = string.Empty;
    public int Created { get; set; }
    public int Resolved { get; set; }
}

public class MonthlyTrendDto
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
}