using HelpDesk.Shared.DTOs;

namespace HelpDesk.Web.Services;

public interface ITicketApiService
{
    Task<List<TicketDto>> GetByTenantAsync(Guid tenantId, string accessToken);
    Task<TicketDto?> GetByIdAsync(Guid id, string accessToken);
    Task<TicketDetailDto?> GetDetailAsync(Guid id, string accessToken);
    Task<TicketDto?> CreateAsync(Guid tenantId, CreateTicketDto dto, string accessToken);
    Task<bool> AssignAsync(Guid ticketId, AssignTicketDto dto, string accessToken);
    Task<(bool Success, string? ErrorMessage)> UpdateStatusAsync(Guid ticketId, UpdateStatusDto dto, string accessToken);
    Task AddCommentAsync(Guid ticketId, AddCommentDto dto, string accessToken);
    Task<UserDto?> GetCurrentUserAsync(string keycloakId,string accessToken);
    Task<List<CategoryDto>> GetCategoriesAsync(string accessToken);
    Task<List<UserDto>> GetTechniciansAsync(Guid tenantId, string accessToken);
    Task<ReportsResultDto?> GetReportsAsync(Guid tenantId, string accessToken);
    Task<AdminDashboardDto?> GetAdminDashboardAsync(Guid tenantId, string accessToken);
    Task<TeamLeadDashboardDto?> GetTeamLeadDashboardAsync(Guid tenantId, string accessToken);
    Task<(bool Success, string? ErrorMessage)> EscalateAsync(Guid ticketId, Guid escalatedById, string accessToken);
}