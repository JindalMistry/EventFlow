using EventFlow.Domain.Entities;

namespace EventFlow.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}
