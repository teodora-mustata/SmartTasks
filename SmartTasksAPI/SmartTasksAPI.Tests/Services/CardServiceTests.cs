using Moq;
using SmartTasksAPI.Models;
using SmartTasksAPI.Repositories;
using SmartTasksAPI.Services;

namespace SmartTasksAPI.Tests.Services;

public class CardServiceTests
{
    [Fact]
    public async Task GetByListIdAsync_ShouldThrow_WhenListDoesNotExist()
    {
        var listId = Guid.NewGuid();
        var cardRepository = new Mock<ICardRepository>();
        var listRepository = new Mock<IListRepository>();
        var userRepository = new Mock<IUserRepository>();

        listRepository.Setup(x => x.GetByIdAsync(listId)).ReturnsAsync((BoardList?)null);

        var service = new CardService(cardRepository.Object, listRepository.Object, userRepository.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetByListIdAsync(listId));
    }

    [Fact]
    public async Task GetByListIdAsync_ShouldReturnCards_WhenListExists()
    {
        var listId = Guid.NewGuid();
        var expected = new List<CardItem> { new() { Id = Guid.NewGuid(), ListId = listId, Title = "Task" } };
        var cardRepository = new Mock<ICardRepository>();
        var listRepository = new Mock<IListRepository>();
        var userRepository = new Mock<IUserRepository>();

        listRepository.Setup(x => x.GetByIdAsync(listId)).ReturnsAsync(new BoardList { Id = listId, Name = "Todo" });
        cardRepository.Setup(x => x.GetByListIdAsync(listId)).ReturnsAsync(expected);

        var service = new CardService(cardRepository.Object, listRepository.Object, userRepository.Object);

        var result = await service.GetByListIdAsync(listId);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCardFromRepository()
    {
        var cardId = Guid.NewGuid();
        var expected = new CardItem { Id = cardId, Title = "Task", ListId = Guid.NewGuid() };
        var cardRepository = new Mock<ICardRepository>();
        var listRepository = new Mock<IListRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(cardId)).ReturnsAsync(expected);

        var service = new CardService(cardRepository.Object, listRepository.Object, userRepository.Object);

        var result = await service.GetByIdAsync(cardId);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenListDoesNotExist()
    {
        var listId = Guid.NewGuid();
        var cardRepository = new Mock<ICardRepository>();
        var listRepository = new Mock<IListRepository>();
        var userRepository = new Mock<IUserRepository>();

        listRepository.Setup(x => x.GetByIdAsync(listId)).ReturnsAsync((BoardList?)null);

        var service = new CardService(cardRepository.Object, listRepository.Object, userRepository.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateAsync(listId, "Title", "Desc", DateTime.UtcNow));
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateCard_WhenListExists()
    {
        var listId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow;
        CardItem? addedCard = null;

        var cardRepository = new Mock<ICardRepository>();
        var listRepository = new Mock<IListRepository>();
        var userRepository = new Mock<IUserRepository>();

        listRepository.Setup(x => x.GetByIdAsync(listId)).ReturnsAsync(new BoardList { Id = listId, Name = "Todo" });
        cardRepository.Setup(x => x.GetNextPositionAsync(listId)).ReturnsAsync(3);
        cardRepository.Setup(x => x.AddAsync(It.IsAny<CardItem>()))
            .Callback<CardItem>(card => addedCard = card)
            .ReturnsAsync((CardItem card) => card);

        var service = new CardService(cardRepository.Object, listRepository.Object, userRepository.Object);

        var result = await service.CreateAsync(listId, "  New Card ", "  Desc ", dueDate);

        Assert.NotNull(addedCard);
        Assert.Equal("New Card", addedCard!.Title);
        Assert.Equal("Desc", addedCard.Description);
        Assert.Equal(3, addedCard.Position);
        Assert.Equal(listId, addedCard.ListId);
        Assert.Equal(dueDate, addedCard.DueDateUtc);
        Assert.Same(addedCard, result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenCardDoesNotExist()
    {
        var cardRepository = new Mock<ICardRepository>();
        var listRepository = new Mock<IListRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CardItem?)null);

        var service = new CardService(cardRepository.Object, listRepository.Object, userRepository.Object);

        var result = await service.UpdateAsync(Guid.NewGuid(), "Title", "Desc", 1, null);

        Assert.False(result);
        cardRepository.Verify(x => x.UpdateAsync(It.IsAny<CardItem>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCard_WhenCardExists()
    {
        var cardId = Guid.NewGuid();
        var card = new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Old", Description = "Old", Position = 2 };
        var dueDate = DateTime.UtcNow;

        var cardRepository = new Mock<ICardRepository>();
        var listRepository = new Mock<IListRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(cardId)).ReturnsAsync(card);

        var service = new CardService(cardRepository.Object, listRepository.Object, userRepository.Object);

        var result = await service.UpdateAsync(cardId, "  New ", "  Updated ", 7, dueDate);

        Assert.True(result);
        Assert.Equal("New", card.Title);
        Assert.Equal("Updated", card.Description);
        Assert.Equal(7, card.Position);
        Assert.Equal(dueDate, card.DueDateUtc);
        cardRepository.Verify(x => x.UpdateAsync(card), Times.Once);
    }

    [Fact]
    public async Task MoveAsync_ShouldReturnFalse_WhenCardDoesNotExist()
    {
        var cardRepository = new Mock<ICardRepository>();
        var listRepository = new Mock<IListRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CardItem?)null);

        var service = new CardService(cardRepository.Object, listRepository.Object, userRepository.Object);

        var result = await service.MoveAsync(Guid.NewGuid(), Guid.NewGuid(), 1);

        Assert.False(result);
    }

    [Fact]
    public async Task MoveAsync_ShouldThrow_WhenTargetListDoesNotExist()
    {
        var cardId = Guid.NewGuid();
        var targetListId = Guid.NewGuid();
        var cardRepository = new Mock<ICardRepository>();
        var listRepository = new Mock<IListRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(cardId))
            .ReturnsAsync(new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" });
        listRepository.Setup(x => x.GetByIdAsync(targetListId)).ReturnsAsync((BoardList?)null);

        var service = new CardService(cardRepository.Object, listRepository.Object, userRepository.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.MoveAsync(cardId, targetListId, 5));
    }

    [Fact]
    public async Task MoveAsync_ShouldMoveCard_WhenTargetListExists()
    {
        var cardId = Guid.NewGuid();
        var targetListId = Guid.NewGuid();
        var card = new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task", Position = 2 };
        var cardRepository = new Mock<ICardRepository>();
        var listRepository = new Mock<IListRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(cardId)).ReturnsAsync(card);
        listRepository.Setup(x => x.GetByIdAsync(targetListId)).ReturnsAsync(new BoardList { Id = targetListId, Name = "Done" });

        var service = new CardService(cardRepository.Object, listRepository.Object, userRepository.Object);

        var result = await service.MoveAsync(cardId, targetListId, 10);

        Assert.True(result);
        Assert.Equal(targetListId, card.ListId);
        Assert.Equal(10, card.Position);
        cardRepository.Verify(x => x.UpdateAsync(card), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenCardDoesNotExist()
    {
        var cardRepository = new Mock<ICardRepository>();
        var listRepository = new Mock<IListRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CardItem?)null);

        var service = new CardService(cardRepository.Object, listRepository.Object, userRepository.Object);

        var result = await service.DeleteAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCard_WhenCardExists()
    {
        var card = new CardItem { Id = Guid.NewGuid(), ListId = Guid.NewGuid(), Title = "Task" };
        var cardRepository = new Mock<ICardRepository>();
        var listRepository = new Mock<IListRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(card.Id)).ReturnsAsync(card);

        var service = new CardService(cardRepository.Object, listRepository.Object, userRepository.Object);

        var result = await service.DeleteAsync(card.Id);

        Assert.True(result);
        cardRepository.Verify(x => x.DeleteAsync(card), Times.Once);
    }

    [Fact]
    public async Task AssignUserAsync_ShouldReturnFalse_WhenCardDoesNotExist()
    {
        var cardRepository = new Mock<ICardRepository>();
        var listRepository = new Mock<IListRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CardItem?)null);

        var service = new CardService(cardRepository.Object, listRepository.Object, userRepository.Object);

        var result = await service.AssignUserAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task AssignUserAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        var cardId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var cardRepository = new Mock<ICardRepository>();
        var listRepository = new Mock<IListRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(cardId))
            .ReturnsAsync(new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" });
        userRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        var service = new CardService(cardRepository.Object, listRepository.Object, userRepository.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.AssignUserAsync(cardId, userId));
    }

    [Fact]
    public async Task AssignUserAsync_ShouldThrow_WhenUserAlreadyAssigned()
    {
        var cardId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var cardRepository = new Mock<ICardRepository>();
        var listRepository = new Mock<IListRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(cardId))
            .ReturnsAsync(new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" });
        userRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new User { Id = userId, Email = "user@test.com", FullName = "User" });
        cardRepository.Setup(x => x.GetAssignmentAsync(cardId, userId))
            .ReturnsAsync(new CardAssignment { CardId = cardId, UserId = userId });

        var service = new CardService(cardRepository.Object, listRepository.Object, userRepository.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AssignUserAsync(cardId, userId));
    }

    [Fact]
    public async Task AssignUserAsync_ShouldAssign_WhenDataIsValid()
    {
        var cardId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        CardAssignment? assignment = null;

        var cardRepository = new Mock<ICardRepository>();
        var listRepository = new Mock<IListRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(cardId))
            .ReturnsAsync(new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" });
        userRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new User { Id = userId, Email = "user@test.com", FullName = "User" });
        cardRepository.Setup(x => x.GetAssignmentAsync(cardId, userId)).ReturnsAsync((CardAssignment?)null);
        cardRepository.Setup(x => x.AddAssignmentAsync(It.IsAny<CardAssignment>()))
            .Callback<CardAssignment>(x => assignment = x)
            .Returns(Task.CompletedTask);

        var service = new CardService(cardRepository.Object, listRepository.Object, userRepository.Object);

        var result = await service.AssignUserAsync(cardId, userId);

        Assert.True(result);
        Assert.NotNull(assignment);
        Assert.Equal(cardId, assignment!.CardId);
        Assert.Equal(userId, assignment.UserId);
    }

    [Fact]
    public async Task UnassignUserAsync_ShouldReturnFalse_WhenCardDoesNotExist()
    {
        var cardRepository = new Mock<ICardRepository>();
        var listRepository = new Mock<IListRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CardItem?)null);

        var service = new CardService(cardRepository.Object, listRepository.Object, userRepository.Object);

        var result = await service.UnassignUserAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task UnassignUserAsync_ShouldReturnFalse_WhenAssignmentDoesNotExist()
    {
        var cardId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var cardRepository = new Mock<ICardRepository>();
        var listRepository = new Mock<IListRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(cardId))
            .ReturnsAsync(new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" });
        cardRepository.Setup(x => x.GetAssignmentAsync(cardId, userId)).ReturnsAsync((CardAssignment?)null);

        var service = new CardService(cardRepository.Object, listRepository.Object, userRepository.Object);

        var result = await service.UnassignUserAsync(cardId, userId);

        Assert.False(result);
    }

    [Fact]
    public async Task UnassignUserAsync_ShouldRemoveAssignment_WhenAssignmentExists()
    {
        var cardId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var assignment = new CardAssignment { CardId = cardId, UserId = userId };
        var cardRepository = new Mock<ICardRepository>();
        var listRepository = new Mock<IListRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(cardId))
            .ReturnsAsync(new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" });
        cardRepository.Setup(x => x.GetAssignmentAsync(cardId, userId)).ReturnsAsync(assignment);

        var service = new CardService(cardRepository.Object, listRepository.Object, userRepository.Object);

        var result = await service.UnassignUserAsync(cardId, userId);

        Assert.True(result);
        cardRepository.Verify(x => x.RemoveAssignmentAsync(assignment), Times.Once);
    }
}

public class CardServiceTestsOriginal
{
    [Fact]
    public async Task AssignUserAsync_ShouldThrow_WhenUserAlreadyAssigned_Original()
    {
        var cardId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var cardRepository = new Mock<ICardRepository>();
        var listRepository = new Mock<IListRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(cardId))
            .ReturnsAsync(new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" });
        userRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new User { Id = userId, Email = "user@test.com", FullName = "User" });
        cardRepository.Setup(x => x.GetAssignmentAsync(cardId, userId))
            .ReturnsAsync(new CardAssignment { CardId = cardId, UserId = userId });

        var service = new CardService(cardRepository.Object, listRepository.Object, userRepository.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AssignUserAsync(cardId, userId));
    }
}
