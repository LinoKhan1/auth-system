using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using server.Configuration;
using server.Domain.Entities;
using server.Application.Interfaces;

namespace server.Application.Services
{
    public class JwtAuthService : IJwtAuthService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtAuthService> _logger;

        public JwtAuthService(JwtSettings jwtSettings, ILogger<JwtAuthService> logger)
        {
            _jwtSettings = jwtSettings;
            _logger = logger;
        }

        // -------------------------
        // GENERATE JWT TOKEN
        // -------------------------
        public string GenerateToken(User user)
        {
            try
            {
                _logger.LogInformation("Generating JWT token for user {UserId} ({Email})", user.Id, user.Email);

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.GivenName, user.FirstName),
                        new Claim(ClaimTypes.Surname, user.LastName)
                    }),
                    Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpireHours),
                    Issuer = _jwtSettings.Issuer,
                    Audience = _jwtSettings.Audience,
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature
                    )
                };

                // Create token
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwt = tokenHandler.WriteToken(token);

                _logger.LogInformation("JWT token generated successfully for user {UserId}", user.Id);

                return jwt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate JWT token for user {UserId} ({Email})", user.Id, user.Email);
                throw new Exception("Error generating JWT token", ex);
            }
        }
    }
}