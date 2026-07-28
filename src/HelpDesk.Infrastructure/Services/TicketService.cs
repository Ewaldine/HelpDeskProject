using HelpDesk.Core.Entities;
using HelpDesk.Core.Enums;
using HelpDesk.Core.Interfaces.Repositories;
using HelpDesk.Core.Interfaces.Services;
using HelpDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Services;

public class TicketService : ITicketService
{
    private readonly HelpDeskDbContext _context;
    private readonly ITicketRepository _ticketRepository;
    private readonly ISlaService _slaService;

    public TicketService(
        HelpDeskDbContext context,
        ITicketRepository ticketRepository,
        ISlaService slaService)
    {
        _context = context;
        _ticketRepository = ticketRepository;
        _slaService = slaService;
    }

    public async Task<Ticket> CreateTicketAsync(Ticket ticket, Guid tenantId)
    {
        ticket.TenantId = tenantId;
        ticket.Status = TicketStatus.Open;
        ticket.ResponseDueAt = await _slaService.CalculateResponseDueAsync(ticket.Priority, tenantId);
        ticket.ResolutionDueAt = await _slaService.CalculateResolutionDueAsync(ticket.Priority, tenantId);

        await _context.Tickets.AddAsync(ticket);
        await _context.SaveChangesAsync();

        await AddHistoryAsync(ticket.Id, "Created", null, "Open", ticket.SubmittedById);

        return ticket;
    }

    public async Task<Ticket> AssignTicketAsync(Guid ticketId, Guid technicianId, Guid assignedById)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId)
            ?? throw new InvalidOperationException("Ticket not found");

        var oldAssignee = ticket.AssignedToId;
        ticket.AssignedToId = technicianId;
        ticket.Status = TicketStatus.Assigned;

        await _context.SaveChangesAsync();

        await AddHistoryAsync(ticketId, "Assigned", oldAssignee?.ToString(), technicianId.ToString(), assignedById);

        return await _context.Tickets
       .Include(t => t.SubmittedBy)
       .Include(t => t.AssignedTo)
       .Include(t => t.Category)
       .FirstAsync(t => t.Id == ticketId);
    }
    private static readonly Dictionary<TicketStatus, List<TicketStatus>> ValidTransitions = new()
    {
        { TicketStatus.Open, new List<TicketStatus> { TicketStatus.Assigned, TicketStatus.InProgress } },
        { TicketStatus.Assigned, new List<TicketStatus> { TicketStatus.InProgress, TicketStatus.Pending } },
        { TicketStatus.InProgress, new List<TicketStatus> { TicketStatus.Pending, TicketStatus.Resolved } },
        { TicketStatus.Pending, new List<TicketStatus> { TicketStatus.InProgress, TicketStatus.Resolved } },
        { TicketStatus.Resolved, new List<TicketStatus> { TicketStatus.InProgress, TicketStatus.Closed } },
        { TicketStatus.Escalated, new List<TicketStatus> { TicketStatus.InProgress, TicketStatus.Resolved, TicketStatus.Closed } },
        { TicketStatus.Closed, new List<TicketStatus>() }
    };

    public async Task<Ticket> UpdateStatusAsync(Guid ticketId, TicketStatus newStatus, Guid changedById)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId)
            ?? throw new InvalidOperationException("Ticket not found");

        if (ticket.AssignedToId == null)
            throw new InvalidOperationException("Cannot update status on an unassigned ticket. Assign it first.");

        if (ticket.Status == TicketStatus.Closed)
            throw new InvalidOperationException("This ticket is closed and cannot be modified further.");

        if (!ValidTransitions.TryGetValue(ticket.Status, out var allowedNext) || !allowedNext.Contains(newStatus))
            throw new InvalidOperationException($"Cannot change status from {ticket.Status} to {newStatus}.");

        var wasEscalated = ticket.Status == TicketStatus.Escalated;

        var oldStatus = ticket.Status;
        ticket.Status = newStatus;

        if (newStatus == TicketStatus.Resolved)
        {
            ticket.ResolvedAt = DateTime.UtcNow;
        }
        else if (newStatus == TicketStatus.Closed)
        {
            ticket.ClosedAt = DateTime.UtcNow;
        }
        else if (oldStatus == TicketStatus.Resolved && newStatus == TicketStatus.InProgress)
        {
            // Reopening — clear the resolved timestamp
            ticket.ResolvedAt = null;
        }

        await _context.SaveChangesAsync();

        await AddHistoryAsync(ticketId, "StatusChanged", oldStatus.ToString(), newStatus.ToString(), changedById);

        if (wasEscalated)
        {
            var changedByUser = await _context.Users.FindAsync(changedById);
            var changedByName = changedByUser != null ? $"{changedByUser.FirstName} {changedByUser.LastName}" : "Someone";
            var isPrivileged = changedByUser?.Role == UserRole.TeamLead || changedByUser?.Role == UserRole.Admin;

            if (!isPrivileged)
            {
                await NotifyTeamLeadsAndAdminsAsync(ticket.TenantId, ticket.Id,
                    $"{changedByName} updated escalated ticket \"{ticket.Title}\" to {newStatus}");
            }
        }

        return ticket;
    }

    private async Task NotifyTeamLeadsAndAdminsAsync(Guid tenantId, Guid ticketId, string message)
    {
        var recipients = await _context.Users
            .Where(u => u.TenantId == tenantId && (u.Role == UserRole.TeamLead || u.Role == UserRole.Admin) && u.IsActive)
            .ToListAsync();

        foreach (var recipient in recipients)
        {
            var notification = new Notification
            {
                UserId = recipient.Id,
                TicketId = ticketId,
                Title = "Escalated Ticket Updated",
                Message = message,
                IsRead = false
            };
            await _context.Notifications.AddAsync(notification);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<Ticket> EscalateTicketAsync(Guid ticketId, Guid escalatedById)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId)
    ?? throw new InvalidOperationException("Ticket not found");

        if (ticket.Status == TicketStatus.Escalated)
            throw new InvalidOperationException("This ticket is already escalated.");

        if (ticket.Status == TicketStatus.Resolved || ticket.Status == TicketStatus.Closed)
            throw new InvalidOperationException("Cannot escalate a resolved or closed ticket.");


        var oldPriority = ticket.Priority;
        ticket.Status = TicketStatus.Escalated;
        ticket.IsSlaBreach = true;

        if (ticket.Priority < TicketPriority.Critical)
        {
            ticket.Priority = ticket.Priority + 1;
        }

        await _context.SaveChangesAsync();

        await AddHistoryAsync(ticketId, "Escalated", oldPriority.ToString(), ticket.Priority.ToString(), escalatedById);

        return await _context.Tickets
       .Include(t => t.SubmittedBy)
       .Include(t => t.AssignedTo)
       .Include(t => t.Category)
       .FirstAsync(t => t.Id == ticketId);
    }

    public async Task AddCommentAsync(Guid ticketId, TicketComment comment)
    {
        comment.TicketId = ticketId;
        await _context.TicketComments.AddAsync(comment);
        await _context.SaveChangesAsync();
    }

    public async Task<Ticket> RateTicketAsync(Guid ticketId, int rating)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5");

        var ticket = await _ticketRepository.GetByIdAsync(ticketId)
            ?? throw new InvalidOperationException("Ticket not found");

        ticket.SatisfactionRating = rating;
        await _context.SaveChangesAsync();

        return ticket;
    }

    private async Task AddHistoryAsync(Guid ticketId, string action, string? oldValue, string? newValue, Guid changedById)
    {
        var user = await _context.Users.FindAsync(changedById);

        var history = new TicketHistory
        {
            TicketId = ticketId,
            Action = action,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedById = changedById,
            ChangedByName = user != null ? $"{user.FirstName} {user.LastName}" : "System"
        };

        await _context.TicketHistories.AddAsync(history);
        await _context.SaveChangesAsync();
    }
}