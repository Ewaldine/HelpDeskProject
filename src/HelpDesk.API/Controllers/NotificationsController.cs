using HelpDesk.Infrastructure.Data;
using HelpDesk.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly HelpDeskDbContext _context;

    public NotificationsController(HelpDeskDbContext context)
    {
        _context = context;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<List<NotificationDto>>> GetAll(Guid userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                Type = n.Type.ToString(),
                IconType = n.IconType,
                TicketId = n.TicketId,
                TicketNumber = n.TicketId != null ? "TKT-" + n.TicketId.ToString()!.Substring(0, 8).ToUpper() : null,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpGet("{userId}/counts")]
    public async Task<ActionResult<NotificationCountsDto>> GetCounts(Guid userId)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        return Ok(new NotificationCountsDto
        {
            All = unread.Count,
            Tickets = unread.Count(n => n.Type == HelpDesk.Core.Enums.NotificationType.Ticket),
            Comments = unread.Count(n => n.Type == HelpDesk.Core.Enums.NotificationType.Comment),
            Assignments = unread.Count(n => n.Type == HelpDesk.Core.Enums.NotificationType.Assignment),
            System = unread.Count(n => n.Type == HelpDesk.Core.Enums.NotificationType.System)
        });
    }

    [HttpPut("{userId}/mark-all-read")]
    public async Task<IActionResult> MarkAllRead(Guid userId)
    {
        var unread = await _context.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        foreach (var n in unread) n.IsRead = true;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{userId}/{id}/read")]
    public async Task<IActionResult> MarkOneRead(Guid userId, Guid id)
    {
        var notif = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        if (notif == null) return NotFound();
        notif.IsRead = true;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{userId}/clear-all")]
    public async Task<IActionResult> ClearAll(Guid userId)
    {
        var all = await _context.Notifications.Where(n => n.UserId == userId).ToListAsync();
        foreach (var n in all) n.IsDeleted = true;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}