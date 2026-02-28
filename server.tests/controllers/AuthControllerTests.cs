using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using server.Application.Interfaces;
using server.Presentation.Controllers;
using server.Presentation.DTOs;
using System.Security.Claims;

namespace server.tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock = new();
        private readonly Mock<ILogger<AuthController>> _loggerMock = new();
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _controller = new AuthController(_userServiceMock.Object, _loggerMock.Object);

            // Setup HttpContext for cookies and User claims
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
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
        public async Task Login_ShouldReturnOk_WithTokenAndSetCookie_WhenSuccessful()
        {
            var request = new LoginRequest { Email = "test@test.com", Password = "123456" };
            var token = "mocked-jwt-token";

            _userServiceMock.Setup(s => s.LoginAsync(request))
                            .ReturnsAsync(token);

            var cookiesMock = new Mock<IResponseCookies>();
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.Response.Cookies).Returns(cookiesMock.Object);
            _controller.ControllerContext.HttpContext = httpContextMock.Object;

            var result = await _controller.Login(request);
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { token });

            // Assert cookie was appended
            cookiesMock.Verify(c => c.Append(
                "jwt",
                token,
                It.IsAny<CookieOptions>()), Times.Once);
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
        // LOGOUT
        // =========================

        [Fact]
        public void Logout_ShouldReturnOk_AndDeleteCookie()
        {
            var cookiesMock = new Mock<IResponseCookies>();
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.Response.Cookies).Returns(cookiesMock.Object);
            _controller.ControllerContext.HttpContext = httpContextMock.Object;

            var result = _controller.Logout();

            result.Should().BeOfType<OkObjectResult>();

            cookiesMock.Verify(c => c.Delete(
                "jwt",
                It.IsAny<CookieOptions>()), Times.Once);
        }

        // =========================
        // GET CURRENT USER (/user/me)
        // =========================

        [Fact]
        public async Task GetCurrentUser_ShouldReturnOk_WhenUserExists()
        {
            var userId = Guid.NewGuid();

            var response = new UserResponse
            {
                Id = userId,
                Email = "test@test.com",
                FirstName = "Lino",
                LastName = "Khan"
            };

            _userServiceMock.Setup(s => s.GetUserByIdAsync(userId))
                            .ReturnsAsync(response);

            // Add User claims to HttpContext
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            _controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var result = await _controller.GetCurrentUser();

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task GetCurrentUser_ShouldReturnUnauthorized_WhenClaimMissing()
        {
            _controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // No claims

            var result = await _controller.GetCurrentUser();

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task GetCurrentUser_ShouldReturnUnauthorized_WhenInvalidGuid()
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "invalid-guid") };
            _controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var result = await _controller.GetCurrentUser();

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task GetCurrentUser_ShouldReturnNotFound_WhenUserNotExist()
        {
            var userId = Guid.NewGuid();

           _userServiceMock.Setup(s => s.GetUserByIdAsync(userId))
                .Returns(Task.FromResult<UserResponse?>(null));

            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            _controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var result = await _controller.GetCurrentUser();

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}