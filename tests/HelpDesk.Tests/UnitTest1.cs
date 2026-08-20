using HelpDesk.Core.Entities;
using HelpDesk.Core.Enums;
using Xunit;

namespace HelpDesk.Tests;

public class TicketTests
{
    [Fact]
    public void NewTicket_DefaultsToOpenStatus()
    {
        var ticket = new Ticket();

        Assert.Equal(TicketStatus.Open, ticket.Status);
    }

    [Fact]
    public void NewTicket_DefaultsToMediumPriority()
    {
        var ticket = new Ticket();

        Assert.Equal(TicketPriority.Medium, ticket.Priority);
    }

    [Fact]
    public void NewTicket_IsNotSlaBreachedByDefault()
    {
        var ticket = new Ticket();

        Assert.False(ticket.IsSlaBreach);
    }

    [Theory]
    [InlineData(TicketPriority.Low)]
    [InlineData(TicketPriority.Medium)]
    [InlineData(TicketPriority.High)]
    public void Priority_CanBeIncremented_WhenBelowCritical(TicketPriority priority)
    {
        var nextPriority = priority + 1;

        Assert.True(nextPriority > priority);
        Assert.True(nextPriority <= TicketPriority.Critical);
    }

    [Fact]
    public void BaseEntity_GeneratesUniqueId_OnCreation()
    {
        var ticket1 = new Ticket();
        var ticket2 = new Ticket();

        Assert.NotEqual(ticket1.Id, ticket2.Id);
    }
}