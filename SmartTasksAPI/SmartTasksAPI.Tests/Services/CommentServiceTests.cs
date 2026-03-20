using Moq;
using SmartTasksAPI.Models;
using SmartTasksAPI.Repositories;
using SmartTasksAPI.Services;

namespace SmartTasksAPI.Tests.Services
{
    public class CommentServiceTests
    {
        private Mock<ICommentRepository> _commentRepository = null!;
        private Mock<ICardRepository> _cardRepository = null!;
        private Mock<IUserRepository> _userRepository = null!;
        private ICommentService _service = null!;

        public CommentServiceTests()
        {
            _commentRepository = new Mock<ICommentRepository>();
            _cardRepository = new Mock<ICardRepository>();
            _userRepository = new Mock<IUserRepository>();
            _service = new CommentService(_commentRepository.Object, _cardRepository.Object, _userRepository.Object);
        }

        #region GetByCardIdAsync
        [Fact]
        public async Task GetByCardIdAsync_NullCardGivenId_ThrowsKeyNotFoundException()
        {
            var cardId = Guid.NewGuid();
            _cardRepository.Setup(x => x.GetByIdAsync(cardId)).ReturnsAsync((CardItem?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetByCardIdAsync(cardId));
        }

        [Fact]
        public async Task GetByCardIdAsync_ValidCardGivenId_ReturnsComments()
        {
            var cardId = Guid.NewGuid();
            var comments = new List<CardComment>
            {
                new CardComment { Id = Guid.NewGuid(), CardId = cardId, AuthorId = Guid.NewGuid(), Message = "Test comment 1" },
                new CardComment { Id = Guid.NewGuid(), CardId = cardId, AuthorId = Guid.NewGuid(), Message = "Test comment 2" }
            };
            _cardRepository.Setup(x => x.GetByIdAsync(cardId)).ReturnsAsync(new CardItem { Id = cardId });
            _commentRepository.Setup(x => x.GetByCardIdAsync(cardId)).ReturnsAsync(comments);

            var result = await _service.GetByCardIdAsync(cardId);

            Assert.Equal(2, result.Count);
            Assert.Equal("Test comment 1", result[0].Message);
            Assert.Equal("Test comment 2", result[1].Message);
        }
        #endregion

        #region DeleteAsync
        [Fact]
        public async Task DeleteAsync_NullCommentGivenId_ReturnsFalse()
        {
            var commentId = Guid.NewGuid();
            _commentRepository.Setup(x => x.GetByIdAsync(commentId)).ReturnsAsync((CardComment?)null);

            var result = await _service.DeleteAsync(commentId);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ValidCommentGivenId_ReturnsTrue()
        {
            var commentId = Guid.NewGuid();
            var comment = new CardComment { Id = commentId, CardId = Guid.NewGuid(), AuthorId = Guid.NewGuid(), Message = "Test comment" };
            _commentRepository.Setup(x => x.GetByIdAsync(commentId)).ReturnsAsync(comment);

            var result = await _service.DeleteAsync(commentId);

            Assert.True(result);
            _commentRepository.Verify(x => x.DeleteAsync(comment), Times.Once);
        }
        #endregion

        #region CreateAsync
        [Fact]
        public async Task CreateAsync_NullCardGivenId_ThrowsKeyNotFoundException()
        {
            var cardId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            _cardRepository.Setup(x => x.GetByIdAsync(cardId)).ReturnsAsync((CardItem?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateAsync(cardId, authorId, "Test message"));
        }

        [Fact]
        public async Task CreateAsync_NullAuthorGivenId_ThrowsKeyNotFoundException()
        {
            var cardId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            _cardRepository.Setup(x => x.GetByIdAsync(cardId)).ReturnsAsync(new CardItem { Id = cardId });
            _userRepository.Setup(x => x.GetByIdAsync(authorId)).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateAsync(cardId, authorId, "Test message"));
        }

        [Fact]
        public async Task CreateAsync_ValidIds_CallsAddAsyncWithExpectedComment()
        {
            var cardId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var rawMessage = "    Test message  ";
            CardComment? captured = null;

            _cardRepository.Setup(x => x.GetByIdAsync(cardId)).ReturnsAsync(new CardItem { Id = cardId });
            _userRepository.Setup(x => x.GetByIdAsync(authorId)).ReturnsAsync(new User { Id = authorId });

            _commentRepository
                .Setup(x => x.AddAsync(It.IsAny<CardComment>()))
                .ReturnsAsync((CardComment c) => { captured = c; return c; });

            var result = await _service.CreateAsync(cardId, authorId, "Test message");

            _commentRepository.Verify(x => x.AddAsync(It.IsAny<CardComment>()), Times.Once);

            Assert.NotNull(result);
            Assert.Equal(cardId, captured!.CardId);
            Assert.Equal(authorId, captured.AuthorId);
            Assert.Equal("Test message", captured.Message);
            Assert.NotEqual(Guid.Empty, captured.Id);
        }
        #endregion
    }
}