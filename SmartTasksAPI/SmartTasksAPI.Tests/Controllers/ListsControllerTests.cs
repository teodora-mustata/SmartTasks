using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartTasksAPI.Controllers;
using SmartTasksAPI.Contracts.Lists;
using SmartTasksAPI.Models;
using SmartTasksAPI.Services;

namespace SmartTasksAPI.Tests.Controllers;

public class ListsControllerTests
{
    [Fact]
    public async Task GetByBoardId_ShouldReturnOk_WhenListsExist()
    {
        var boardId = Guid.NewGuid();
        var lists = new List<BoardList> { new() { Id = Guid.NewGuid(), BoardId = boardId, Name = "Todo" } };
        var listService = new Mock<IListService>();
        listService.Setup(x => x.GetByBoardIdAsync(boardId)).ReturnsAsync(lists);

        var controller = new ListsController(listService.Object);

        var result = await controller.GetByBoardId(boardId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(lists, okResult.Value);
    }

    [Fact]
    public async Task GetByBoardId_ShouldReturnNotFound_WhenBoardDoesNotExist()
    {
        var listService = new Mock<IListService>();
        listService.Setup(x => x.GetByBoardIdAsync(It.IsAny<Guid>())).ThrowsAsync(new KeyNotFoundException("Board not found."));

        var controller = new ListsController(listService.Object);

        var result = await controller.GetByBoardId(Guid.NewGuid());

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Board not found.", notFound.Value!.ToString());
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenListExists()
    {
        var listId = Guid.NewGuid();
        var list = new BoardList { Id = listId, BoardId = Guid.NewGuid(), Name = "Todo" };
        var listService = new Mock<IListService>();
        listService.Setup(x => x.GetByIdAsync(listId)).ReturnsAsync(list);

        var controller = new ListsController(listService.Object);

        var result = await controller.GetById(listId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(list, okResult.Value);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenListDoesNotExist()
    {
        var listService = new Mock<IListService>();
        listService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((BoardList?)null);

        var controller = new ListsController(listService.Object);

        var result = await controller.GetById(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WhenListIsCreated()
    {
        var boardId = Guid.NewGuid();
        var listId = Guid.NewGuid();
        var created = new BoardList { Id = listId, BoardId = boardId, Name = "Todo" };
        var listService = new Mock<IListService>();
        listService.Setup(x => x.CreateAsync(boardId, "Todo")).ReturnsAsync(created);

        var controller = new ListsController(listService.Object);

        var result = await controller.Create(boardId, new CreateListRequest { Name = "Todo" });

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(ListsController.GetById), createdResult.ActionName);
        Assert.Equal(listId, createdResult.RouteValues!["listId"]);
        Assert.Same(created, createdResult.Value);
    }

    [Fact]
    public async Task Create_ShouldReturnNotFound_WhenBoardDoesNotExist()
    {
        var listService = new Mock<IListService>();
        listService.Setup(x => x.CreateAsync(It.IsAny<Guid>(), It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException("Board not found."));

        var controller = new ListsController(listService.Object);

        var result = await controller.Create(Guid.NewGuid(), new CreateListRequest { Name = "Todo" });

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Board not found.", notFound.Value!.ToString());
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenListExists()
    {
        var listService = new Mock<IListService>();
        listService.Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(true);

        var controller = new ListsController(listService.Object);

        var result = await controller.Update(Guid.NewGuid(), new UpdateListRequest { Name = "Doing", Position = 2 });

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenListDoesNotExist()
    {
        var listService = new Mock<IListService>();
        listService.Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(false);

        var controller = new ListsController(listService.Object);

        var result = await controller.Update(Guid.NewGuid(), new UpdateListRequest { Name = "Doing", Position = 2 });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenListExists()
    {
        var listService = new Mock<IListService>();
        listService.Setup(x => x.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        var controller = new ListsController(listService.Object);

        var result = await controller.Delete(Guid.NewGuid());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenListDoesNotExist()
    {
        var listService = new Mock<IListService>();
        listService.Setup(x => x.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        var controller = new ListsController(listService.Object);

        var result = await controller.Delete(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }
}
