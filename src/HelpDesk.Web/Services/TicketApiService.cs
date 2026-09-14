using System.Net.Http.Headers;
using System.Net.Http.Json;
using HelpDesk.Shared.DTOs;

namespace HelpDesk.Web.Services;

public class TicketApiService : ITicketApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public TicketApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CreateClient(string accessToken)
    {
        Console.WriteLine($"[DEBUG] Access token being sent: {accessToken}");
        var client = _httpClientFactory.CreateClient("HelpDeskApi");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    public async Task<List<TicketDto>> GetByTenantAsync(Guid tenantId, string accessToken)
    {
        var client = CreateClient(accessToken);
        var response = await client.GetAsync($"api/Tickets/tenant/{tenantId}");

        Console.WriteLine($"[DEBUG] GetByTenant response: {(int)response.StatusCode} {response.StatusCode}");

        if (!response.IsSuccessStatusCode)
            return new List<TicketDto>();

        var result = await response.Content.ReadFromJsonAsync<List<TicketDto>>();
        return result ?? new List<TicketDto>();
    }

    public async Task<TicketDto?> GetByIdAsync(Guid id, string accessToken)
    {
        var client = CreateClient(accessToken);
        var response = await client.GetAsync($"api/Tickets/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TicketDto>();
    }

    public async Task<TicketDetailDto?> GetDetailAsync(Guid id, string accessToken)
    {
        var client = CreateClient(accessToken);
        var response = await client.GetAsync($"api/Tickets/{id}/detail");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TicketDetailDto>();
    }

    public async Task<TicketDto?> CreateAsync(Guid tenantId, CreateTicketDto dto, string accessToken)
    {
        var client = CreateClient(accessToken);
        var response = await client.PostAsJsonAsync($"api/Tickets/tenant/{tenantId}", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TicketDto>();
    }
    public async Task<List<UserDto>> GetTechniciansAsync(Guid tenantId, string accessToken)
    {
        var client = CreateClient(accessToken);
        var result = await client.GetFromJsonAsync<List<UserDto>>($"api/Users/technicians/{tenantId}");
        return result ?? new List<UserDto>();
    }
    public async Task<(bool Success, string? ErrorMessage)> AssignAsync(Guid ticketId, AssignTicketDto dto, string accessToken)
    {
        var client = CreateClient(accessToken);
        var response = await client.PutAsJsonAsync($"api/Tickets/{ticketId}/assign", dto);
        if (response.IsSuccessStatusCode) return (true, null);
        var error = await response.Content.ReadAsStringAsync();
        return (false, error);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateStatusAsync(Guid ticketId, UpdateStatusDto dto, string accessToken)
    {
        var client = CreateClient(accessToken);
        var response = await client.PutAsJsonAsync($"api/Tickets/{ticketId}/status", dto);

        if (response.IsSuccessStatusCode)
            return (true, null);

        var errorMessage = await response.Content.ReadAsStringAsync();
        return (false, errorMessage);
    }

    public async Task AddCommentAsync(Guid ticketId, AddCommentDto dto, string accessToken)
    {
        var client = CreateClient(accessToken);
        await client.PostAsJsonAsync($"api/Tickets/{ticketId}/comments", dto);
    }

    public async Task<UserDto?> GetCurrentUserAsync(string keycloakId,string accessToken)
    {
        var client = CreateClient(accessToken);
        var response = await client.GetAsync($"api/Users/by-keycloak/{keycloakId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<UserDto>();
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync(string accessToken)
    {
        var client = CreateClient(accessToken);
        var result = await client.GetFromJsonAsync<List<CategoryDto>>("api/Categories");
        return result ?? new List<CategoryDto>();
    }

    public async Task<CategoryDto?> CreateCategoryAsync(CreateCategoryDto dto, string accessToken)
    {
        var client = CreateClient(accessToken);
        var response = await client.PostAsJsonAsync("api/Categories", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CategoryDto>();
    }

    public async Task<ReportsResultDto?> GetReportsAsync(Guid tenantId, string accessToken)
    {
        var client = CreateClient(accessToken);
        var response = await client.GetAsync($"api/Reports/tenant/{tenantId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ReportsResultDto>();
    }
    public async Task<AdminDashboardDto?> GetAdminDashboardAsync(Guid tenantId, string accessToken)
    {
        var client = CreateClient(accessToken);
        var response = await client.GetAsync($"api/Reports/admin-dashboard/{tenantId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AdminDashboardDto>();
    }

    public async Task<TeamLeadDashboardDto?> GetTeamLeadDashboardAsync(Guid tenantId, string accessToken)
    {
        var client = CreateClient(accessToken);
        var response = await client.GetAsync($"api/Reports/teamlead-dashboard/{tenantId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TeamLeadDashboardDto>();
    }



    public async Task<UserPreferencesDto?> GetUserPreferencesAsync(Guid userId, string accessToken)
    {
        var client = CreateClient(accessToken);
        var response = await client.GetAsync($"api/Users/{userId}/preferences");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<UserPreferencesDto>();
    }

    public async Task UpdateUserPreferencesAsync(Guid userId, object preferences, string accessToken)
    {
        var client = CreateClient(accessToken);
        await client.PutAsJsonAsync($"api/Users/{userId}/preferences", preferences);
    }

    public async Task<UserDto?> UpdateUserProfileAsync(Guid userId, object profile, string accessToken)
    {
        var client = CreateClient(accessToken);
        var response = await client.PutAsJsonAsync($"api/Users/{userId}/profile", profile);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<UserDto>();
    }



    public async Task<(bool Success, string? ErrorMessage)> EscalateAsync(Guid ticketId, Guid escalatedById, string accessToken)
    {
        var client = CreateClient(accessToken);
        var response = await client.PutAsJsonAsync($"api/Tickets/{ticketId}/escalate", new { EscalatedById = escalatedById });

        if (response.IsSuccessStatusCode)
            return (true, null);

        var errorMessage = await response.Content.ReadAsStringAsync();
        return (false, errorMessage);
    }

    public async Task UpdateProfileAsync(Guid userId, UpdateProfileDto dto, string accessToken)
    {
        var client = CreateClient(accessToken);
        await client.PutAsJsonAsync($"api/Users/{userId}/profile", dto);
    }

    public async Task UpdateNotificationPreferencesAsync(Guid userId, NotificationPreferencesDto dto, string accessToken)
    {
        var client = CreateClient(accessToken);
        await client.PutAsJsonAsync($"api/Users/{userId}/notifications", dto);
    }

    public async Task<List<NotificationDto>> GetNotificationsAsync(Guid userId, string accessToken)
    {
        var client = CreateClient(accessToken);
        var result = await client.GetFromJsonAsync<List<NotificationDto>>($"api/Notifications/{userId}");
        return result ?? new List<NotificationDto>();
    }

    public async Task<NotificationCountsDto> GetNotificationCountsAsync(Guid userId, string accessToken)
    {
        var client = CreateClient(accessToken);
        var result = await client.GetFromJsonAsync<NotificationCountsDto>($"api/Notifications/{userId}/counts");
        return result ?? new NotificationCountsDto();
    }

    public async Task MarkAllNotificationsReadAsync(Guid userId, string accessToken)
    {
        var client = CreateClient(accessToken);
        await client.PutAsync($"api/Notifications/{userId}/mark-all-read", null);
    }

    public async Task MarkNotificationReadAsync(Guid userId, Guid notificationId, string accessToken)
    {
        var client = CreateClient(accessToken);
        await client.PutAsync($"api/Notifications/{userId}/{notificationId}/read", null);
    }

    public async Task ClearAllNotificationsAsync(Guid userId, string accessToken)
    {
        var client = CreateClient(accessToken);
        await client.DeleteAsync($"api/Notifications/{userId}/clear-all");
    }

    public async Task UpdateProfileWithPhotoAsync(Guid userId, UpdateProfileDto dto, string accessToken)
    {
        var client = CreateClient(accessToken);
        await client.PutAsJsonAsync($"api/Users/{userId}/profile", dto);
    }
}