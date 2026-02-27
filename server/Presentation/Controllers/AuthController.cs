using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using server.Presentation.DTOs;
using server.Application.Interfaces;

namespace server.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // -------------------------
        // REGISTER NEW USER
        // -------------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            _logger.LogInformation("Register endpoint called for email {Email}", request.Email);

            try
            {
                var userResponse = await _userService.RegisterAsync(request);
                _logger.LogInformation("User registered successfully: {Email}", request.Email);
                return Ok(userResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register user {Email}", request.Email);
                return BadRequest(new { message = ex.Message });
            }
        }

        // -------------------------
        // LOGIN USER
        // -------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Login endpoint called for email {Email}", request.Email);

            try
            {
                var token = await _userService.LoginAsync(request);
                _logger.LogInformation("User logged in successfully: {Email}", request.Email);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed login attempt for user {Email}", request.Email);
                return Unauthorized(new { message = ex.Message });
            }
        }

        // -------------------------
        // GET USER BY ID (JWT REQUIRED)
        // -------------------------
        [Authorize]
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            _logger.LogInformation("GetUserById endpoint called for user {UserId}", id);

            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", id);
                    return NotFound(new { message = "User not found" });
                }

                _logger.LogInformation("User retrieved successfully: {UserId}", id);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch user {UserId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}