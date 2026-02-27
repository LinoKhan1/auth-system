using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using server.Presentation.DTOs;

namespace server.tests.Integration
{
    public class AuthIntegrationTests 
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_ShouldReturnOk_WhenValidRequest()
        {
            var request = new RegisterRequest
            {
                Email = "integration@test.com",
                Password = "123456",
                FirstName = "Lino",
                LastName = "Khan"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/auth/register", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadFromJsonAsync<UserResponse>();
            content.Should().NotBeNull();
            content!.Email.Should().Be(request.Email);
        }

        [Fact]
        public async Task Login_ShouldReturnToken_WhenValidCredentials()
        {
            // First register
            var register = new RegisterRequest
            {
                Email = "login@test.com",
                Password = "123456",
                FirstName = "Lino",
                LastName = "Khan"
            };

            await _client.PostAsJsonAsync("/api/auth/register", register);

            // Then login
            var login = new LoginRequest
            {
                Email = register.Email,
                Password = register.Password
            };

            var response = await _client.PostAsJsonAsync(
                "/api/auth/login", login);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("token");
        }

        [Fact]
        public async Task GetUser_ShouldReturnUnauthorized_WhenNoToken()
        {
            var response = await _client.GetAsync(
                $"/api/auth/user/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}