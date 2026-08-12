using EventFlow.Application.Exceptions;
using EventFlow.Application.Interfaces;
using EventFlow.Domain.Entities;
using EventFlow.Domain.Enums;
using EventFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly EventFlowDbContext _dbContext;

    public NotificationService(EventFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Notification> CreateAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        if (notification == null)
        {
            throw new BadRequestException("Notification cannot be null.");
        }

        if (notification.EventId == Guid.Empty)
        {
            throw new BadRequestException("EventId is required.");
        }

        if (notification.NotificationRuleId == Guid.Empty)
        {
            throw new BadRequestException("NotificationRuleId is required.");
        }

        if (notification.NotificationTemplateId == Guid.Empty)
        {
            throw new BadRequestException("NotificationTemplateId is required.");
        }

        if (notification.ProviderConfigurationId == Guid.Empty)
        {
            throw new BadRequestException("ProviderConfigurationId is required.");
        }

        if (string.IsNullOrWhiteSpace(notification.Recipient))
        {
            throw new BadRequestException("Recipient is required.");
        }

        if (notification.Id == Guid.Empty)
        {
            notification.Id = Guid.NewGuid();
        }

        if (notification.QueuedAt == default)
        {
            notification.QueuedAt = DateTime.UtcNow;
        }

        if (notification.CreatedAt == default)
        {
            notification.CreatedAt = DateTime.UtcNow;
        }

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return notification;
    }

    public async Task<Notification> CreateAsync(
        Guid eventId,
        Guid notificationRuleId,
        Guid notificationTemplateId,
        Guid providerConfigurationId,
        NotificationChannel channel,
        string recipient,
        NotificationStatus status = NotificationStatus.Pending,
        DateTime? queuedAt = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            NotificationRuleId = notificationRuleId,
            NotificationTemplateId = notificationTemplateId,
            ProviderConfigurationId = providerConfigurationId,
            Channel = channel,
            Recipient = recipient,
            Status = status,
            QueuedAt = queuedAt ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        return await CreateAsync(notification, cancellationToken);
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        return await _dbContext.Notifications
            .AsNoTracking()
            .Include(n => n.NotificationRule)
            .Include(n => n.NotificationTemplate)
            .Include(n => n.ProviderConfiguration)
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task CompleteProcessingAsync(
        Guid id,
        NotificationStatus status,
        string? failureReason = null,
        DateTime? sentAt = null,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new BadRequestException("Notification ID is required.");
        }

        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

        if (notification == null)
        {
            throw new NotFoundException($"Notification with id '{id}' was not found.");
        }

        notification.Status = status;

        if (status == NotificationStatus.Succeeded)
        {
            notification.FailureReason = null;
            notification.SentAt = sentAt ?? DateTime.UtcNow;
        }
        else
        {
            notification.FailureReason = failureReason;
        }

        notification.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Notification?> GetByEventAndRuleAsync(
        Guid eventId,
        Guid notificationRuleId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.EventId == eventId && x.NotificationRuleId == notificationRuleId, cancellationToken);
    }

    public async Task UpdateStatusAsync(Guid Id, NotificationStatus Status, CancellationToken Token)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(o => o.Id == Id, Token);

        if (notification != null)
        {
            notification.Status = Status;
            await _dbContext.SaveChangesAsync(Token);
        }
    }
}
