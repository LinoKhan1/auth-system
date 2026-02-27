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
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepo,
            IJwtAuthService jwtService,
            IMapper mapper,
            ILogger<UserService> logger)
        {
            _userRepo = userRepo;
            _jwtService = jwtService;
            _mapper = mapper;
            _logger = logger;
        }

        // -------------------------
        // REGISTER NEW USER
        // -------------------------
        public async Task<UserResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("Attempting to register user with email {Email}", request.Email);

                // Check if email already exists
                if (await _userRepo.EmailExistsAsync(request.Email))
                {
                    _logger.LogWarning("Registration failed: email already in use {Email}", request.Email);
                    throw new Exception("Email already in use.");
                }

                // Map DTO to entity and hash password
                var user = _mapper.Map<User>(request);
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // Add user to database
                var createdUser = await _userRepo.AddAsync(user);
                _logger.LogInformation("User registered successfully: {Email}", request.Email);

                return _mapper.Map<UserResponse>(createdUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering user {Email}", request.Email);
                throw new Exception("Failed to register user", ex);
            }
        }

        // -------------------------
        // LOGIN USER
        // -------------------------
        public async Task<string> LoginAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Attempting login for user {Email}", request.Email);

                var user = await _userRepo.GetByEmailAsync(request.Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed for user {Email}: invalid credentials", request.Email);
                    throw new Exception("Invalid email or password.");
                }

                var token = _jwtService.GenerateToken(user);
                _logger.LogInformation("User logged in successfully: {Email}", request.Email);
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for user {Email}", request.Email);
                throw new Exception("Failed to login", ex);
            }
        }

        // -------------------------
        // GET USER BY ID
        // -------------------------
        public async Task<UserResponse> GetUserByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching user by ID {UserId}", id);

                var user = await _userRepo.GetByIdAsync(id) ?? throw new Exception("User not found.");
                _logger.LogInformation("User retrieved successfully: {UserId}", id);

                return _mapper.Map<UserResponse>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching user by ID {UserId}", id);
                throw new Exception("Failed to get user", ex);
            }
        }
    }
}