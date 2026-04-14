using Moq;
using SmartTasksAPI.Models;
using SmartTasksAPI.Repositories;
using SmartTasksAPI.Services;

namespace SmartTasksAPI.Tests.Services;

public class ListServiceTests
{
    [Fact]
    public async Task GetByBoardIdAsync_ShouldThrow_WhenBoardDoesNotExist()
    {
        var boardId = Guid.NewGuid();
        var listRepository = new Mock<IListRepository>();
        var boardRepository = new Mock<IBoardRepository>();

        boardRepository.Setup(x => x.GetByIdAsync(boardId)).ReturnsAsync((Board?)null);

        var service = new ListService(listRepository.Object, boardRepository.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetByBoardIdAsync(boardId));
    }

    [Fact]
    public async Task GetByBoardIdAsync_ShouldReturnLists_WhenBoardExists()
    {
        var boardId = Guid.NewGuid();
        var expected = new List<BoardList> { new() { Id = Guid.NewGuid(), BoardId = boardId, Name = "Todo" } };
        var listRepository = new Mock<IListRepository>();
        var boardRepository = new Mock<IBoardRepository>();

        boardRepository.Setup(x => x.GetByIdAsync(boardId)).ReturnsAsync(new Board { Id = boardId, Name = "Board" });
        listRepository.Setup(x => x.GetByBoardIdAsync(boardId)).ReturnsAsync(expected);

        var service = new ListService(listRepository.Object, boardRepository.Object);

        var result = await service.GetByBoardIdAsync(boardId);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnListFromRepository()
    {
        var listId = Guid.NewGuid();
        var expected = new BoardList { Id = listId, BoardId = Guid.NewGuid(), Name = "Todo" };
        var listRepository = new Mock<IListRepository>();
        var boardRepository = new Mock<IBoardRepository>();

        listRepository.Setup(x => x.GetByIdAsync(listId)).ReturnsAsync(expected);

        var service = new ListService(listRepository.Object, boardRepository.Object);

        var result = await service.GetByIdAsync(listId);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenBoardDoesNotExist()
    {
        var boardId = Guid.NewGuid();
        var listRepository = new Mock<IListRepository>();
        var boardRepository = new Mock<IBoardRepository>();

        boardRepository.Setup(x => x.GetByIdAsync(boardId)).ReturnsAsync((Board?)null);

        var service = new ListService(listRepository.Object, boardRepository.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateAsync(boardId, "Todo"));
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateList_WhenBoardExists()
    {
        var boardId = Guid.NewGuid();
        BoardList? addedList = null;

        var listRepository = new Mock<IListRepository>();
        var boardRepository = new Mock<IBoardRepository>();

        boardRepository.Setup(x => x.GetByIdAsync(boardId)).ReturnsAsync(new Board { Id = boardId, Name = "Board" });
        listRepository.Setup(x => x.GetNextPositionAsync(boardId)).ReturnsAsync(4);
        listRepository.Setup(x => x.AddAsync(It.IsAny<BoardList>()))
            .Callback<BoardList>(list => addedList = list)
            .ReturnsAsync((BoardList list) => list);

        var service = new ListService(listRepository.Object, boardRepository.Object);

        var result = await service.CreateAsync(boardId, "  Done ");

        Assert.NotNull(addedList);
        Assert.Equal("Done", addedList!.Name);
        Assert.Equal(boardId, addedList.BoardId);
        Assert.Equal(4, addedList.Position);
        Assert.Same(addedList, result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenListDoesNotExist()
    {
        var listRepository = new Mock<IListRepository>();
        var boardRepository = new Mock<IBoardRepository>();

        listRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((BoardList?)null);

        var service = new ListService(listRepository.Object, boardRepository.Object);

        var result = await service.UpdateAsync(Guid.NewGuid(), "Name", 1);

        Assert.False(result);
        listRepository.Verify(x => x.UpdateAsync(It.IsAny<BoardList>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateList_WhenListExists()
    {
        var listId = Guid.NewGuid();
        var list = new BoardList { Id = listId, BoardId = Guid.NewGuid(), Name = "Old", Position = 1 };

        var listRepository = new Mock<IListRepository>();
        var boardRepository = new Mock<IBoardRepository>();

        listRepository.Setup(x => x.GetByIdAsync(listId)).ReturnsAsync(list);

        var service = new ListService(listRepository.Object, boardRepository.Object);

        var result = await service.UpdateAsync(listId, "  New Name ", 6);

        Assert.True(result);
        Assert.Equal("New Name", list.Name);
        Assert.Equal(6, list.Position);
        listRepository.Verify(x => x.UpdateAsync(list), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenListDoesNotExist()
    {
        var listRepository = new Mock<IListRepository>();
        var boardRepository = new Mock<IBoardRepository>();

        listRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((BoardList?)null);

        var service = new ListService(listRepository.Object, boardRepository.Object);

        var result = await service.DeleteAsync(Guid.NewGuid());

        Assert.False(result);
        listRepository.Verify(x => x.DeleteAsync(It.IsAny<BoardList>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteList_WhenListExists()
    {
        var list = new BoardList { Id = Guid.NewGuid(), BoardId = Guid.NewGuid(), Name = "Todo", Position = 1 };

        var listRepository = new Mock<IListRepository>();
        var boardRepository = new Mock<IBoardRepository>();

        listRepository.Setup(x => x.GetByIdAsync(list.Id)).ReturnsAsync(list);

        var service = new ListService(listRepository.Object, boardRepository.Object);

        var result = await service.DeleteAsync(list.Id);

        Assert.True(result);
        listRepository.Verify(x => x.DeleteAsync(list), Times.Once);
    }
}
