using HelpDesk.Core.Entities;
using HelpDesk.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Data.Seeding;

public static class DbSeeder
{
    public static async Task SeedAsync(HelpDeskDbContext context)
    {
        if (await context.Tenants.AnyAsync())
        {
            return; // Already seeded, do nothing
        }

        // 1. Create a Tenant
        var tenant = new Tenant
        {
            Name = "Standard Bank Demo",
            Domain = "standardbank-demo.local",
            IsActive = true
        };
        await context.Tenants.AddAsync(tenant);
        await context.SaveChangesAsync();

        // 2. Create Categories
        var categories = new List<Category>
        {
            new() { Name = "Hardware", Description = "Computer, monitor, printer issues" },
            new() { Name = "Software", Description = "Application installs and bugs" },
            new() { Name = "Network", Description = "Connectivity and VPN issues" },
            new() { Name = "Email", Description = "Outlook and email related issues" },
            new() { Name = "Access Request", Description = "Account and permission requests" }
        };
        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        // 3. Create SLA Policies (one per priority)
        var slaPolicies = new List<SlaPolicy>
        {
            new() { Name = "Critical SLA", Priority = TicketPriority.Critical, ResponseTimeHours = 1, ResolutionTimeHours = 4, TenantId = tenant.Id },
            new() { Name = "High SLA", Priority = TicketPriority.High, ResponseTimeHours = 4, ResolutionTimeHours = 8, TenantId = tenant.Id },
            new() { Name = "Medium SLA", Priority = TicketPriority.Medium, ResponseTimeHours = 8, ResolutionTimeHours = 24, TenantId = tenant.Id },
            new() { Name = "Low SLA", Priority = TicketPriority.Low, ResponseTimeHours = 24, ResolutionTimeHours = 72, TenantId = tenant.Id }
        };
        await context.SlaPolicies.AddRangeAsync(slaPolicies);
        await context.SaveChangesAsync();

        // 4. Create Users (Employee, Technician, Team Lead, Admin)
        var employee = new User
        {
            FirstName = "Thabo",
            LastName = "Nkosi",
            Email = "thabo.nkosi@example.com",
            KeycloakId = Guid.Parse("8424f644-2b80-403e-946f-2391db69c0a2").ToString(),
            Role = UserRole.Employee,
            TenantId = tenant.Id,
            IsActive = true
        };

        var technician = new User
        {
            FirstName = "Sarah",
            LastName = "Mokoena",
            Email = "sarah.mokoena@example.com",
            KeycloakId = Guid.Parse("c817caa5-e864-4bdd-b11c-c576898622b8").ToString(),
            Role = UserRole.Technician,
            TenantId = tenant.Id,
            IsActive = true
        };

        var teamLead = new User
        {
            FirstName = "John",
            LastName = "Smith",
            Email = "john.smith@example.com",
            KeycloakId = Guid.Parse("5601f69a-b79d-48fc-8b0e-0fc33d419e7a").ToString(),
            Role = UserRole.TeamLead,
            TenantId = tenant.Id,
            IsActive = true
        };

        var admin = new User
        {
            FirstName = "Admin",
            LastName = "User",
            Email = "admin.user@example.com",
            KeycloakId = Guid.Parse("adbbad42-0842-4bfb-a2c9-03ef550a7c2a").ToString(),
            Role = UserRole.Admin,
            TenantId = tenant.Id,
            IsActive = true
        };

        await context.Users.AddRangeAsync(employee, technician, teamLead, admin);
        await context.SaveChangesAsync();

        // 5. Create Sample Tickets
        var hardwareCategory = categories.First(c => c.Name == "Hardware");
        var softwareCategory = categories.First(c => c.Name == "Software");
        var networkCategory = categories.First(c => c.Name == "Network");

        var tickets = new List<Ticket>
        {
            new()
            {
                Title = "Computer won't start",
                Description = "My desktop computer doesn't turn on at all this morning.",
                Status = TicketStatus.Open,
                Priority = TicketPriority.Critical,
                TenantId = tenant.Id,
                SubmittedById = employee.Id,
                CategoryId = hardwareCategory.Id,
                ResponseDueAt = DateTime.UtcNow.AddHours(1),
                ResolutionDueAt = DateTime.UtcNow.AddHours(4)
            },
            new()
            {
                Title = "Need Excel installed",
                Description = "Requesting Microsoft Excel installation for monthly reporting.",
                Status = TicketStatus.Assigned,
                Priority = TicketPriority.Low,
                TenantId = tenant.Id,
                SubmittedById = employee.Id,
                AssignedToId = technician.Id,
                CategoryId = softwareCategory.Id,
                ResponseDueAt = DateTime.UtcNow.AddHours(24),
                ResolutionDueAt = DateTime.UtcNow.AddHours(72)
            },
            new()
            {
                Title = "VPN keeps disconnecting",
                Description = "VPN connection drops every 10 minutes while working from home.",
                Status = TicketStatus.InProgress,
                Priority = TicketPriority.High,
                TenantId = tenant.Id,
                SubmittedById = employee.Id,
                AssignedToId = technician.Id,
                CategoryId = networkCategory.Id,
                ResponseDueAt = DateTime.UtcNow.AddHours(4),
                ResolutionDueAt = DateTime.UtcNow.AddHours(8)
            }
        };

        await context.Tickets.AddRangeAsync(tickets);
        await context.SaveChangesAsync();
    }
}