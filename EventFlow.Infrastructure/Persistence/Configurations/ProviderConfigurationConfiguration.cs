using EventFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventFlow.Infrastructure.Persistence.Configurations;

public class ProviderConfigurationConfiguration : IEntityTypeConfiguration<ProviderConfiguration>
{
    public void Configure(EntityTypeBuilder<ProviderConfiguration> builder)
    {
        builder.ToTable("ProviderConfigurations");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ApplicationId)
            .IsRequired();

        builder.Property(p => p.ProviderType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.IsEnabled)
            .IsRequired();

        builder.Property(p => p.Configuration)
            .HasColumnType("jsonb")
            .IsRequired();

        // Unique Index
        builder.HasIndex(p => new { p.ApplicationId, p.ProviderType, p.DisplayName })
            .IsUnique();

        // Relationship
        builder.HasOne(p => p.Application)
            .WithMany(a => a.ProviderConfigurations)
            .HasForeignKey(p => p.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
