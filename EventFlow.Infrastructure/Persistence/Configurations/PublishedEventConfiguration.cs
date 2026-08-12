using EventFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventFlow.Infrastructure.Persistence.Configurations;

public class PublishedEventConfiguration : IEntityTypeConfiguration<PublishedEvent>
{
    public void Configure(EntityTypeBuilder<PublishedEvent> builder)
    {
        builder.ToTable("PublishedEvents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ApplicationId)
            .IsRequired();

        builder.Property(e => e.EventDefinitionId)
            .IsRequired();

        builder.Property(e => e.CorrelationId)
            .IsRequired();

        builder.Property(e => e.Payload)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.OccurredAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(e => e.CorrelationId);
        builder.HasIndex(e => e.OccurredAt);
        builder.HasIndex(e => e.Status);

        // Relationships
        builder.HasOne(e => e.Application)
            .WithMany(a => a.PublishedEvents)
            .HasForeignKey(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.EventDefinition)
            .WithMany(ed => ed.PublishedEvents)
            .HasForeignKey(e => e.EventDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Notifications)
            .WithOne(n => n.Event)
            .HasForeignKey(n => n.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
