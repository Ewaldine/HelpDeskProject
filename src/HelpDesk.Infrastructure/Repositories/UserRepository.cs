using HelpDesk.Core.Entities;
using HelpDesk.Core.Enums;
using HelpDesk.Core.Interfaces.Repositories;
using HelpDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(HelpDeskDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByKeycloakIdAsync(string keycloakId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetByTenantIdAsync(Guid tenantId)
    {
        return await _dbSet
            .Where(u => u.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role, Guid tenantId)
    {
        return await _dbSet
            .Where(u => u.Role == role && u.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetAvailableTechniciansAsync(Guid tenantId)
    {
        return await _dbSet
            .Where(u => u.Role == UserRole.Technician
                && u.TenantId == tenantId
                && u.IsActive == true)
            .ToListAsync();
    }
}