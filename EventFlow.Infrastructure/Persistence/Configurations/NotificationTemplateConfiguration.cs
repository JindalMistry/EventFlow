using EventFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventFlow.Infrastructure.Persistence.Configurations;

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("NotificationTemplates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.ApplicationId)
            .IsRequired();

        builder.Property(t => t.EventDefinitionId)
            .IsRequired();

        builder.Property(t => t.Channel)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.Subject)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.Body)
            .IsRequired();

        builder.Property(t => t.IsActive)
            .IsRequired();

        builder.Property(t => t.Version)
            .IsRequired();

        // Unique Index
        builder.HasIndex(t => new { t.ApplicationId, t.EventDefinitionId, t.Channel, t.Version })
            .IsUnique();

        // Relationships
        builder.HasOne(t => t.Application)
            .WithMany(a => a.NotificationTemplates)
            .HasForeignKey(t => t.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.EventDefinition)
            .WithMany(e => e.NotificationTemplates)
            .HasForeignKey(t => t.EventDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
