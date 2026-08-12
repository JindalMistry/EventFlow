using EventFlow.Application.Common;
using EventFlow.Application.DTOs.Applications;

namespace EventFlow.Application.Interfaces;

public interface IApplicationService
{
    Task<CreateApplicationResponse> CreateApplicationAsync(CreateApplicationRequest request);
    Task<ApplicationResponse> UpdateApplicationAsync(Guid id, UpdateApplicationRequest request);
    Task<PagedResponse<ApplicationResponse>> GetApplicationsAsync(ApplicationQueryParameters query);
    Task<ApplicationDetailResponse> GetApplicationByIdAsync(Guid id);
    Task DeleteApplicationAsync(Guid id);
    Task ActivateApplicationAsync(Guid id);
    Task DeactivateApplicationAsync(Guid id);
    Task<RotateApiKeyResponse> RotateApiKeyAsync(Guid id);
}
