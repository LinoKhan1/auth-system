using server.Presentation.DTOs;

namespace server.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserResponse> RegisterAsync(RegisterRequest request);
        Task<string> LoginAsync(LoginRequest request);
        Task<UserResponse> GetUserByIdAsync(Guid userId);
    }
}