using Microsoft.EntityFrameworkCore;

namespace SolarflowServer.Tests.Data
{
    public class ApplicationDbContextTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public void Can_Create_Database_InMemory()
        {
            // Arrange & Act
            var context = GetDbContext();
            context.Database.EnsureCreated();

            // Assert
            Assert.True(context.Database.IsInMemory());
        }

        [Fact]
        public void Can_Insert_And_Retrieve_User()
        {
            // Arrange
            var context = GetDbContext();
            var user = new ApplicationUser
            {
                Id = 999,
                Email = "test@example.com",
                Fullname = "Test User"
            };

            // Act
            context.Users.Add(user);
            context.SaveChanges();
            var retrievedUser = context.Users.Find(1);

            // Assert
            Assert.NotNull(retrievedUser);
            Assert.Equal("test@example.com", retrievedUser.Email);
        }

        [Fact]
        public void Default_Values_Are_Set_Correctly()
        {
            // Arrange
            var context = GetDbContext();
            var user = new ApplicationUser
            {
                Id = 1,
                Email = "test@example.com",
                Fullname = "Test User"
            };

            // Act
            context.Users.Add(user);
            context.SaveChanges();
            var retrievedUser = context.Users.Find(1);

            // Assert
            Assert.NotNull(retrievedUser);
            Assert.Equal(false, retrievedUser.ConfirmedEmail); 
        }

        [Fact]
        public void Can_Delete_User()
        {
            // Arrange
            var context = GetDbContext();
            var user = new ApplicationUser
            {
                Id = 99,
                Email = "delete@example.com",
                Fullname = "User To Delete"
            };

            context.Users.Add(user);
            context.SaveChanges();

            // Act
            context.Users.Remove(user);
            context.SaveChanges();

            var deletedUser = context.Users.Find(99);

            // Assert
            Assert.Null(deletedUser); 
        }
    }
}
