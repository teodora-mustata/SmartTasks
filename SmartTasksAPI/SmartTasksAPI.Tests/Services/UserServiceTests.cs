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

    }
}
