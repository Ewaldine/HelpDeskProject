using HelpDesk.Core.Entities;

namespace HelpDesk.Core.Interfaces.Repositories;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task MarkAllAsReadAsync(Guid userId);
}