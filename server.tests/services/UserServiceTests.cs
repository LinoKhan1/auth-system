using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using server.Application.Interfaces;
using server.Application.Services;
using server.Domain.Entities;
using server.Infrastructure.Repository.Interfaces;
using server.Presentation.DTOs;

namespace server.tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _repoMock = new();
        private readonly Mock<IJwtAuthService> _jwtMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<ILogger<UserService>> _loggerMock = new();
        private readonly UserService _service;

        public UserServiceTests()
        {
            _service = new UserService(
                _repoMock.Object,
                _jwtMock.Object,
                _mapperMock.Object,
                _loggerMock.Object
            );
        }

        // REGISTER
        [Fact]
        // should create user when email does not exist, otherwise should throw exception
        public async Task RegisterAsync_ShouldCreateUser_WhenEmailNotExists()
        {
            var request = new RegisterRequest
            {
                Email = "test@test.com",
                Password = "123456",
                FirstName = "Lino",
                LastName = "Khan"
            };

            var userEntity = new User { Email = request.Email };
            var createdUser = new User { Id = Guid.NewGuid(), Email = request.Email };
            var responseDto = new UserResponse { Id = createdUser.Id, Email = createdUser.Email };

            _repoMock.Setup(r => r.EmailExistsAsync(request.Email))
                     .ReturnsAsync(false);

            _mapperMock.Setup(m => m.Map<User>(request))
                       .Returns(userEntity);

            _repoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                     .ReturnsAsync(createdUser);

            _mapperMock.Setup(m => m.Map<UserResponse>(createdUser))
                       .Returns(responseDto);

            var result = await _service.RegisterAsync(request);

            result.Should().NotBeNull();
            result.Email.Should().Be(request.Email);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrow_WhenEmailExists()
        {
            var request = new RegisterRequest
            {
                Email = "existing@test.com",
                Password = "123"
            };

            _repoMock.Setup(r => r.EmailExistsAsync(request.Email))
                     .ReturnsAsync(true);

            var act = async () => await _service.RegisterAsync(request);

            await act.Should()
                     .ThrowAsync<Exception>()
                     .WithMessage("Failed to register user");
        }

        // =========================
        // LOGIN
        // =========================

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsValid()
        {
            var password = "123456";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var request = new LoginRequest
            {
                Email = "test@test.com",
                Password = password
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = hashedPassword
            };

            _repoMock.Setup(r => r.GetByEmailAsync(request.Email))
                     .ReturnsAsync(user);

            _jwtMock.Setup(j => j.GenerateToken(user))
                    .Returns("mocked-jwt-token");

            var result = await _service.LoginAsync(request);

            result.Should().Be("mocked-jwt-token");
        }

        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenInvalidCredentials()
        {
            var request = new LoginRequest
            {
                Email = "wrong@test.com",
                Password = "wrong"
            };

            _repoMock.Setup(r => r.GetByEmailAsync(request.Email))
                     .ReturnsAsync((User?)null);

            var act = async () => await _service.LoginAsync(request);

            await act.Should()
                     .ThrowAsync<Exception>()
                     .WithMessage("Failed to login");
        }

        // =========================
        // GET USER
        // =========================

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = "test@test.com" };
            var response = new UserResponse { Id = userId, Email = "test@test.com" };

            _repoMock.Setup(r => r.GetByIdAsync(userId))
                     .ReturnsAsync(user);

            _mapperMock.Setup(m => m.Map<UserResponse>(user))
                       .Returns(response);

            var result = await _service.GetUserByIdAsync(userId);

            result.Should().NotBeNull();
            result.Id.Should().Be(userId);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldThrow_WhenUserNotFound()
        {
            var userId = Guid.NewGuid();

            _repoMock.Setup(r => r.GetByIdAsync(userId))
                     .ReturnsAsync((User?)null);

            var act = async () => await _service.GetUserByIdAsync(userId);

            await act.Should()
                     .ThrowAsync<Exception>()
                     .WithMessage("Failed to get user");
        }
    }
}