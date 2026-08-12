using EventFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventFlow.Infrastructure.Persistence.Configurations;

public class NotificationAttemptConfiguration : IEntityTypeConfiguration<NotificationAttempt>
{
    public void Configure(EntityTypeBuilder<NotificationAttempt> builder)
    {
        builder.ToTable("NotificationAttempts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.NotificationId)
            .IsRequired();

        builder.Property(a => a.AttemptNumber)
            .IsRequired();

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.StartedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(a => a.NotificationId);
        builder.HasIndex(a => a.AttemptNumber);

        // Relationship
        builder.HasOne(a => a.Notification)
            .WithMany(n => n.NotificationAttempts)
            .HasForeignKey(a => a.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
