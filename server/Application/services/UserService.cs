// server/Services/UserService.cs
using AutoMapper;
using server.Domain.Entities;
using server.Presentation.DTOs;
using server.Infrastructure.Repository.Interfaces;
using server.Application.Interfaces;

namespace server.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IJwtAuthService _jwtService;
        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepo,
            IJwtAuthService jwtService,
            IMapper mapper)
        {
            _userRepo = userRepo;
            _jwtService = jwtService;
            _mapper = mapper;
        }

        public async Task<UserResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                if (await _userRepo.EmailExistsAsync(request.Email))
                    throw new Exception("Email already in use.");

                var user = _mapper.Map<User>(request);
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                var createdUser = await _userRepo.AddAsync(user);
                return _mapper.Map<UserResponse>(createdUser);
            }
            catch (Exception ex)
            {
                // Log exception here if needed
                throw new Exception("Failed to register user", ex);
            }
        }

        public async Task<string> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _userRepo.GetByEmailAsync(request.Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                    throw new Exception("Invalid email or password.");

                return _jwtService.GenerateToken(user);
            }
            catch (Exception ex)
            {
                // Log exception here if needed
                throw new Exception("Failed to login", ex);
            }
        }

        public async Task<UserResponse> GetUserByIdAsync (Guid id)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(id) ?? throw new Exception("User not found.");
                return _mapper.Map<UserResponse>(user);
            }
            catch (Exception ex)
            {
                // Log exception here if needed
                throw new Exception("Failed to get user", ex);
            }
        }
    }
}