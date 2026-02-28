using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.Presentation.DTOs;
using server.Application.Interfaces;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

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
                Response.Cookies.Append("jwt", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // Set to true in production with HTTPS
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                });
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
        // LOGOUT USER
        // -------------------------
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            _logger.LogInformation("Logout endpoint called");

            Response.Cookies.Delete("jwt", new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Set to true in production with HTTPS
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

            _logger.LogInformation("User logged out successfully");
            return Ok(new { message = "Logged out successfully" });
        }

      
        // -------------------------
        // GET USER BY ID (JWT REQUIRED)
        // -------------------------
        [Authorize]
        [HttpGet("user/me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            _logger.LogInformation("GetCurrentUser endpoint called");

            try
            {
                // Use ClaimTypes instead of JwtRegisteredClaimNames
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogWarning("User ID claim not found in token");
                    return Unauthorized(new { message = "Invalid token" });
                }
                if (!Guid.TryParse(userIdClaim, out Guid userId))
                {
                    _logger.LogWarning("Invalid User ID format in token: {UserId}", userIdClaim);
                    return Unauthorized(new { message = "Invalid token" });
                }

                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return NotFound(new { message = "User not found" });
                }

                _logger.LogInformation("User retrieved successfully: {UserId}", userId);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}