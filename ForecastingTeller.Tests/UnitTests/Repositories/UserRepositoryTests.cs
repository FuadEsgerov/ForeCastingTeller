// ForecastingTeller.Tests/UnitTests/Repositories/UserRepositoryTests.cs
using ForecastingTeller.API.Data;
using ForecastingTeller.API.Models;
using ForecastingTeller.API.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ForecastingTeller.Tests.UnitTests.Repositories
{
    public class UserRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

        public UserRepositoryTests()
        {
            // Use in-memory database for testing
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"ForecastingTellerDb_{Guid.NewGuid()}")
                .Options;

            // Seed the database
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                context.Database.EnsureCreated();
                SeedDatabase(context);
            }
        }

        private void SeedDatabase(ApplicationDbContext context)
        {
            // Add test users
            var user1 = new User
            {
                Id = Guid.Parse("f9168c5e-ceb2-4faa-b6bf-329bf39fa1e4"),
                Username = "testuser1",
                Email = "test1@example.com",
                PasswordHash = "hashedPassword1",
                Salt = "salt1",
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                LastLoginAt = DateTime.UtcNow.AddDays(-1)
            };

            var user2 = new User
            {
                Id = Guid.Parse("1c3de5ef-458b-4b9c-b21a-545d84c342fb"),
                Username = "testuser2",
                Email = "test2@example.com",
                PasswordHash = "hashedPassword2",
                Salt = "salt2",
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                LastLoginAt = DateTime.UtcNow.AddDays(-2)
            };

            // Add profiles for the users
            user1.Profile = new UserProfile
            {
                Id = Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7"),
                UserId = user1.Id,
                FullName = "Test User One",
                Bio = "This is a test user profile."
            };

            user2.Profile = new UserProfile
            {
                Id = Guid.Parse("3d490a70-94ce-4d15-9494-5248280c2ce3"),
                UserId = user2.Id,
                FullName = "Test User Two"
            };

            context.Users.Add(user1);
            context.Users.Add(user2);
            context.SaveChanges();
        }

        [Fact]
        public async Task GetByIdAsync_ExistingUser_ReturnsUser()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var repository = new UserRepository(context);
            var userId = Guid.Parse("f9168c5e-ceb2-4faa-b6bf-329bf39fa1e4");

            // Act
            var user = await repository.GetByIdAsync(userId);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(userId, user.Id);
            Assert.Equal("testuser1", user.Username);
            Assert.Equal("test1@example.com", user.Email);
            Assert.NotNull(user.Profile);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingUser_ReturnsNull()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var repository = new UserRepository(context);
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000000");

            // Act
            var user = await repository.GetByIdAsync(userId);

            // Assert
            Assert.Null(user);
        }

        [Fact]
        public async Task GetByEmailAsync_ExistingEmail_ReturnsUser()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var repository = new UserRepository(context);
            var email = "test1@example.com";

            // Act
            var user = await repository.GetByEmailAsync(email);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(email, user.Email);
            Assert.Equal("testuser1", user.Username);
        }

        [Fact]
        public async Task GetByEmailAsync_NonExistingEmail_ReturnsNull()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var repository = new UserRepository(context);
            var email = "nonexistent@example.com";

            // Act
            var user = await repository.GetByEmailAsync(email);

            // Assert
            Assert.Null(user);
        }

        [Fact]
        public async Task CreateAsync_ValidUser_CreatesAndReturnsUser()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var repository = new UserRepository(context);
            var newUser = new User
            {
                Username = "newuser",
                Email = "new@example.com",
                PasswordHash = "hashedPassword",
                Salt = "salt",
                Profile = new UserProfile { FullName = "New User" }
            };

            // Act
            var createdUser = await repository.CreateAsync(newUser);

            // Assert
            Assert.NotNull(createdUser);
            Assert.Equal(newUser.Username, createdUser.Username);
            Assert.Equal(newUser.Email, createdUser.Email);

            // Verify user was added to database
            var dbUser = await context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Email == newUser.Email);
            Assert.NotNull(dbUser);
            Assert.Equal(newUser.Username, dbUser.Username);
            Assert.NotNull(dbUser.Profile);
            Assert.Equal("New User", dbUser.Profile.FullName);
        }

        [Fact]
        public async Task UpdateAsync_ExistingUser_UpdatesAndReturnsUser()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var repository = new UserRepository(context);
            var userId = Guid.Parse("f9168c5e-ceb2-4faa-b6bf-329bf39fa1e4");
            
            var user = await repository.GetByIdAsync(userId);
            Assert.NotNull(user);
            
            // Update user properties
            user.Username = "updatedusername";
            user.Email = "updated@example.com";
            user.Profile.FullName = "Updated User Name";

            // Act
            var updatedUser = await repository.UpdateAsync(user);

            // Assert
            Assert.NotNull(updatedUser);
            Assert.Equal("updatedusername", updatedUser.Username);
            Assert.Equal("updated@example.com", updatedUser.Email);
            Assert.Equal("Updated User Name", updatedUser.Profile.FullName);

            // Verify user was updated in database
            var dbUser = await context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == userId);
            Assert.NotNull(dbUser);
            Assert.Equal("updatedusername", dbUser.Username);
            Assert.Equal("updated@example.com", dbUser.Email);
            Assert.Equal("Updated User Name", dbUser.Profile.FullName);
        }

        [Fact]
        public async Task DeleteAsync_ExistingUser_RemovesUserAndReturnsTrue()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var repository = new UserRepository(context);
            var userId = Guid.Parse("1c3de5ef-458b-4b9c-b21a-545d84c342fb");

            // Act
            var result = await repository.DeleteAsync(userId);

            // Assert
            Assert.True(result);
            
            // Verify user was removed from database
            var dbUser = await context.Users.FindAsync(userId);
            Assert.Null(dbUser);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingUser_ReturnsFalse()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var repository = new UserRepository(context);
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000000");

            // Act
            var result = await repository.DeleteAsync(userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByEmailAsync_ExistingEmail_ReturnsTrue()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var repository = new UserRepository(context);
            var email = "test1@example.com";

            // Act
            var result = await repository.ExistsByEmailAsync(email);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByEmailAsync_NonExistingEmail_ReturnsFalse()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var repository = new UserRepository(context);
            var email = "nonexistent@example.com";

            // Act
            var result = await repository.ExistsByEmailAsync(email);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByUsernameAsync_ExistingUsername_ReturnsTrue()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var repository = new UserRepository(context);
            var username = "testuser1";

            // Act
            var result = await repository.ExistsByUsernameAsync(username);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByUsernameAsync_NonExistingUsername_ReturnsFalse()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var repository = new UserRepository(context);
            var username = "nonexistentuser";

            // Act
            var result = await repository.ExistsByUsernameAsync(username);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetUserProfileAsync_ExistingUserId_ReturnsProfile()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var repository = new UserRepository(context);
            var userId = Guid.Parse("f9168c5e-ceb2-4faa-b6bf-329bf39fa1e4");

            // Act
            var profile = await repository.GetUserProfileAsync(userId);

            // Assert
            Assert.NotNull(profile);
            Assert.Equal(userId, profile.UserId);
            Assert.Equal("Test User One", profile.FullName);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_ExistingProfile_UpdatesAndReturnsProfile()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var repository = new UserRepository(context);
            var userId = Guid.Parse("f9168c5e-ceb2-4faa-b6bf-329bf39fa1e4");
            
            var profile = await repository.GetUserProfileAsync(userId);
            Assert.NotNull(profile);
            
            // Update profile properties
            profile.FullName = "Updated Full Name";
            profile.Bio = "Updated bio information";
            profile.ZodiacSign = "Leo";

            // Act
            var updatedProfile = await repository.UpdateUserProfileAsync(profile);

            // Assert
            Assert.NotNull(updatedProfile);
            Assert.Equal("Updated Full Name", updatedProfile.FullName);
            Assert.Equal("Updated bio information", updatedProfile.Bio);
            Assert.Equal("Leo", updatedProfile.ZodiacSign);

            // Verify profile was updated in database
            var dbProfile = await context.UserProfiles.FindAsync(profile.Id);
            Assert.NotNull(dbProfile);
            Assert.Equal("Updated Full Name", dbProfile.FullName);
            Assert.Equal("Updated bio information", dbProfile.Bio);
            Assert.Equal("Leo", dbProfile.ZodiacSign);
        }
    }
    }