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
            KeycloakId = user.KeycloakId
        });
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