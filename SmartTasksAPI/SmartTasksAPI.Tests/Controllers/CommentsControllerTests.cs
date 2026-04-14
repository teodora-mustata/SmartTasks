using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartTasksAPI.Controllers;
using SmartTasksAPI.Contracts.Comments;
using SmartTasksAPI.Models;
using SmartTasksAPI.Services;

namespace SmartTasksAPI.Tests.Controllers;

public class CommentsControllerTests
{
    [Fact]
    public async Task GetByCard_ShouldReturnOk_WhenCommentsExist()
    {
        var cardId = Guid.NewGuid();
        var comments = new List<CardComment> { new() { Id = Guid.NewGuid(), CardId = cardId, Message = "Nice" } };
        var commentService = new Mock<ICommentService>();
        commentService.Setup(x => x.GetByCardIdAsync(cardId)).ReturnsAsync(comments);

        var controller = new CommentsController(commentService.Object);

        var result = await controller.GetByCard(cardId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(comments, okResult.Value);
    }

    [Fact]
    public async Task GetByCard_ShouldReturnNotFound_WhenCardDoesNotExist()
    {
        var commentService = new Mock<ICommentService>();
        commentService.Setup(x => x.GetByCardIdAsync(It.IsAny<Guid>())).ThrowsAsync(new KeyNotFoundException("Card not found."));

        var controller = new CommentsController(commentService.Object);

        var result = await controller.GetByCard(Guid.NewGuid());

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Card not found.", notFound.Value!.ToString());
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WithLocation_WhenCommentIsCreated()
    {
        var cardId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var created = new CardComment { Id = commentId, CardId = cardId, AuthorId = Guid.NewGuid(), Message = "Nice" };
        var commentService = new Mock<ICommentService>();
        commentService.Setup(x => x.CreateAsync(cardId, created.AuthorId, "Nice")).ReturnsAsync(created);

        var controller = new CommentsController(commentService.Object);

        var result = await controller.Create(cardId, new CreateCommentRequest { AuthorId = created.AuthorId, Message = "Nice" });

        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal($"/api/comments/{commentId}", createdResult.Location);
        Assert.Same(created, createdResult.Value);
    }

    [Fact]
    public async Task Create_ShouldReturnNotFound_WhenCardOrAuthorDoesNotExist()
    {
        var commentService = new Mock<ICommentService>();
        commentService.Setup(x => x.CreateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .ThrowsAsync(new KeyNotFoundException("Card not found."));

        var controller = new CommentsController(commentService.Object);

        var result = await controller.Create(Guid.NewGuid(), new CreateCommentRequest { AuthorId = Guid.NewGuid(), Message = "Nice" });

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Card not found.", notFound.Value!.ToString());
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenCommentExists()
    {
        var commentService = new Mock<ICommentService>();
        commentService.Setup(x => x.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        var controller = new CommentsController(commentService.Object);

        var result = await controller.Delete(Guid.NewGuid());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenCommentDoesNotExist()
    {
        var commentService = new Mock<ICommentService>();
        commentService.Setup(x => x.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        var controller = new CommentsController(commentService.Object);

        var result = await controller.Delete(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }
}
