using EventFlow.Application.Common;
using EventFlow.Application.DTOs.NotificationRules;

namespace EventFlow.Application.Interfaces;

public interface INotificationRuleService
{
    Task<NotificationRuleDetailResponse> CreateNotificationRuleAsync(CreateNotificationRuleRequest request);
    Task<PagedResponse<NotificationRuleResponse>> GetNotificationRulesAsync(NotificationRuleQueryParameters query);
    Task<NotificationRuleDetailResponse> GetNotificationRuleByIdAsync(Guid id);
    Task<NotificationRuleDetailResponse> UpdateNotificationRuleAsync(Guid id, UpdateNotificationRuleRequest request);
    Task<NotificationRuleDetailResponse> EnableNotificationRuleAsync(Guid id);
    Task<NotificationRuleDetailResponse> DisableNotificationRuleAsync(Guid id);
    Task DeleteNotificationRuleAsync(Guid id);
    Task<List<NotificationRuleDetailResponse>> GetNotificationRulesByEventIdAsync(Guid id, CancellationToken token);
}
