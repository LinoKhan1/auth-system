using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using server.Application.Services;
using server.Configuration;
using server.Domain.Entities;

namespace server.tests.Services
{
    public class JwtAuthServiceTests
    {
        private readonly JwtSettings _jwtSettings;
        private readonly JwtAuthService _service;

        public JwtAuthServiceTests()
        {
            _jwtSettings = new JwtSettings
            {
                SecretKey = "THIS_IS_A_SUPER_SECRET_KEY_123456789",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpireHours = 1
            };

            var mockLogger = new Mock<ILogger<JwtAuthService>>();
            _service = new JwtAuthService(_jwtSettings, mockLogger.Object);
        }

        [Fact]
        public void GenerateToken_ShouldReturnValidJwtToken()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                FirstName = "Lino",
                LastName = "Khan"
            };

            var token = _service.GenerateToken(user);

            token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void GenerateToken_ShouldContainCorrectClaims()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                FirstName = "Lino",
                LastName = "Khan"
            };

            var token = _service.GenerateToken(user);

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            jwtToken.Claims.Should().Contain(c =>
                c.Type == ClaimTypes.NameIdentifier &&
                c.Value == user.Id.ToString());

            jwtToken.Claims.Should().Contain(c =>
                c.Type == ClaimTypes.Email &&
                c.Value == user.Email);

            jwtToken.Claims.Should().Contain(c =>
                c.Type == ClaimTypes.GivenName &&
                c.Value == user.FirstName);

            jwtToken.Claims.Should().Contain(c =>
                c.Type == ClaimTypes.Surname &&
                c.Value == user.LastName);
        }

        [Fact]
        public void GenerateToken_ShouldHaveCorrectIssuerAndAudience()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                FirstName = "Lino",
                LastName = "Khan"
            };

            var token = _service.GenerateToken(user);

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            jwtToken.Issuer.Should().Be(_jwtSettings.Issuer);
            jwtToken.Audiences.Should().Contain(_jwtSettings.Audience);
        }

        [Fact]
        public void GenerateToken_ShouldHaveValidExpiration()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                FirstName = "Lino",
                LastName = "Khan"
            };

            var before = DateTime.UtcNow;
            var token = _service.GenerateToken(user);

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            jwtToken.ValidTo.Should().BeAfter(before);
        }

        [Fact]
        public void GenerateToken_ShouldBeValidWithCorrectSigningKey()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                FirstName = "Lino",
                LastName = "Khan"
            };

            var token = _service.GenerateToken(user);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            Action act = () =>
                tokenHandler.ValidateToken(token, validationParameters, out _);

            act.Should().NotThrow();
        }
    }
}