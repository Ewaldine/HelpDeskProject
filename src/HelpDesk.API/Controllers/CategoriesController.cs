using HelpDesk.Infrastructure.Data;
using HelpDesk.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Core.Entities;

namespace HelpDesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly HelpDeskDbContext _context;

    public CategoriesController(HelpDeskDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        var categories = await _context.Categories
            .Where(c => !c.IsDeleted)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ActiveTicketCount = c.Tickets.Count(t => !t.IsDeleted && t.Status != HelpDesk.Core.Enums.TicketStatus.Resolved && t.Status != HelpDesk.Core.Enums.TicketStatus.Closed)
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Name is required");

        var category = new Category
        {
            Name = dto.Name.Trim(),
            Description = dto.Description ?? string.Empty
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var result = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ActiveTicketCount = 0
        };

        return Ok(result);
    }
}