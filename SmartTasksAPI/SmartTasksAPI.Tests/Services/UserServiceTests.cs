using Moq;
using SmartTasksAPI.Models;
using SmartTasksAPI.Repositories;
using SmartTasksAPI.Services;

namespace SmartTasksAPI.Tests.Services
{
    public class UserServiceTests
    {
        Mock<IUserRepository> _userRepository = null!;
        IUserService _service = null!;

        public UserServiceTests()
        {
            _userRepository = new Mock<IUserRepository>();
            _service = new UserService(_userRepository.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateUser_WhenEmailDoesNotExist()
        {
            _userRepository.Setup(x => x.GetByEmailAsync("john@example.com"))
                .ReturnsAsync((User?)null);
            _userRepository.Setup(x => x.AddAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            var result = await _service.CreateAsync("John Doe", "JOHN@EXAMPLE.COM");

            Assert.Equal("John Doe", result.FullName);
            Assert.Equal("john@example.com", result.Email);
            _userRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenEmailAlreadyExists()
        {
            _userRepository.Setup(x => x.GetByEmailAsync("john@example.com"))
                .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = "john@example.com", FullName = "Existing" });

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateAsync("John Doe", "john@example.com"));
        }

        [Fact]
        public async Task GetAllAsync_CallsRepositoryMethod()
        {
            _userRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<User>());

            var result = await _service.GetAllAsync();

            Assert.NotNull(result);
            _userRepository.Verify(x => x.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_CallsRepositoryMethod()
        {
            var userId = Guid.NewGuid();
            _userRepository.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(new User { Id = userId, Email = "test@test.com", FullName = "Test User" });

            var result = await _service.GetByIdAsync(userId);

            Assert.NotNull(result);
            _userRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
        }
    }
}
