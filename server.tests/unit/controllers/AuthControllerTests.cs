using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using server.Application.Interfaces;
using server.Presentation.Controllers;
using server.Presentation.DTOs;

namespace server.tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock = new();
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _controller = new AuthController(_userServiceMock.Object);
        }

        // =========================
        // REGISTER
        // =========================

        [Fact]
        public async Task Register_ShouldReturnOk_WhenSuccessful()
        {
            var request = new RegisterRequest
            {
                Email = "test@test.com",
                Password = "123456",
                FirstName = "Lino",
                LastName = "Khan"
            };

            var response = new UserResponse
            {
                Id = Guid.NewGuid(),
                Email = request.Email
            };

            _userServiceMock.Setup(s => s.RegisterAsync(request))
                            .ReturnsAsync(response);

            var result = await _controller.Register(request);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenExceptionThrown()
        {
            var request = new RegisterRequest
            {
                Email = "existing@test.com",
                Password = "123"
            };

            _userServiceMock.Setup(s => s.RegisterAsync(request))
                            .ThrowsAsync(new Exception("Failed to register user"));

            var result = await _controller.Register(request);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // =========================
        // LOGIN
        // =========================

        [Fact]
        public async Task Login_ShouldReturnOk_WithToken_WhenSuccessful()
        {
            var request = new LoginRequest
            {
                Email = "test@test.com",
                Password = "123456"
            };

            _userServiceMock.Setup(s => s.LoginAsync(request))
                            .ReturnsAsync("mocked-jwt-token");

            var result = await _controller.Login(request);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;

            okResult.Value.Should().BeEquivalentTo(new { token = "mocked-jwt-token" });
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenExceptionThrown()
        {
            var request = new LoginRequest
            {
                Email = "wrong@test.com",
                Password = "wrong"
            };

            _userServiceMock.Setup(s => s.LoginAsync(request))
                            .ThrowsAsync(new Exception("Failed to login"));

            var result = await _controller.Login(request);

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        // =========================
        // GET USER BY ID
        // =========================

        [Fact]
        public async Task GetUserById_ShouldReturnOk_WhenUserExists()
        {
            var userId = Guid.NewGuid();

            var response = new UserResponse
            {
                Id = userId,
                Email = "test@test.com"
            };

            _userServiceMock.Setup(s => s.GetUserByIdAsync(userId))
                            .ReturnsAsync(response);

            var result = await _controller.GetUserById(userId);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnBadRequest_WhenExceptionThrown()
        {
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(s => s.GetUserByIdAsync(userId))
                            .ThrowsAsync(new Exception("Failed to get user"));

            var result = await _controller.GetUserById(userId);

            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}