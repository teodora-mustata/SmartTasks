using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartTasksAPI.Controllers;
using SmartTasksAPI.Contracts.Cards;
using SmartTasksAPI.Models;
using SmartTasksAPI.Services;

namespace SmartTasksAPI.Tests.Controllers;

public class CardsControllerTests
{
    [Fact]
    public async Task GetByList_ShouldReturnOk_WhenListHasCards()
    {
        var listId = Guid.NewGuid();
        var cards = new List<CardItem> { new() { Id = Guid.NewGuid(), ListId = listId, Title = "Task" } };
        var cardService = new Mock<ICardService>();
        cardService.Setup(x => x.GetByListIdAsync(listId)).ReturnsAsync(cards);

        var controller = new CardsController(cardService.Object);

        var result = await controller.GetByList(listId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(cards, okResult.Value);
    }

    [Fact]
    public async Task GetByList_ShouldReturnNotFound_WhenListDoesNotExist()
    {
        var cardService = new Mock<ICardService>();
        cardService.Setup(x => x.GetByListIdAsync(It.IsAny<Guid>())).ThrowsAsync(new KeyNotFoundException("List not found."));

        var controller = new CardsController(cardService.Object);

        var result = await controller.GetByList(Guid.NewGuid());

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("List not found.", notFound.Value!.ToString());
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenCardExists()
    {
        var cardId = Guid.NewGuid();
        var card = new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" };
        var cardService = new Mock<ICardService>();
        cardService.Setup(x => x.GetByIdAsync(cardId)).ReturnsAsync(card);

        var controller = new CardsController(cardService.Object);

        var result = await controller.GetById(cardId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(card, okResult.Value);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenCardDoesNotExist()
    {
        var cardService = new Mock<ICardService>();
        cardService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CardItem?)null);

        var controller = new CardsController(cardService.Object);

        var result = await controller.GetById(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WhenCardIsCreated()
    {
        var listId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var created = new CardItem { Id = cardId, ListId = listId, Title = "Task" };
        var cardService = new Mock<ICardService>();
        cardService.Setup(x => x.CreateAsync(listId, "Task", "Desc", It.IsAny<DateTime?>())).ReturnsAsync(created);

        var controller = new CardsController(cardService.Object);

        var result = await controller.Create(listId, new CreateCardRequest { Title = "Task", Description = "Desc", DueDateUtc = null });

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(CardsController.GetById), createdResult.ActionName);
        Assert.Equal(cardId, createdResult.RouteValues!["cardId"]);
        Assert.Same(created, createdResult.Value);
    }

    [Fact]
    public async Task Create_ShouldReturnNotFound_WhenListDoesNotExist()
    {
        var cardService = new Mock<ICardService>();
        cardService.Setup(x => x.CreateAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<DateTime?>()))
            .ThrowsAsync(new KeyNotFoundException("List not found."));

        var controller = new CardsController(cardService.Object);

        var result = await controller.Create(Guid.NewGuid(), new CreateCardRequest { Title = "Task", Description = "Desc", DueDateUtc = null });

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("List not found.", notFound.Value!.ToString());
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenCardExists()
    {
        var cardService = new Mock<ICardService>();
        cardService.Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(true);

        var controller = new CardsController(cardService.Object);

        var result = await controller.Update(Guid.NewGuid(), new UpdateCardRequest { Title = "Task", Description = "Desc", Position = 2, DueDateUtc = null });

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenCardDoesNotExist()
    {
        var cardService = new Mock<ICardService>();
        cardService.Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(false);

        var controller = new CardsController(cardService.Object);

        var result = await controller.Update(Guid.NewGuid(), new UpdateCardRequest { Title = "Task", Description = "Desc", Position = 2, DueDateUtc = null });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Move_ShouldReturnNoContent_WhenCardIsMoved()
    {
        var cardService = new Mock<ICardService>();
        cardService.Setup(x => x.MoveAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>())).ReturnsAsync(true);

        var controller = new CardsController(cardService.Object);

        var result = await controller.Move(Guid.NewGuid(), new MoveCardRequest { TargetListId = Guid.NewGuid(), TargetPosition = 3 });

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Move_ShouldReturnNotFound_WhenCardDoesNotExist()
    {
        var cardService = new Mock<ICardService>();
        cardService.Setup(x => x.MoveAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>())).ReturnsAsync(false);

        var controller = new CardsController(cardService.Object);

        var result = await controller.Move(Guid.NewGuid(), new MoveCardRequest { TargetListId = Guid.NewGuid(), TargetPosition = 3 });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Move_ShouldReturnNotFound_WhenTargetListDoesNotExist()
    {
        var cardService = new Mock<ICardService>();
        cardService.Setup(x => x.MoveAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ThrowsAsync(new KeyNotFoundException("Target list not found."));

        var controller = new CardsController(cardService.Object);

        var result = await controller.Move(Guid.NewGuid(), new MoveCardRequest { TargetListId = Guid.NewGuid(), TargetPosition = 3 });

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Target list not found.", notFound.Value!.ToString());
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenCardExists()
    {
        var cardService = new Mock<ICardService>();
        cardService.Setup(x => x.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        var controller = new CardsController(cardService.Object);

        var result = await controller.Delete(Guid.NewGuid());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenCardDoesNotExist()
    {
        var cardService = new Mock<ICardService>();
        cardService.Setup(x => x.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        var controller = new CardsController(cardService.Object);

        var result = await controller.Delete(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Assign_ShouldReturnNoContent_WhenUserAssigned()
    {
        var cardService = new Mock<ICardService>();
        cardService.Setup(x => x.AssignUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);

        var controller = new CardsController(cardService.Object);

        var result = await controller.Assign(Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Assign_ShouldReturnNotFound_WhenCardOrUserDoesNotExist()
    {
        var cardService = new Mock<ICardService>();
        cardService.Setup(x => x.AssignUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(false);

        var controller = new CardsController(cardService.Object);

        var result = await controller.Assign(Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Assign_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var cardService = new Mock<ICardService>();
        cardService.Setup(x => x.AssignUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ThrowsAsync(new KeyNotFoundException("User not found."));

        var controller = new CardsController(cardService.Object);

        var result = await controller.Assign(Guid.NewGuid(), Guid.NewGuid());

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("User not found.", notFound.Value!.ToString());
    }

    [Fact]
    public async Task Assign_ShouldReturnConflict_WhenUserAlreadyAssigned()
    {
        var cardService = new Mock<ICardService>();
        cardService.Setup(x => x.AssignUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ThrowsAsync(new InvalidOperationException("User is already assigned to this card."));

        var controller = new CardsController(cardService.Object);

        var result = await controller.Assign(Guid.NewGuid(), Guid.NewGuid());

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Contains("User is already assigned to this card.", conflict.Value!.ToString());
    }

    [Fact]
    public async Task Unassign_ShouldReturnNoContent_WhenAssignmentIsRemoved()
    {
        var cardService = new Mock<ICardService>();
        cardService.Setup(x => x.UnassignUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);

        var controller = new CardsController(cardService.Object);

        var result = await controller.Unassign(Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Unassign_ShouldReturnNotFound_WhenAssignmentDoesNotExist()
    {
        var cardService = new Mock<ICardService>();
        cardService.Setup(x => x.UnassignUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(false);

        var controller = new CardsController(cardService.Object);

        var result = await controller.Unassign(Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }
}
