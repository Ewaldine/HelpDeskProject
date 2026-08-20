using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces.Repositories;
using HelpDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Repositories;

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(HelpDeskDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(n => n.UserId == userId && n.IsRead == false)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _dbSet
            .CountAsync(n => n.UserId == userId && n.IsRead == false);
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var unreadNotifications = await _dbSet
            .Where(n => n.UserId == userId && n.IsRead == false)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();
    }
}