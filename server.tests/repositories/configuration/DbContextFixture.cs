using server.Domain.Entities;
using server.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace server.tests.repositories.configuration
{
    public class DbContextFixture : IDisposable
    {
        public AppDbContext Context { get; private set; }

        public DbContextFixture()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            Context = new AppDbContext(options);
            SeedData();
        }

        private void SeedData()
        {
            var users = new List<User>
            {
                new User {FirstName ="test_first_name1", LastName = "test_last_name1", Email = "test_email1@test.com", PasswordHash = "hashed_password1"},
                new User {FirstName ="test_first_name2", LastName = "test_last_name2", Email = "test_email2@test.com", PasswordHash = "hashed_password2"}
            };

            Context.Users.AddRange(users);
            Context.SaveChanges();
        }
        public void Dispose()
        {
            Context.Dispose();
        }

    }

}