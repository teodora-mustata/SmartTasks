using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartTasksAPI.Controllers;
using SmartTasksAPI.Contracts.Users;
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

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, FullName = "Jane", Email = "jane@test.com" };
        var userService = new Mock<IUserService>();
        userService.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        var controller = new UsersController(userService.Object);

        var result = await controller.GetById(userId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(user, okResult.Value);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var userService = new Mock<IUserService>();
        userService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        var controller = new UsersController(userService.Object);

        var result = await controller.GetById(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WhenUserIsCreated()
    {
        var userId = Guid.NewGuid();
        var created = new User { Id = userId, FullName = "Jane", Email = "jane@test.com" };
        var userService = new Mock<IUserService>();
        userService.Setup(x => x.CreateAsync("Jane", "jane@test.com")).ReturnsAsync(created);

        var controller = new UsersController(userService.Object);

        var result = await controller.Create(new CreateUserRequest { FullName = "Jane", Email = "jane@test.com" });

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(UsersController.GetById), createdResult.ActionName);
        Assert.Equal(userId, createdResult.RouteValues!["id"]);
        Assert.Same(created, createdResult.Value);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        var userService = new Mock<IUserService>();
        userService.Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("A user with this email already exists."));

        var controller = new UsersController(userService.Object);

        var result = await controller.Create(new CreateUserRequest { FullName = "Jane", Email = "jane@test.com" });

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Contains("A user with this email already exists.", conflict.Value!.ToString());
    }
}
