using HelpDesk.Core.Interfaces.Repositories;
using HelpDesk.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationsController(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetForUser(Guid userId)
    {
        var items = await _notificationRepository.GetUnreadByUserIdAsync(userId);
        var dtos = items.Select(n => new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            IsRead = n.IsRead,
            TicketId = n.TicketId,
            CreatedAt = n.CreatedAt
        }).ToList();

        return Ok(dtos);
    }

    [HttpPost("user/{userId}/mark-read")]
    public async Task<IActionResult> MarkAllRead(Guid userId)
    {
        await _notificationRepository.MarkAllAsReadAsync(userId);
        return NoContent();
    }
}
