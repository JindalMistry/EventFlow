using EventFlow.Application.Exceptions;
using EventFlow.Application.Interfaces;
using EventFlow.Domain.Entities;
using EventFlow.Domain.Enums;
using EventFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Infrastructure.Services;

public class NotificationAttemptService : INotificationAttemptService
{
    private readonly EventFlowDbContext _dbContext;

    public NotificationAttemptService(EventFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<NotificationAttempt> CreateAsync(NotificationAttempt attempt, CancellationToken cancellationToken = default)
    {
        if (attempt == null)
        {
            throw new BadRequestException("Notification attempt cannot be null.");
        }

        if (attempt.NotificationId == Guid.Empty)
        {
            throw new BadRequestException("NotificationId is required.");
        }

        if (attempt.Id == Guid.Empty)
        {
            attempt.Id = Guid.NewGuid();
        }

        if (attempt.StartedAt == default)
        {
            attempt.StartedAt = DateTime.UtcNow;
        }

        if (attempt.CreatedAt == default)
        {
            attempt.CreatedAt = DateTime.UtcNow;
        }

        _dbContext.NotificationAttempts.Add(attempt);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return attempt;
    }

    public async Task<NotificationAttempt> CreateAsync(
        Guid notificationId,
        int attemptNumber,
        AttemptStatus status = AttemptStatus.Retrying,
        string? providerResponse = null,
        DateTime? startedAt = null,
        CancellationToken cancellationToken = default)
    {
        var attempt = new NotificationAttempt
        {
            Id = Guid.NewGuid(),
            NotificationId = notificationId,
            AttemptNumber = attemptNumber,
            Status = status,
            ProviderResponse = providerResponse,
            StartedAt = startedAt ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        return await CreateAsync(attempt, cancellationToken);
    }

    public async Task<NotificationAttempt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        return await _dbContext.NotificationAttempts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task CompleteAsync(
        Guid id,
        AttemptStatus status,
        string? errorMessage = null,
        string? providerResponse = null,
        DateTime? completedAt = null,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new BadRequestException("Notification attempt ID is required.");
        }

        var attempt = await _dbContext.NotificationAttempts
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (attempt == null)
        {
            throw new NotFoundException($"Notification attempt with id '{id}' was not found.");
        }

        attempt.Status = status;
        attempt.CompletedAt = completedAt ?? DateTime.UtcNow;
        attempt.ErrorMessage = errorMessage;
        if (providerResponse != null)
        {
            attempt.ProviderResponse = providerResponse;
        }

        attempt.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<NotificationAttempt?> GetLatestAttempByNotificationId(Guid NotificationId, CancellationToken cancellationToken = default)
    {
        var attempt = await _dbContext.NotificationAttempts
            .AsNoTracking()
            .Where(o => o.NotificationId == NotificationId)
            .OrderByDescending(o => o.AttemptNumber)
            .FirstOrDefaultAsync(cancellationToken);

        return attempt;
    }
}
