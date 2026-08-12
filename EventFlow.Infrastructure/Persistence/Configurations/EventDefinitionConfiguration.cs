using EventFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventFlow.Infrastructure.Persistence.Configurations;

public class EventDefinitionConfiguration : IEntityTypeConfiguration<EventDefinition>
{
    public void Configure(EntityTypeBuilder<EventDefinition> builder)
    {
        builder.ToTable("EventDefinitions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ApplicationId)
            .IsRequired();

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.IsActive)
            .IsRequired();

        // Unique Index
        builder.HasIndex(e => new { e.ApplicationId, e.Code })
            .IsUnique();

        // Relationships
        builder.HasOne(e => e.Application)
            .WithMany(a => a.EventDefinitions)
            .HasForeignKey(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.NotificationTemplates)
            .WithOne(t => t.EventDefinition)
            .HasForeignKey(t => t.EventDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.NotificationRules)
            .WithOne(r => r.EventDefinition)
            .HasForeignKey(r => r.EventDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.PublishedEvents)
            .WithOne(ev => ev.EventDefinition)
            .HasForeignKey(ev => ev.EventDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
