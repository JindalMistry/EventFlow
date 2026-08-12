using EventFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventFlow.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.EventId)
            .IsRequired();

        builder.Property(n => n.NotificationRuleId)
            .IsRequired();

        builder.Property(n => n.NotificationTemplateId)
            .IsRequired();

        builder.Property(n => n.ProviderConfigurationId)
            .IsRequired();

        builder.Property(n => n.Channel)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(n => n.Recipient)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(n => n.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(n => n.QueuedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(n => n.Status);
        builder.HasIndex(n => n.Channel);
        builder.HasIndex(n => n.Recipient);

        // Unique
        builder.HasIndex(x => new
                    {
                        x.EventId,
                        x.NotificationRuleId
                    })
            .IsUnique();

        // Relationships
        builder.HasOne(n => n.Event)
            .WithMany(e => e.Notifications)
            .HasForeignKey(n => n.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.NotificationRule)
            .WithMany()
            .HasForeignKey(n => n.NotificationRuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.NotificationTemplate)
            .WithMany()
            .HasForeignKey(n => n.NotificationTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.ProviderConfiguration)
            .WithMany()
            .HasForeignKey(n => n.ProviderConfigurationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(n => n.NotificationAttempts)
            .WithOne(a => a.Notification)
            .HasForeignKey(a => a.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
