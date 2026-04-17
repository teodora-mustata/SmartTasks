using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartTasksAPI.Controllers;
//using SmartTasksAPI.Contracts.Users;
using SmartTasksAPI.Models;
using SmartTasksAPI.Services;

namespace SmartTasksAPI.Tests.Controllers;

public class UsersControllerTests
{
    [Fact]
    public async Task GetAll_ShouldReturnOk_WithUsers()
    {
        var users = new List<User> { new() { Id = Guid.NewGuid(), FullName = "Jane", Email = "jane@test.com" } };
        var userService = new Mock<IUserService>();
        userService.Setup(x => x.GetAllAsync()).ReturnsAsync(users);

        var controller = new UsersController(userService.Object);

        var result = await controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(users, okResult.Value);
    }

}
