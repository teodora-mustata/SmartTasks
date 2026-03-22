using SmartTasksAPI.Models;
using SmartTasksAPI.Models.Data;
using SmartTasksAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SmartTasksAPI.Tests.Repositories
{
    public class UserRepositoryTests
    {
        ApplicationDbContext _dbContext;
        DbContextOptions<ApplicationDbContext> _options;
        IUserRepository _repository;

        public UserRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ApplicationDbContext(_options);
            _repository = new UserRepository(_dbContext);
        }

        #region Test Helpers
        private void SetUpUsersInDbContext(List<User> users)
        {
            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();
        }
        #endregion

        #region
        [Fact]
        public async Task GetAllAsync_ReturnsDbUserList()
        {
            var usersList = new List<User>
            {
                new() { Id = Guid.NewGuid(), FullName = "User 1", Email = "test@test.com", CreatedAtUtc = DateTime.Now },
                new() { Id = Guid.NewGuid(), FullName = "User 2", Email = "test2@test.com", CreatedAtUtc = DateTime.Now },
            };
            SetUpUsersInDbContext(usersList);

            var result = await _repository.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(usersList.Count, result.Count());
        }
        #endregion

        #region GetByIdAsync Tests
        [Fact]
        public async Task GetByIdAsync_NoIdsMatch_NoResultsReturned()
        {
            var testUserId = Guid.NewGuid();
            var usersList = new List<User>
            {
                new() { Id = Guid.NewGuid(), FullName = "User 1", Email = "test@test.com", CreatedAtUtc = DateTime.Now },
                new() { Id = Guid.NewGuid(), FullName = "User 2", Email = "test2@test.com", CreatedAtUtc = DateTime.Now }
            };
            SetUpUsersInDbContext(usersList);

            var result = await _repository.GetByIdAsync(testUserId);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_UserWithId_IsReturned()
        {
            var testUserId = Guid.NewGuid();
            var usersList = new List<User>
            {
                new() { Id = Guid.NewGuid(), FullName = "User 1", Email = "test@test.com", CreatedAtUtc = DateTime.Now },
                new() { Id = testUserId, FullName = "User 2", Email = "test2@test.com", CreatedAtUtc = DateTime.Now }
            };
            SetUpUsersInDbContext(usersList);

            var result = await _repository.GetByIdAsync(testUserId);

            Assert.NotNull(result);
            Assert.Equal(testUserId, result.Id);
            Assert.Equal("User 2", result.FullName);
        }
        #endregion

        #region GetByEmailAsync Tests
        [Fact]
        public async Task GetByEmailAsync_NoUsersWithEmail_ReturnsNull()
        {
            var testEmail = "test1@test.com";
            var usersList = new List<User>
            {
                new() { Id = Guid.NewGuid(), FullName = "User 1", Email = "test_1@test.com", CreatedAtUtc = DateTime.Now },
                new() { Id = Guid.NewGuid(), FullName = "User 2", Email = "test_1_@test.com", CreatedAtUtc = DateTime.Now }
            };
            SetUpUsersInDbContext(usersList);

            var result = await _repository.GetByEmailAsync(testEmail);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByEmailAsync_UserWithEmail_IsReturned()
        {
            var testEmail = "test1@test.com";
            var usersList = new List<User>
            {
                new() { Id = Guid.NewGuid(), FullName = "User 1", Email = testEmail, CreatedAtUtc = DateTime.Now },
                new() { Id = Guid.NewGuid(), FullName = "User 2", Email = "test2@test.com", CreatedAtUtc = DateTime.Now }
            };
            SetUpUsersInDbContext(usersList);

            var result = await _repository.GetByEmailAsync(testEmail);

            Assert.NotNull(result);
            Assert.Equal(testEmail, result.Email);
        }
        #endregion

        #region AddAsync Tests
        [Fact]
        public async Task AddUser_NewUser_IsPersistedAndReturned()
        {
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                FullName = "New User",
                Email = $"newUser_{Guid.NewGuid():N}@test.com",
                CreatedAtUtc = DateTime.UtcNow
            };

            var result = await _repository.AddAsync(newUser);

            Assert.NotNull(result);
            Assert.Equal(newUser.Id, result.Id);
            Assert.Equal(newUser.Email, result.Email);

            // verify persistance in db
            var persisted = await _dbContext.Users.FindAsync(newUser.Id);
            Assert.NotNull(persisted);
            Assert.Equal(newUser.Email, persisted.Email);

            // confirm users increased following the add
            var count = _dbContext.Users.Count();
            Assert.True(count >= 1);
        }
        #endregion
    }
}
