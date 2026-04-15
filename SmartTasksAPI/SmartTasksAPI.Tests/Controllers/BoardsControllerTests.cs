using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartTasksAPI.Controllers;
using SmartTasksAPI.Contracts.Boards;
using SmartTasksAPI.Models;
using SmartTasksAPI.Services;

namespace SmartTasksAPI.Tests.Controllers;

public class BoardsControllerTests
{
    [Fact]
    public async Task GetAll_ShouldReturnOk_WithBoards()
    {
        var boards = new List<Board> { new() { Id = Guid.NewGuid(), Name = "Board" } };
        var boardService = new Mock<IBoardService>();
        boardService.Setup(x => x.GetAllAsync()).ReturnsAsync(boards);

        var controller = new BoardsController(boardService.Object);

        var result = await controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(boards, okResult.Value);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenBoardExists()
    {
        var boardId = Guid.NewGuid();
        var board = new Board { Id = boardId, Name = "Board" };
        var boardService = new Mock<IBoardService>();
        boardService.Setup(x => x.GetByIdAsync(boardId)).ReturnsAsync(board);

        var controller = new BoardsController(boardService.Object);

        var result = await controller.GetById(boardId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(board, okResult.Value);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenBoardDoesNotExist()
    {
        var boardService = new Mock<IBoardService>();
        boardService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Board?)null);

        var controller = new BoardsController(boardService.Object);

        var result = await controller.GetById(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WhenBoardIsCreated()
    {
        var ownerId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var created = new Board { Id = boardId, Name = "Board", OwnerId = ownerId };
        var boardService = new Mock<IBoardService>();
        boardService.Setup(x => x.CreateAsync("Board", "Desc", ownerId)).ReturnsAsync(created);

        var controller = new BoardsController(boardService.Object);

        var result = await controller.Create(new CreateBoardRequest { Name = "Board", Description = "Desc", OwnerId = ownerId });

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(BoardsController.GetById), createdResult.ActionName);
        Assert.Equal(boardId, createdResult.RouteValues!["boardId"]);
        Assert.Same(created, createdResult.Value);
    }

    [Fact]
    public async Task Create_ShouldReturnNotFound_WhenOwnerDoesNotExist()
    {
        var boardService = new Mock<IBoardService>();
        boardService.Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<Guid>()))
            .ThrowsAsync(new KeyNotFoundException("Owner not found."));

        var controller = new BoardsController(boardService.Object);

        var result = await controller.Create(new CreateBoardRequest { Name = "Board", Description = "Desc", OwnerId = Guid.NewGuid() });

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Owner not found.", notFound.Value!.ToString());
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenBoardExists()
    {
        var boardService = new Mock<IBoardService>();
        boardService.Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>())).ReturnsAsync(true);

        var controller = new BoardsController(boardService.Object);

        var result = await controller.Update(Guid.NewGuid(), new UpdateBoardRequest { Name = "Board", Description = "Desc" });

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenBoardDoesNotExist()
    {
        var boardService = new Mock<IBoardService>();
        boardService.Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>())).ReturnsAsync(false);

        var controller = new BoardsController(boardService.Object);

        var result = await controller.Update(Guid.NewGuid(), new UpdateBoardRequest { Name = "Board", Description = "Desc" });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenBoardExists()
    {
        var boardService = new Mock<IBoardService>();
        boardService.Setup(x => x.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        var controller = new BoardsController(boardService.Object);

        var result = await controller.Delete(Guid.NewGuid());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenBoardDoesNotExist()
    {
        var boardService = new Mock<IBoardService>();
        boardService.Setup(x => x.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        var controller = new BoardsController(boardService.Object);

        var result = await controller.Delete(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddMember_ShouldReturnNoContent_WhenMemberIsAdded()
    {
        var boardService = new Mock<IBoardService>();
        boardService.Setup(x => x.AddMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);

        var controller = new BoardsController(boardService.Object);

        var result = await controller.AddMember(Guid.NewGuid(), new AddBoardMemberRequest { UserId = Guid.NewGuid() });

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task AddMember_ShouldReturnNotFound_WhenBoardOrUserDoesNotExist()
    {
        var boardService = new Mock<IBoardService>();
        boardService.Setup(x => x.AddMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(false);

        var controller = new BoardsController(boardService.Object);

        var result = await controller.AddMember(Guid.NewGuid(), new AddBoardMemberRequest { UserId = Guid.NewGuid() });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddMember_ShouldReturnNotFound_WhenUserIsMissing()
    {
        var boardService = new Mock<IBoardService>();
        boardService.Setup(x => x.AddMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ThrowsAsync(new KeyNotFoundException("User not found."));

        var controller = new BoardsController(boardService.Object);

        var result = await controller.AddMember(Guid.NewGuid(), new AddBoardMemberRequest { UserId = Guid.NewGuid() });

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("User not found.", notFound.Value!.ToString());
    }

    [Fact]
    public async Task AddMember_ShouldReturnConflict_WhenMemberAlreadyExists()
    {
        var boardService = new Mock<IBoardService>();
        boardService.Setup(x => x.AddMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ThrowsAsync(new InvalidOperationException("User is already a board member."));

        var controller = new BoardsController(boardService.Object);

        var result = await controller.AddMember(Guid.NewGuid(), new AddBoardMemberRequest { UserId = Guid.NewGuid() });

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Contains("User is already a board member.", conflict.Value!.ToString());
    }

    [Fact]
    public async Task RemoveMember_ShouldReturnNoContent_WhenMemberIsRemoved()
    {
        var boardService = new Mock<IBoardService>();
        boardService.Setup(x => x.RemoveMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);

        var controller = new BoardsController(boardService.Object);

        var result = await controller.RemoveMember(Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RemoveMember_ShouldReturnNotFound_WhenMemberDoesNotExist()
    {
        var boardService = new Mock<IBoardService>();
        boardService.Setup(x => x.RemoveMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(false);

        var controller = new BoardsController(boardService.Object);

        var result = await controller.RemoveMember(Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task RemoveMember_ShouldReturnConflict_WhenOwnerCannotBeRemoved()
    {
        var boardService = new Mock<IBoardService>();
        boardService.Setup(x => x.RemoveMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ThrowsAsync(new InvalidOperationException("Board owner cannot be removed."));

        var controller = new BoardsController(boardService.Object);

        var result = await controller.RemoveMember(Guid.NewGuid(), Guid.NewGuid());

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Contains("Board owner cannot be removed.", conflict.Value!.ToString());
    }
}
