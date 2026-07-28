using HelpDesk.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Data;

public class HelpDeskDbContext : DbContext
{
    public HelpDeskDbContext(DbContextOptions<HelpDeskDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TicketHistory> TicketHistories => Set<TicketHistory>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<SlaPolicy> SlaPolicies => Set<SlaPolicy>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HelpDeskDbContext).Assembly);

        // Soft delete filters — every query automatically excludes IsDeleted = true rows
        modelBuilder.Entity<Tenant>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Ticket>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TicketComment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TicketHistory>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SlaPolicy>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Notification>().HasQueryFilter(e => !e.IsDeleted);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically stamp UpdatedAt on every modified entity before saving
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}