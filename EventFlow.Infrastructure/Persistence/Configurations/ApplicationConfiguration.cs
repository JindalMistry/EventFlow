using EventFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApplicationEntity = EventFlow.Domain.Entities.Application;

namespace EventFlow.Infrastructure.Persistence.Configurations;

public class ApplicationConfiguration : IEntityTypeConfiguration<ApplicationEntity>
{
    public void Configure(EntityTypeBuilder<ApplicationEntity> builder)
    {
        builder.ToTable("Applications");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Code)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Description)
            .HasMaxLength(500);

        builder.Property(a => a.SupportEmail)
            .HasMaxLength(256);

        builder.Property(a => a.ApiKeyHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.ApiKeyCreatedAt)
            .IsRequired();

        // Unique Indexes
        builder.HasIndex(a => a.Code)
            .IsUnique();

        builder.HasIndex(a => a.Name)
            .IsUnique();

        builder.HasIndex(a => a.ApiKeyHash)
            .IsUnique();

        // Relationships
        builder.HasMany(a => a.Users)
            .WithOne(u => u.Application)
            .HasForeignKey(u => u.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.ProviderConfigurations)
            .WithOne(p => p.Application)
            .HasForeignKey(p => p.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.EventDefinitions)
            .WithOne(e => e.Application)
            .HasForeignKey(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.NotificationTemplates)
            .WithOne(t => t.Application)
            .HasForeignKey(t => t.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.NotificationRules)
            .WithOne(r => r.Application)
            .HasForeignKey(r => r.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.PublishedEvents)
            .WithOne(e => e.Application)
            .HasForeignKey(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
