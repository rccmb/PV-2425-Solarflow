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

        [Fact]
        public void Can_Create_User_With_ViewAccount()
        {
            // Arrange
            var context = GetDbContext();
            var user = new ApplicationUser
            {
                Id = 6,
                Email = "user@example.com",
                Fullname = "Test User"
            };

            var viewAccount = new ViewAccount
            {
                Id = 1,
                Name = "Test View Account",
                UserId = user.Id,
                User = user
            };

            // Act
            context.Users.Add(user);
            context.ViewAccounts.Add(viewAccount);
            context.SaveChanges();

            var retrievedUser = context.Users.Include(u => u.ViewAccount).FirstOrDefault(u => u.Id == 6);

            // Assert
            Assert.NotNull(retrievedUser);
            Assert.NotNull(retrievedUser.ViewAccount);
            Assert.Equal("Test View Account", retrievedUser.ViewAccount.Name);
        }

        [Fact]
        public void Deleting_User_Should_Delete_ViewAccount()
        {
            // Arrange
            var context = GetDbContext();
            var user = new ApplicationUser
            {
                Id = 99,
                Email = "user@example.com",
                Fullname = "Test User"
            };

            var viewAccount = new ViewAccount
            {
                Id = 2,
                Name = "Test View Account",
                UserId = user.Id,
                User = user
            };

            context.Users.Add(user);
            context.ViewAccounts.Add(viewAccount);
            context.SaveChanges();

            // Act
            context.Users.Remove(user);
            context.SaveChanges();

            var retrievedViewAccount = context.ViewAccounts.Find(2);

            // Assert
            Assert.Null(retrievedViewAccount); 
        }



    }
}
