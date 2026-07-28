using HelpDesk.Core.Entities;
using HelpDesk.Core.Enums;

namespace HelpDesk.Core.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByKeycloakIdAsync(string keycloakId);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetByTenantIdAsync(Guid tenantId);
    Task<IEnumerable<User>> GetByRoleAsync(UserRole role, Guid tenantId);
    Task<IEnumerable<User>> GetAvailableTechniciansAsync(Guid tenantId);
}