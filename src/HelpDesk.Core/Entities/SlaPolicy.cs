using HelpDesk.Core.Enums;

namespace HelpDesk.Core.Entities;

public class SlaPolicy : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public TicketPriority Priority { get; set; }
    public int ResponseTimeHours { get; set; }
    public int ResolutionTimeHours { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
}