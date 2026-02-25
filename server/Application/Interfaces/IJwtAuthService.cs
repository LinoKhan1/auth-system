using server.Domain.Entities;

namespace server.Application.Interfaces
{
    public interface IJwtAuthService
    {
        string GenerateToken(User user);
    }
}