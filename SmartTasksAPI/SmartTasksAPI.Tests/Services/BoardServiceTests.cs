using Moq;
using SmartTasksAPI.Models;
using SmartTasksAPI.Models.Enums;
using SmartTasksAPI.Repositories;
using SmartTasksAPI.Services;

namespace SmartTasksAPI.Tests.Services;

public class BoardServiceTests
{
    [Fact]
    public async Task GetAllAsync_ShouldReturnBoardsFromRepository()
    {
        var expected = new List<Board> { new() { Id = Guid.NewGuid(), Name = "Roadmap" } };
        var boardRepository = new Mock<IBoardRepository>();
        var userRepository = new Mock<IUserRepository>();

        boardRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(expected);

        var service = new BoardService(boardRepository.Object, userRepository.Object);

        var result = await service.GetAllAsync();

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnBoardFromRepository()
    {
        var boardId = Guid.NewGuid();
        var expected = new Board { Id = boardId, Name = "Roadmap" };
        var boardRepository = new Mock<IBoardRepository>();
        var userRepository = new Mock<IUserRepository>();

        boardRepository.Setup(x => x.GetByIdAsync(boardId)).ReturnsAsync(expected);

        var service = new BoardService(boardRepository.Object, userRepository.Object);

        var result = await service.GetByIdAsync(boardId);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenOwnerDoesNotExist()
    {
        var boardRepository = new Mock<IBoardRepository>();
        var userRepository = new Mock<IUserRepository>();

        userRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        var service = new BoardService(boardRepository.Object, userRepository.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateAsync("Board", "Desc", Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateBoardAndOwnerMembership_WhenOwnerExists()
    {
        var ownerId = Guid.NewGuid();
        Board? addedBoard = null;
        BoardMember? addedMember = null;

        var boardRepository = new Mock<IBoardRepository>();
        var userRepository = new Mock<IUserRepository>();

        userRepository.Setup(x => x.GetByIdAsync(ownerId))
            .ReturnsAsync(new User { Id = ownerId, FullName = "Owner", Email = "owner@test.com" });

        boardRepository.Setup(x => x.AddAsync(It.IsAny<Board>()))
            .Callback<Board>(board => addedBoard = board)
            .ReturnsAsync((Board board) => board);

        boardRepository.Setup(x => x.AddMemberAsync(It.IsAny<BoardMember>()))
            .Callback<BoardMember>(member => addedMember = member)
            .Returns(Task.CompletedTask);

        boardRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => new Board { Id = id, Name = "Board", OwnerId = ownerId });

        var service = new BoardService(boardRepository.Object, userRepository.Object);

        var result = await service.CreateAsync("  Product  ", "  Q2 plan  ", ownerId);

        Assert.NotNull(addedBoard);
        Assert.Equal("Product", addedBoard!.Name);
        Assert.Equal("Q2 plan", addedBoard.Description);
        Assert.Equal(ownerId, addedBoard.OwnerId);

        Assert.NotNull(addedMember);
        Assert.Equal(addedBoard.Id, addedMember!.BoardId);
        Assert.Equal(ownerId, addedMember.UserId);
        Assert.Equal(BoardRole.Owner, addedMember.Role);

        Assert.Equal(addedBoard.Id, result.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenBoardDoesNotExist()
    {
        var boardRepository = new Mock<IBoardRepository>();
        var userRepository = new Mock<IUserRepository>();

        boardRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Board?)null);

        var service = new BoardService(boardRepository.Object, userRepository.Object);

        var result = await service.UpdateAsync(Guid.NewGuid(), "Board", "Desc");

        Assert.False(result);
        boardRepository.Verify(x => x.UpdateAsync(It.IsAny<Board>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateBoard_WhenBoardExists()
    {
        var boardId = Guid.NewGuid();
        var existing = new Board { Id = boardId, Name = "Old", Description = "Old desc", OwnerId = Guid.NewGuid() };

        var boardRepository = new Mock<IBoardRepository>();
        var userRepository = new Mock<IUserRepository>();

        boardRepository.Setup(x => x.GetByIdAsync(boardId)).ReturnsAsync(existing);

        var service = new BoardService(boardRepository.Object, userRepository.Object);

        var result = await service.UpdateAsync(boardId, "  New Name ", "  New Desc ");

        Assert.True(result);
        Assert.Equal("New Name", existing.Name);
        Assert.Equal("New Desc", existing.Description);
        boardRepository.Verify(x => x.UpdateAsync(existing), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenBoardDoesNotExist()
    {
        var boardRepository = new Mock<IBoardRepository>();
        var userRepository = new Mock<IUserRepository>();

        boardRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Board?)null);

        var service = new BoardService(boardRepository.Object, userRepository.Object);

        var result = await service.DeleteAsync(Guid.NewGuid());

        Assert.False(result);
        boardRepository.Verify(x => x.DeleteAsync(It.IsAny<Board>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteBoard_WhenBoardExists()
    {
        var board = new Board { Id = Guid.NewGuid(), Name = "Board", OwnerId = Guid.NewGuid() };

        var boardRepository = new Mock<IBoardRepository>();
        var userRepository = new Mock<IUserRepository>();

        boardRepository.Setup(x => x.GetByIdAsync(board.Id)).ReturnsAsync(board);

        var service = new BoardService(boardRepository.Object, userRepository.Object);

        var result = await service.DeleteAsync(board.Id);

        Assert.True(result);
        boardRepository.Verify(x => x.DeleteAsync(board), Times.Once);
    }

    [Fact]
    public async Task AddMemberAsync_ShouldReturnFalse_WhenBoardDoesNotExist()
    {
        var boardRepository = new Mock<IBoardRepository>();
        var userRepository = new Mock<IUserRepository>();

        boardRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Board?)null);

        var service = new BoardService(boardRepository.Object, userRepository.Object);

        var result = await service.AddMemberAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task AddMemberAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var boardRepository = new Mock<IBoardRepository>();
        var userRepository = new Mock<IUserRepository>();

        boardRepository.Setup(x => x.GetByIdAsync(boardId))
            .ReturnsAsync(new Board { Id = boardId, OwnerId = Guid.NewGuid(), Name = "Board" });
        userRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        var service = new BoardService(boardRepository.Object, userRepository.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.AddMemberAsync(boardId, userId));
    }

    [Fact]
    public async Task AddMemberAsync_ShouldThrow_WhenMemberAlreadyExists()
    {
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var boardRepository = new Mock<IBoardRepository>();
        var userRepository = new Mock<IUserRepository>();

        boardRepository.Setup(x => x.GetByIdAsync(boardId))
            .ReturnsAsync(new Board { Id = boardId, OwnerId = Guid.NewGuid(), Name = "Board" });
        userRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new User { Id = userId, Email = "a@a.com", FullName = "A" });
        boardRepository.Setup(x => x.MemberExistsAsync(boardId, userId)).ReturnsAsync(true);

        var service = new BoardService(boardRepository.Object, userRepository.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddMemberAsync(boardId, userId));
    }

    [Fact]
    public async Task AddMemberAsync_ShouldAddMember_WhenDataIsValid()
    {
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        BoardMember? addedMember = null;

        var boardRepository = new Mock<IBoardRepository>();
        var userRepository = new Mock<IUserRepository>();

        boardRepository.Setup(x => x.GetByIdAsync(boardId))
            .ReturnsAsync(new Board { Id = boardId, OwnerId = Guid.NewGuid(), Name = "Board" });
        userRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new User { Id = userId, Email = "a@a.com", FullName = "A" });
        boardRepository.Setup(x => x.MemberExistsAsync(boardId, userId)).ReturnsAsync(false);
        boardRepository.Setup(x => x.AddMemberAsync(It.IsAny<BoardMember>()))
            .Callback<BoardMember>(member => addedMember = member)
            .Returns(Task.CompletedTask);

        var service = new BoardService(boardRepository.Object, userRepository.Object);

        var result = await service.AddMemberAsync(boardId, userId);

        Assert.True(result);
        Assert.NotNull(addedMember);
        Assert.Equal(boardId, addedMember!.BoardId);
        Assert.Equal(userId, addedMember.UserId);
        Assert.Equal(BoardRole.Member, addedMember.Role);
    }

    [Fact]
    public async Task RemoveMemberAsync_ShouldReturnFalse_WhenBoardDoesNotExist()
    {
        var boardRepository = new Mock<IBoardRepository>();
        var userRepository = new Mock<IUserRepository>();

        boardRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Board?)null);

        var service = new BoardService(boardRepository.Object, userRepository.Object);

        var result = await service.RemoveMemberAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task RemoveMemberAsync_ShouldThrow_WhenTryingToRemoveOwner()
    {
        var boardId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var boardRepository = new Mock<IBoardRepository>();
        var userRepository = new Mock<IUserRepository>();

        boardRepository.Setup(x => x.GetByIdAsync(boardId))
            .ReturnsAsync(new Board { Id = boardId, OwnerId = ownerId, Name = "Board" });

        var service = new BoardService(boardRepository.Object, userRepository.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RemoveMemberAsync(boardId, ownerId));
    }

    [Fact]
    public async Task RemoveMemberAsync_ShouldReturnFalse_WhenMemberDoesNotExist()
    {
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var boardRepository = new Mock<IBoardRepository>();
        var userRepository = new Mock<IUserRepository>();

        boardRepository.Setup(x => x.GetByIdAsync(boardId))
            .ReturnsAsync(new Board { Id = boardId, OwnerId = Guid.NewGuid(), Name = "Board" });
        boardRepository.Setup(x => x.GetMemberAsync(boardId, userId)).ReturnsAsync((BoardMember?)null);

        var service = new BoardService(boardRepository.Object, userRepository.Object);

        var result = await service.RemoveMemberAsync(boardId, userId);

        Assert.False(result);
    }

    [Fact]
    public async Task RemoveMemberAsync_ShouldRemoveMember_WhenMemberExists()
    {
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var member = new BoardMember { BoardId = boardId, UserId = userId };

        var boardRepository = new Mock<IBoardRepository>();
        var userRepository = new Mock<IUserRepository>();

        boardRepository.Setup(x => x.GetByIdAsync(boardId))
            .ReturnsAsync(new Board { Id = boardId, OwnerId = Guid.NewGuid(), Name = "Board" });
        boardRepository.Setup(x => x.GetMemberAsync(boardId, userId)).ReturnsAsync(member);

        var service = new BoardService(boardRepository.Object, userRepository.Object);

        var result = await service.RemoveMemberAsync(boardId, userId);

        Assert.True(result);
        boardRepository.Verify(x => x.RemoveMemberAsync(member), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldTrimWhitespace_FromNameAndDescription()
    {
        var ownerId = Guid.NewGuid();
        Board? addedBoard = null;

        var boardRepository = new Mock<IBoardRepository>();
        var userRepository = new Mock<IUserRepository>();

        userRepository.Setup(x => x.GetByIdAsync(ownerId))
            .ReturnsAsync(new User { Id = ownerId, FullName = "Owner", Email = "owner@test.com" });

        boardRepository.Setup(x => x.AddAsync(It.IsAny<Board>()))
            .Callback<Board>(board => addedBoard = board)
            .ReturnsAsync((Board board) => board);

        boardRepository.Setup(x => x.AddMemberAsync(It.IsAny<BoardMember>())).Returns(Task.CompletedTask);
        boardRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => new Board { Id = id, Name = addedBoard?.Name ?? "", OwnerId = ownerId });

        var service = new BoardService(boardRepository.Object, userRepository.Object);

        var result = await service.CreateAsync("   Spaces   ", "\t\tTabs\t\t", ownerId);

        Assert.Equal("Spaces", addedBoard?.Name);
        Assert.Equal("Tabs", addedBoard?.Description);
    }

    [Fact]
    public async Task UpdateAsync_ShouldTrimWhitespace_FromNameAndDescription()
    {
        var boardId = Guid.NewGuid();
        var existing = new Board { Id = boardId, Name = "Old", Description = "Old", OwnerId = Guid.NewGuid() };

        var boardRepository = new Mock<IBoardRepository>();
        var userRepository = new Mock<IUserRepository>();

        boardRepository.Setup(x => x.GetByIdAsync(boardId)).ReturnsAsync(existing);

        var service = new BoardService(boardRepository.Object, userRepository.Object);

        await service.UpdateAsync(boardId, "\n\nNew\n\n", "   New Desc   ");

        Assert.Equal("New", existing.Name);
        Assert.Equal("New Desc", existing.Description);
    }
}
