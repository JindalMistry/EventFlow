using EventFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ApplicationEntity = EventFlow.Domain.Entities.Application;

namespace EventFlow.Infrastructure.Persistence;

public class EventFlowDbContext : DbContext
{
    public EventFlowDbContext(DbContextOptions<EventFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<ApplicationEntity> Applications => Set<ApplicationEntity>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ProviderConfiguration> ProviderConfigurations => Set<ProviderConfiguration>();
    public DbSet<EventDefinition> EventDefinitions => Set<EventDefinition>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<NotificationRule> NotificationRules => Set<NotificationRule>();
    public DbSet<PublishedEvent> PublishedEvents => Set<PublishedEvent>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationAttempt> NotificationAttempts => Set<NotificationAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventFlowDbContext).Assembly);
    }
}
