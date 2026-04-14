using Moq;
using SmartTasksAPI.Models;
using SmartTasksAPI.Repositories;
using SmartTasksAPI.Services;

namespace SmartTasksAPI.Tests.Services
{


    public class UserServiceTests
    {
        [Fact]
        public async Task GetAllAsync_ShouldReturnUsersFromRepository()
        {
            var expected = new List<User> { new() { Id = Guid.NewGuid(), FullName = "Jane", Email = "jane@test.com" } };
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(expected);

            var service = new UserService(userRepository.Object);

            var result = await service.GetAllAsync();

            Assert.Same(expected, result);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUserFromRepository()
        {
            var userId = Guid.NewGuid();
            var expected = new User { Id = userId, FullName = "Jane", Email = "jane@test.com" };
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(expected);

            var service = new UserService(userRepository.Object);

            var result = await service.GetByIdAsync(userId);

            Assert.Same(expected, result);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateUser_WhenEmailDoesNotExist()
        {
            var userRepository = new Mock<IUserRepository>();
            User? addedUser = null;

            userRepository.Setup(x => x.GetByEmailAsync(" JOHN@EXAMPLE.COM "))
                .ReturnsAsync((User?)null);
            userRepository.Setup(x => x.AddAsync(It.IsAny<User>()))
                .Callback<User>(user => addedUser = user)
                .ReturnsAsync((User u) => u);

            var service = new UserService(userRepository.Object);

            var result = await service.CreateAsync("  John Doe  ", " JOHN@EXAMPLE.COM ");

            Assert.Equal("John Doe", result.FullName);
            Assert.Equal("john@example.com", result.Email);
            Assert.NotNull(addedUser);
            Assert.Equal("John Doe", addedUser!.FullName);
            Assert.Equal("john@example.com", addedUser.Email);
            userRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenEmailAlreadyExists()
        {
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(x => x.GetByEmailAsync("john@example.com"))
                .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = "john@example.com", FullName = "Existing" });

            var service = new UserService(userRepository.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateAsync("John Doe", "john@example.com"));
        }
    }

    public class UserServiceTestsOriginal
    {
        [Fact]
        public async Task CreateAsync_ShouldCreateUser_WhenEmailDoesNotExist_Original()
        {
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(x => x.GetByEmailAsync("john@example.com"))
                .ReturnsAsync((User?)null);
            userRepository.Setup(x => x.AddAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            var service = new UserService(userRepository.Object);

            var result = await service.CreateAsync("John Doe", "JOHN@EXAMPLE.COM");

            Assert.Equal("John Doe", result.FullName);
            Assert.Equal("john@example.com", result.Email);
            userRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenEmailAlreadyExists_Original()
        {
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(x => x.GetByEmailAsync("john@example.com"))
                .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = "john@example.com", FullName = "Existing" });

            var service = new UserService(userRepository.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateAsync("John Doe", "john@example.com"));
        }
    }


}
