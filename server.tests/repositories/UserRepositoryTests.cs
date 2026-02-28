using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using server.Domain.Entities;
using server.Infrastructure.Repository;
using server.tests.repositories.configuration;

namespace server.tests.repositories
{
    [Collection("DbCollection")]
    public class UserRepositoryTests
    {
        private readonly UserRepository _repository;
        private readonly DbContextFixture _fixture;

        public UserRepositoryTests(DbContextFixture fixture)
        {
            _fixture = fixture;
            var loggerMock = new Mock<ILogger<UserRepository>>();
            _repository = new UserRepository(_fixture.Context, loggerMock.Object);
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnUser_WhenEmailExists()
        {
            // Arrange
            var email = "test_email1@test.com";

            // Act
            var result = await _repository.GetByEmailAsync(email);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be(email);
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailDoesNotExist()
        {
            var result = await _repository.GetByEmailAsync("notfound@test.com");

            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_ShouldAddUser()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "New",
                LastName = "User",
                Email = "new@test.com",
                PasswordHash = "hash"
            };

            // Act
            var result = await _repository.AddAsync(user);

            // Assert
            result.Should().NotBeNull();
            _fixture.Context.Users.Should().Contain(u => u.Email == "new@test.com");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            var existingUser = _fixture.Context.Users.First();

            var result = await _repository.GetByIdAsync(existingUser.Id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(existingUser.Id);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateUser()
        {
            var user = _fixture.Context.Users.First();
            user.FirstName = "UpdatedName";

            await _repository.UpdateAsync(user);

            var updatedUser = await _repository.GetByIdAsync(user.Id);

            updatedUser!.FirstName.Should().Be("UpdatedName");
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnTrue_WhenUserExists()
        {
            var user = _fixture.Context.Users.First();

            var result = await _repository.DeleteAsync(user.Id);

            result.Should().BeTrue();
            _fixture.Context.Users.Should().NotContain(u => u.Id == user.Id);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            var result = await _repository.DeleteAsync(Guid.NewGuid());

            result.Should().BeFalse();
        }

        [Fact]
        public async Task EmailExistsAsync_ShouldReturnTrue_WhenEmailExists()
        {
            var result = await _repository.EmailExistsAsync("test_email1@test.com");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task EmailExistsAsync_ShouldReturnFalse_WhenEmailDoesNotExist()
        {
            var result = await _repository.EmailExistsAsync("notfound@test.com");

            result.Should().BeFalse();
        }
    }
}