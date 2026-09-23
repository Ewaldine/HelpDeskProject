using HelpDesk.Core.Interfaces.Services;
using HelpDesk.Shared.DTOs;
using HelpDesk.Web.Models;
using HelpDesk.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Web.Controllers;

[Authorize]
public class TicketsController : BaseController
{
    private readonly ITicketApiService _ticketApiService;

    // TODO: replace with dynamic tenant resolution later
    private static readonly Guid TenantId = Guid.Parse("E9DC1A56-E4FE-448F-93B8-8234B1379D2A");

    public TicketsController(ITicketApiService ticketApiService)
    {
        _ticketApiService = ticketApiService;
    }

    public async Task<IActionResult> MyTickets()
    {
        var token = await GetAccessTokenAsync();
        var currentUser = await GetCurrentUserAsync(_ticketApiService, token);
        var allTickets = await _ticketApiService.GetByTenantAsync(TenantId, token);

        List<HelpDesk.Shared.DTOs.TicketDto> myTickets;
        string subtitle;

        if (User.IsInRole("Technician") || User.IsInRole("TeamLead"))
        {
            myTickets = currentUser != null
                ? allTickets.Where(t => t.SubmittedById == currentUser.Id || t.AssignedToId == currentUser.Id).ToList()
                : new();
            subtitle = "Tickets you've submitted or that are assigned to you";
        }
        else
        {
            myTickets = currentUser != null ? allTickets.Where(t => t.SubmittedById == currentUser.Id).ToList() : new();
            subtitle = "Tickets you've submitted";
        }

        return View("TicketList", new TicketListViewModel { PageTitle = "My Tickets", PageSubtitle = subtitle, Tickets = myTickets });
    }

    public async Task<IActionResult> AssignedToMe()
    {
        var token = await GetAccessTokenAsync();
        var currentUser = await GetCurrentUserAsync(_ticketApiService, token);

        var allTickets = await _ticketApiService.GetByTenantAsync(TenantId, token);

        var assignedTickets = currentUser != null
            ? allTickets.Where(t => t.AssignedToId == currentUser.Id).ToList()
            : new List<HelpDesk.Shared.DTOs.TicketDto>();

        var model = new TicketListViewModel
        {
            PageTitle = "Assigned to Me",
            PageSubtitle = "Tickets assigned to you",
            Tickets = assignedTickets
        };

        return View("TicketList", model);
    }

    [HttpPost]
public async Task<IActionResult> AssignToTechnician(Guid id, Guid technicianId)
{
    var token = await GetAccessTokenAsync();
    var currentUser = await GetCurrentUserAsync(_ticketApiService, token);

    if (currentUser != null)
    {
        var dto = new AssignTicketDto { TechnicianId = technicianId, AssignedById = currentUser.Id };
        var result = await _ticketApiService.AssignAsync(id, dto, token);
        if (!result.Success)
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to assign ticket.";
    }

    var referer = Request.Headers["Referer"].ToString();
    return Redirect(!string.IsNullOrEmpty(referer) ? referer : Url.Action("Index", "Dashboard")!);
}


    public async Task<IActionResult> UnassignedQueue()
    {
        var token = await GetAccessTokenAsync();
        var allTickets = await _ticketApiService.GetByTenantAsync(TenantId, token);
        var technicians = await _ticketApiService.GetTechniciansAsync(TenantId, token);

        var unassigned = allTickets
            .Where(t => t.AssignedToId == null && t.Status != "Resolved" && t.Status != "Closed")
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .ToList();

        var model = new TicketListViewModel
        {
            PageTitle = "Unassigned Queue",
            PageSubtitle = "Tickets awaiting technician assignment",
            Tickets = unassigned,
            Technicians = technicians
        };

        return View("UnassignedQueue", model);
    }
    public async Task<IActionResult> AllTickets()
    {
        var token = await GetAccessTokenAsync();
        var tickets = await _ticketApiService.GetByTenantAsync(TenantId, token);

        var model = new TicketListViewModel
        {
            PageTitle = "All Tickets",
            PageSubtitle = "Every ticket in the organization",
            Tickets = tickets
        };

        return View("TicketList", model);
    }

    public async Task<IActionResult> Details(Guid id)
    {
    var token = await GetAccessTokenAsync();
    var ticket = await _ticketApiService.GetDetailAsync(id, token);

    if (ticket == null)
        return NotFound();

    var currentUser = await GetCurrentUserAsync(_ticketApiService, token);

    var model = new TicketDetailViewModel
    {
        Ticket = ticket,
        Comments = ticket.Comments.Select(c => new CommentViewModel
        {
            Content = c.Content,
            AuthorName = c.AuthorName,
            AuthorRole = c.AuthorRole,
            IsInternalNote = c.IsInternalNote,
            IsMine = currentUser != null && c.AuthorId == currentUser.Id,
            CreatedAt = c.CreatedAt
        }).ToList(),
        History = ticket.History.Select(h => new HistoryViewModel
        {
            Action = h.Action,
            OldValue = h.OldValue,
            NewValue = h.NewValue,
            ChangedByName = h.ChangedByName,
            CreatedAt = h.CreatedAt
        }).ToList(),
        CanAssign = User.IsInRole("TeamLead") || User.IsInRole("Admin"),
        CanUpdateStatus = User.IsInRole("Technician") || User.IsInRole("TeamLead") || User.IsInRole("Admin"),
        CanAddInternalNote = User.IsInRole("Technician") || User.IsInRole("TeamLead") || User.IsInRole("Admin")
    };

    if (model.CanAssign)
    {
        model.Technicians = await _ticketApiService.GetTechniciansAsync(TenantId, token);
    }

    return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var token = await GetAccessTokenAsync();
        var categories = await _ticketApiService.GetCategoriesAsync(token);
        return Json(categories);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTicketDto dto)
    {
        var token = await GetAccessTokenAsync();
        Console.WriteLine($"[DEBUG] Create ticket - token length: {token?.Length}");

        var currentUser = await GetCurrentUserAsync(token);
        Console.WriteLine($"[DEBUG] Create ticket - currentUser: {currentUser?.Id}");

        if (currentUser == null)
        {
            Console.WriteLine("[DEBUG] Create ticket - currentUser is null, returning Unauthorized");
            return Unauthorized();
        }

        dto.SubmittedById = currentUser.Id;
        Console.WriteLine($"[DEBUG] Create ticket - submitting with dto: {dto.Title}, {dto.Priority}, {dto.CategoryId}");

        // Prevent near-duplicate submissions: check for a ticket with same title/description by this user in the last 5 seconds
        try
        {
            var recent = await _ticketApiService.GetByTenantAsync(TenantId, token);
            var duplicate = recent.FirstOrDefault(t =>
                t.SubmittedById == currentUser.Id
                && string.Equals(t.Title ?? string.Empty, dto.Title ?? string.Empty, StringComparison.Ordinal)
                && string.Equals(t.Description ?? string.Empty, dto.Description ?? string.Empty, StringComparison.Ordinal)
                && (DateTime.UtcNow - t.CreatedAt).TotalSeconds <= 5);

            if (duplicate != null)
            {
                Console.WriteLine($"[DEBUG] Create ticket - duplicate detected, returning existing ticket {duplicate.Id}");
                return Ok(duplicate);
            }
        }
        catch (Exception ex)
        {
            // Non-fatal: if duplicate check fails, continue to create the ticket but log for diagnostics
            Console.WriteLine($"[WARN] Duplicate check failed: {ex.Message}");
        }

        var ticket = await _ticketApiService.CreateAsync(TenantId, dto, token);
        Console.WriteLine($"[DEBUG] Create ticket - result: {(ticket == null ? "NULL" : ticket.Id.ToString())}");

        if (ticket == null)
            return BadRequest();

        return Ok(ticket);
    }

    [HttpPost]
    public async Task<IActionResult> AddComment(Guid id, string content, bool isInternalNote = false)
    {
        var token = await GetAccessTokenAsync();
        var currentUser = await GetCurrentUserAsync(token);

        if (currentUser != null)
        {
            var dto = new AddCommentDto
            {
                Content = content,
                IsInternalNote = isInternalNote,
                AuthorId = currentUser.Id
            };

            await _ticketApiService.AddCommentAsync(id, dto, token);
        }

        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(Guid id, string newStatus)
    {
        var token = await GetAccessTokenAsync();
        var currentUser = await GetCurrentUserAsync(_ticketApiService, token);

        if (currentUser != null)
        {
            var dto = new UpdateStatusDto
            {
                NewStatus = newStatus,
                ChangedById = currentUser.Id
            };

            var result = await _ticketApiService.UpdateStatusAsync(id, dto, token);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
        }

        return RedirectToAction("Details", new { id });
    }

    // Manual escalate action removed. SLA escalation is handled automatically by the system.


    private async Task<UserDto?> GetCurrentUserAsync(string token)
    {
        Console.WriteLine("[DEBUG] All claims available:");
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"  {claim.Type} = {claim.Value}");
        }

        var keycloakId = User.FindFirst("sub")?.Value
            ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        Console.WriteLine($"[DEBUG] Looking up user by keycloakId: {keycloakId}");
        if (string.IsNullOrEmpty(keycloakId)) return null;
        return await _ticketApiService.GetCurrentUserAsync(keycloakId, token);
    }
}