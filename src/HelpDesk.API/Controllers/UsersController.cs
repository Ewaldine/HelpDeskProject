using HelpDesk.Core.Interfaces.Repositories;
using HelpDesk.Infrastructure.Data;
using HelpDesk.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly HelpDeskDbContext _context;

    public UsersController(IUserRepository userRepository, HelpDeskDbContext context)
    {
        _userRepository = userRepository;
        _context = context;
    }

public class PreferencesUpdateDto
{
    public bool InAppNotifications { get; set; }
}

public class UserProfileUpdateDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Department { get; set; }
    public string? OfficeLocation { get; set; }
}
    [HttpGet("by-keycloak/{keycloakId}")]
    public async Task<ActionResult<UserDto>> GetByKeycloakId(string keycloakId)
    {
        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId);
        if (user == null) return NotFound();

        return Ok(new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToString(),
            KeycloakId = user.KeycloakId,
            PhoneNumber = user.PhoneNumber,
            Department = user.Department,
            OfficeLocation = user.OfficeLocation,
            EmailNotifications = user.EmailNotifications,
            TicketStatusUpdates = user.TicketStatusUpdates,
            NewCommentNotifications = user.NewCommentNotifications,
            WeeklySummary = user.WeeklySummary
        });
    }

    [HttpGet("{id}/preferences")]
    public async Task<ActionResult<object>> GetPreferences(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        return Ok(new
        {
            InAppNotifications = user.InAppNotificationsEnabled
        });
    }

    [HttpPut("{id}/preferences")]
    public async Task<IActionResult> UpdatePreferences(Guid id, [FromBody] PreferencesUpdateDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.InAppNotificationsEnabled = dto.InAppNotifications;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id}/profile")]
    public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] UpdateProfileDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;
        user.Department = dto.Department;
        user.OfficeLocation = dto.OfficeLocation;

        await _userRepository.UpdateAsync(user);
        return NoContent();
    }

    [HttpPut("{id}/notifications")]
    public async Task<IActionResult> UpdateNotifications(Guid id, [FromBody] NotificationPreferencesDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();

        user.EmailNotifications = dto.EmailNotifications;
        user.TicketStatusUpdates = dto.TicketStatusUpdates;
        user.NewCommentNotifications = dto.NewCommentNotifications;
        user.WeeklySummary = dto.WeeklySummary;

        await _userRepository.UpdateAsync(user);
        return NoContent();
    }

    [HttpGet("technicians/{tenantId}")]
    public async Task<ActionResult<List<UserDto>>> GetTechnicians(Guid tenantId)
    {
        var technicians = await _userRepository.GetAvailableTechniciansAsync(tenantId);
        var technicianIds = technicians.Select(t => t.Id).ToList();

        var openCounts = await _context.Tickets
            .Where(t => technicianIds.Contains(t.AssignedToId ?? Guid.Empty)
                && t.Status != Core.Enums.TicketStatus.Resolved
                && t.Status != Core.Enums.TicketStatus.Closed)
            .GroupBy(t => t.AssignedToId)
            .Select(g => new { TechnicianId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TechnicianId!.Value, x => x.Count);

        return Ok(technicians.Select(t => new UserDto
        {
            Id = t.Id,
            FirstName = t.FirstName,
            LastName = t.LastName,
            Email = t.Email,
            Role = t.Role.ToString(),
            KeycloakId = t.KeycloakId,
            OpenTicketCount = openCounts.GetValueOrDefault(t.Id, 0)
        }));
    }
}