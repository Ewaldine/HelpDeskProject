using HelpDesk.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HelpDesk.Shared.DTOs;

namespace HelpDesk.Web.Controllers;

[Authorize(Roles = "Admin")]
public class CategoriesController : BaseController
{
    private readonly ITicketApiService _ticketApiService;

    public CategoriesController(ITicketApiService ticketApiService)
    {
        _ticketApiService = ticketApiService;
    }

    public async Task<IActionResult> Index()
    {
        var token = await GetAccessTokenAsync();
        var categories = await _ticketApiService.GetCategoriesAsync(token);
        return View(categories);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Name required");

        var token = await GetAccessTokenAsync();
        var created = await _ticketApiService.CreateCategoryAsync(dto, token);

        if (created == null)
            return StatusCode(500, "Could not create category");

        return Ok(created);
    }
}