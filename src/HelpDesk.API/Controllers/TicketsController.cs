using HelpDesk.Core.Entities;
using HelpDesk.Core.Enums;
using HelpDesk.Core.Interfaces.Repositories;
using HelpDesk.Core.Interfaces.Services;
using HelpDesk.Infrastructure.Repositories;
using HelpDesk.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketRepository _ticketRepository;
private readonly ITicketService _ticketService;
    private readonly IUserRepository _userRepository;
private readonly HelpDesk.Infrastructure.Data.HelpDeskDbContext _context;

public TicketsController(
    ITicketRepository ticketRepository,
    ITicketService ticketService,
    HelpDesk.Infrastructure.Data.HelpDeskDbContext context)
{
    _ticketRepository = ticketRepository;
    _ticketService = ticketService;
    _context = context;
}

    [HttpGet("tenant/{tenantId}")]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetByTenant(Guid tenantId)
    {
        var tickets = await _ticketRepository.GetByTenantIdAsync(tenantId);
        return Ok(tickets.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TicketDto>> GetById(Guid id)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id);
        if (ticket == null)
            return NotFound();

        return Ok(MapToDto(ticket));
    }

    [HttpGet("technicians/{tenantId}")]
    public async Task<ActionResult<List<UserDto>>> GetTechnicians(Guid tenantId)
    {
        var technicians = await _userRepository.GetAvailableTechniciansAsync(tenantId);

        return Ok(technicians.Select(t => new UserDto
        {
            Id = t.Id,
            FirstName = t.FirstName,
            LastName = t.LastName,
            Email = t.Email,
            Role = t.Role.ToString(),
            KeycloakId = t.KeycloakId
        }));
    }

    [HttpGet("{id}/detail")]
    public async Task<ActionResult<TicketDetailDto>> GetDetail(Guid id)
    {
        var isStaff = User.IsInRole("Technician") || User.IsInRole("TeamLead") || User.IsInRole("Admin");

        var ticket = await _context.Tickets
             .Include(t => t.SubmittedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.Category)
            .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
            .Include(t => t.History)
                .ThenInclude(h => h.ChangedBy)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
            return NotFound();

           var validTransitions = new Dictionary<TicketStatus, List<TicketStatus>>
            {
                { TicketStatus.Open, new List<TicketStatus> { TicketStatus.Assigned, TicketStatus.InProgress } },
                { TicketStatus.Assigned, new List<TicketStatus> { TicketStatus.InProgress, TicketStatus.Pending } },
                { TicketStatus.InProgress, new List<TicketStatus> { TicketStatus.Pending, TicketStatus.Resolved } },
                { TicketStatus.Pending, new List<TicketStatus> { TicketStatus.InProgress, TicketStatus.Resolved } },
                { TicketStatus.Resolved, new List<TicketStatus> { TicketStatus.InProgress, TicketStatus.Closed } },
                { TicketStatus.Escalated, new List<TicketStatus> { TicketStatus.InProgress, TicketStatus.Resolved, TicketStatus.Closed } },
                { TicketStatus.Closed, new List<TicketStatus>() }
            };

        var validNext = validTransitions.TryGetValue(ticket.Status, out var next)
                        ? next.Select(s => s.ToString()).ToList()
                        : new List<string>();

        var result = new TicketDetailDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status.ToString(),
            Priority = ticket.Priority.ToString(),
            IsSlaBreach = ticket.IsSlaBreach,
            CreatedAt = ticket.CreatedAt,
            ResolutionDueAt = ticket.ResolutionDueAt,
            SubmittedByName = ticket.SubmittedBy != null
                ? $"{ticket.SubmittedBy.FirstName} {ticket.SubmittedBy.LastName}" : null,
            AssignedToName = ticket.AssignedTo != null
                ? $"{ticket.AssignedTo.FirstName} {ticket.AssignedTo.LastName}" : null,
            CategoryName = ticket.Category?.Name,
            Comments = ticket.Comments
                .Where(c => !c.IsDeleted && (!c.IsInternalNote || isStaff))
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    IsInternalNote = c.IsInternalNote,
                    AuthorId = c.AuthorId,
                    AuthorName = $"{c.Author.FirstName} {c.Author.LastName}",
                    AuthorRole = c.Author.Role.ToString(),
                    CreatedAt = c.CreatedAt
                }).ToList(),
            History = ticket.History
                .OrderByDescending(h => h.CreatedAt)
                .Select(h => new TicketHistoryDto
                {
                    Action = h.Action,
                    OldValue = h.OldValue,
                    NewValue = h.NewValue,
                    ChangedByName = h.ChangedByName,
                    CreatedAt = h.CreatedAt
                }).ToList(),

                ValidNextStatuses = validNext,
        };

        return Ok(result);
    }

    [HttpPost("tenant/{tenantId}")]
    public async Task<ActionResult<TicketDto>> Create(Guid tenantId, [FromBody] CreateTicketDto dto)
    {
        if (!Enum.TryParse<TicketPriority>(dto.Priority, true, out var priority))
            return BadRequest($"Invalid priority value: {dto.Priority}");

        var ticket = new Ticket
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = priority,
            CategoryId = dto.CategoryId,
            SubmittedById = dto.SubmittedById
        };

        var created = await _ticketService.CreateTicketAsync(ticket, tenantId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
    }

    [HttpPut("{id}/assign")]
    [Authorize(Roles = "TeamLead,Admin")]
    public async Task<ActionResult<TicketDto>> Assign(Guid id, [FromBody] AssignTicketDto dto)
    {
        try
        {
            var ticket = await _ticketService.AssignTicketAsync(id, dto.TechnicianId, dto.AssignedById);
            return Ok(MapToDto(ticket));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Technician,TeamLead,Admin")]
    public async Task<ActionResult<TicketDto>> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto)
    {
        if (!Enum.TryParse<TicketStatus>(dto.NewStatus, true, out var status))
            return BadRequest($"Invalid status value: {dto.NewStatus}");

        try
        {
            var ticket = await _ticketService.UpdateStatusAsync(id, status, dto.ChangedById);
            return Ok(MapToDto(ticket));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddComment(Guid id, [FromBody] AddCommentDto dto)
    {
        var comment = new TicketComment
        {
            Content = dto.Content,
            IsInternalNote = dto.IsInternalNote,
            AuthorId = dto.AuthorId
        };

        await _ticketService.AddCommentAsync(id, comment);
        return NoContent();
    }



    [HttpPut("{id}/escalate")]
    [Authorize(Roles = "Technician,TeamLead,Admin")]
    public async Task<ActionResult<TicketDto>> Escalate(Guid id, [FromBody] EscalateTicketDto dto)
    {
        // Manual escalation endpoint has been disabled. SLA escalation is handled automatically by the system.
        return BadRequest("Manual escalation is disabled. SLA escalation is handled automatically.");
    }


    private static TicketDto MapToDto(Ticket t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Status = t.Status.ToString(),
        Priority = t.Priority.ToString(),
        IsSlaBreach = t.IsSlaBreach,
        CreatedAt = t.CreatedAt,
        ResolutionDueAt = t.ResolutionDueAt,
        SubmittedById = t.SubmittedById,
        AssignedToId = t.AssignedToId,
        SubmittedByName = t.SubmittedBy != null ? $"{t.SubmittedBy.FirstName} {t.SubmittedBy.LastName}" : null,
        AssignedToName = t.AssignedTo != null ? $"{t.AssignedTo.FirstName} {t.AssignedTo.LastName}" : null,
        CategoryName = t.Category?.Name,
        ResolvedAt = t.ResolvedAt
    };
}