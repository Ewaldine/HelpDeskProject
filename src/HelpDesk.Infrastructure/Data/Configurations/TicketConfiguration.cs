using HelpDesk.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Data.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).IsRequired().HasMaxLength(4000);

        builder.HasOne(t => t.Tenant)
            .WithMany(tn => tn.Tickets)
            .HasForeignKey(t => t.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.SubmittedBy)
            .WithMany(u => u.SubmittedTickets)
            .HasForeignKey(t => t.SubmittedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AssignedTo)
            .WithMany(u => u.AssignedTickets)
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Category)
            .WithMany(c => c.Tickets)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Performance indexes — these speed up the most common queries
        builder.HasIndex(t => t.TenantId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.Priority);
        builder.HasIndex(t => t.AssignedToId);
        builder.HasIndex(t => t.SubmittedById);
        builder.HasIndex(t => new { t.TenantId, t.Status });
        builder.HasIndex(t => t.ResolutionDueAt);
    }
}