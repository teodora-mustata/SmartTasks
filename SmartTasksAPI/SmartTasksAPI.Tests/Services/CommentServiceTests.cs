using Moq;
using SmartTasksAPI.Models;
using SmartTasksAPI.Repositories;
using SmartTasksAPI.Services;

namespace SmartTasksAPI.Tests.Services;

public class CommentServiceTests
{
    [Fact]
    public async Task GetByCardIdAsync_ShouldThrow_WhenCardDoesNotExist()
    {
        var cardId = Guid.NewGuid();
        var commentRepository = new Mock<ICommentRepository>();
        var cardRepository = new Mock<ICardRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(cardId)).ReturnsAsync((CardItem?)null);

        var service = new CommentService(commentRepository.Object, cardRepository.Object, userRepository.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetByCardIdAsync(cardId));
    }

    [Fact]
    public async Task GetByCardIdAsync_ShouldReturnComments_WhenCardExists()
    {
        var cardId = Guid.NewGuid();
        var expected = new List<CardComment> { new() { Id = Guid.NewGuid(), CardId = cardId, Message = "Looks good" } };
        var commentRepository = new Mock<ICommentRepository>();
        var cardRepository = new Mock<ICardRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(cardId)).ReturnsAsync(new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" });
        commentRepository.Setup(x => x.GetByCardIdAsync(cardId)).ReturnsAsync(expected);

        var service = new CommentService(commentRepository.Object, cardRepository.Object, userRepository.Object);

        var result = await service.GetByCardIdAsync(cardId);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenCardDoesNotExist()
    {
        var cardId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var commentRepository = new Mock<ICommentRepository>();
        var cardRepository = new Mock<ICardRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(cardId)).ReturnsAsync((CardItem?)null);

        var service = new CommentService(commentRepository.Object, cardRepository.Object, userRepository.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateAsync(cardId, authorId, "Message"));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenAuthorDoesNotExist()
    {
        var cardId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var commentRepository = new Mock<ICommentRepository>();
        var cardRepository = new Mock<ICardRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(cardId)).ReturnsAsync(new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" });
        userRepository.Setup(x => x.GetByIdAsync(authorId)).ReturnsAsync((User?)null);

        var service = new CommentService(commentRepository.Object, cardRepository.Object, userRepository.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateAsync(cardId, authorId, "Message"));
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateComment_WhenCardAndAuthorExist()
    {
        var cardId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        CardComment? addedComment = null;

        var commentRepository = new Mock<ICommentRepository>();
        var cardRepository = new Mock<ICardRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(cardId)).ReturnsAsync(new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" });
        userRepository.Setup(x => x.GetByIdAsync(authorId)).ReturnsAsync(new User { Id = authorId, FullName = "User", Email = "user@test.com" });
        commentRepository.Setup(x => x.AddAsync(It.IsAny<CardComment>()))
            .Callback<CardComment>(comment => addedComment = comment)
            .ReturnsAsync((CardComment comment) => comment);

        var service = new CommentService(commentRepository.Object, cardRepository.Object, userRepository.Object);

        var result = await service.CreateAsync(cardId, authorId, "  Looks good  ");

        Assert.NotNull(addedComment);
        Assert.Equal(cardId, addedComment!.CardId);
        Assert.Equal(authorId, addedComment.AuthorId);
        Assert.Equal("Looks good", addedComment.Message);
        Assert.Same(addedComment, result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenCommentDoesNotExist()
    {
        var commentRepository = new Mock<ICommentRepository>();
        var cardRepository = new Mock<ICardRepository>();
        var userRepository = new Mock<IUserRepository>();

        commentRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CardComment?)null);

        var service = new CommentService(commentRepository.Object, cardRepository.Object, userRepository.Object);

        var result = await service.DeleteAsync(Guid.NewGuid());

        Assert.False(result);
        commentRepository.Verify(x => x.DeleteAsync(It.IsAny<CardComment>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteComment_WhenCommentExists()
    {
        var commentId = Guid.NewGuid();
        var comment = new CardComment { Id = commentId, CardId = Guid.NewGuid(), AuthorId = Guid.NewGuid(), Message = "test" };

        var commentRepository = new Mock<ICommentRepository>();
        var cardRepository = new Mock<ICardRepository>();
        var userRepository = new Mock<IUserRepository>();

        commentRepository.Setup(x => x.GetByIdAsync(commentId)).ReturnsAsync(comment);

        var service = new CommentService(commentRepository.Object, cardRepository.Object, userRepository.Object);

        var result = await service.DeleteAsync(commentId);

        Assert.True(result);
        commentRepository.Verify(x => x.DeleteAsync(comment), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldTrimWhitespace_FromMessage()
    {
        var cardId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        CardComment? addedComment = null;

        var commentRepository = new Mock<ICommentRepository>();
        var cardRepository = new Mock<ICardRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(cardId)).ReturnsAsync(new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" });
        userRepository.Setup(x => x.GetByIdAsync(authorId)).ReturnsAsync(new User { Id = authorId, FullName = "User", Email = "user@test.com" });
        commentRepository.Setup(x => x.AddAsync(It.IsAny<CardComment>()))
            .Callback<CardComment>(comment => addedComment = comment)
            .ReturnsAsync((CardComment comment) => comment);

        var service = new CommentService(commentRepository.Object, cardRepository.Object, userRepository.Object);

        await service.CreateAsync(cardId, authorId, "  \n\tTrimmed message\n\t  ");

        Assert.Equal("Trimmed message", addedComment?.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldVerifyCommentAuthorAndCard()
    {
        var cardId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var commentRepository = new Mock<ICommentRepository>();
        var cardRepository = new Mock<ICardRepository>();
        var userRepository = new Mock<IUserRepository>();

        cardRepository.Setup(x => x.GetByIdAsync(cardId)).ReturnsAsync(new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" });
        userRepository.Setup(x => x.GetByIdAsync(authorId)).ReturnsAsync(new User { Id = authorId, FullName = "User", Email = "user@test.com" });
        commentRepository.Setup(x => x.AddAsync(It.IsAny<CardComment>())).ReturnsAsync((CardComment c) => c);

        var service = new CommentService(commentRepository.Object, cardRepository.Object, userRepository.Object);

        await service.CreateAsync(cardId, authorId, "Message");

        cardRepository.Verify(x => x.GetByIdAsync(cardId), Times.Once);
        userRepository.Verify(x => x.GetByIdAsync(authorId), Times.Once);
        commentRepository.Verify(x => x.AddAsync(It.IsAny<CardComment>()), Times.Once);
    }
}
