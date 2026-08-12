using EventFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventFlow.Infrastructure.Persistence.Configurations;

public class NotificationRuleConfiguration : IEntityTypeConfiguration<NotificationRule>
{
    public void Configure(EntityTypeBuilder<NotificationRule> builder)
    {
        builder.ToTable("NotificationRules");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ApplicationId)
            .IsRequired();

        builder.Property(r => r.EventDefinitionId)
            .IsRequired();

        builder.Property(r => r.NotificationTemplateId)
            .IsRequired();

        builder.Property(r => r.ProviderConfigurationId)
            .IsRequired();

        builder.Property(r => r.Channel)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.IsEnabled)
            .IsRequired();

        builder.Property(r => r.RetryCount)
            .IsRequired();

        builder.Property(r => r.DelayInSeconds)
            .IsRequired();

        builder.Property(r => r.Priority)
            .IsRequired();

        // Unique Index
        builder.HasIndex(r => new { r.ApplicationId, r.EventDefinitionId, r.Channel })
            .IsUnique();

        // Relationships
        builder.HasOne(r => r.Application)
            .WithMany(a => a.NotificationRules)
            .HasForeignKey(r => r.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.EventDefinition)
            .WithMany(e => e.NotificationRules)
            .HasForeignKey(r => r.EventDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.NotificationTemplate)
            .WithMany()
            .HasForeignKey(r => r.NotificationTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ProviderConfiguration)
            .WithMany()
            .HasForeignKey(r => r.ProviderConfigurationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
