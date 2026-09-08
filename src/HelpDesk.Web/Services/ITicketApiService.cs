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
    /// <summary>
    /// Retrieves unread notifications for the specified user from the API.
    /// </summary>
    /// <param name="userId">User identifier to fetch notifications for.</param>
    /// <param name="accessToken">Bearer access token used for the API request.</param>
    /// <returns>A list of <see cref="HelpDesk.Shared.DTOs.NotificationDto"/> representing unread notifications.</returns>
    Task<List<HelpDesk.Shared.DTOs.NotificationDto>> GetNotificationsAsync(Guid userId, string accessToken);

    /// <summary>
    /// Marks all notifications as read for the specified user.
    /// </summary>
    /// <param name="userId">User identifier whose notifications should be marked read.</param>
    /// <param name="accessToken">Bearer access token used for the API request.</param>
    Task MarkAllNotificationsReadAsync(Guid userId, string accessToken);

    /// <summary>
    /// Fetches the stored user preferences from the API.
    /// </summary>
    Task<UserPreferencesDto?> GetUserPreferencesAsync(Guid userId, string accessToken);

    /// <summary>
    /// Updates the user's preferences via the API.
    /// </summary>
    Task UpdateUserPreferencesAsync(Guid userId, object preferences, string accessToken);

    /// <summary>
    /// Updates basic user profile fields such as FirstName, LastName, PhoneNumber, Department, OfficeLocation
    /// </summary>

    Task UpdateProfileAsync(Guid userId, UpdateProfileDto dto, string accessToken);
    Task UpdateNotificationPreferencesAsync(Guid userId, NotificationPreferencesDto dto, string accessToken);
}