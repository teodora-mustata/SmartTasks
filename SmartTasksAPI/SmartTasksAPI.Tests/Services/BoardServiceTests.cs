using Moq;
using SmartTasksAPI.Models;
using SmartTasksAPI.Models.Enums;
using SmartTasksAPI.Repositories;
using SmartTasksAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTasksAPI.Tests.Services
{
    public class BoardServiceTests
    {
        readonly Mock<IBoardRepository> _boardRepository;
        readonly Mock<IUserRepository> _userRepository;
        readonly IBoardService _service;

        public BoardServiceTests()
        {
            _boardRepository = new Mock<IBoardRepository>();
            _userRepository = new Mock<IUserRepository>();
            _service = new BoardService(_boardRepository.Object, _userRepository.Object);
        }

        [Fact]
        public async Task GetAllAsync_CallsRepositoryMethod()
        {
            await _service.GetAllAsync();
            _boardRepository.Verify(x => x.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_CallsRepositoryMethod()
        {
            var boardId = Guid.NewGuid();
            await _service.GetByIdAsync(boardId);
            _boardRepository.Verify(x => x.GetByIdAsync(boardId), Times.Once);
        }

        #region CreateAsync Tests
        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenOwnerDoesNotExist()
        {
            _userRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.CreateAsync("Board", "Desc", Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateAsync_CallsRelevantRepositoryMethods()
        {
            var ownerId = Guid.NewGuid();
            _userRepository.Setup(x => x.GetByIdAsync(ownerId))
                .ReturnsAsync(new User { Id = ownerId, Email = "test@test.com"});
            
            await _service.CreateAsync("Board", "Desc", ownerId);

            _boardRepository.Verify(x => x.AddAsync(It.Is<Board>(b => b.Name == "Board" && b.Description == "Desc" && b.OwnerId == ownerId)), Times.Once);
            _boardRepository.Verify(x => x.AddMemberAsync(It.Is<BoardMember>(m => m.UserId == ownerId && m.Role == BoardRole.Owner)), Times.Once);
            _boardRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ParametersWithExtraSpace_TrimmedBeforeCreating()
        {
            var ownerId = Guid.NewGuid();
            _userRepository.Setup(x => x.GetByIdAsync(ownerId))
                .ReturnsAsync(new User { Id = ownerId, Email = "test@test.com"});

            await _service.CreateAsync("  Board  ", "  Desc  ", ownerId);

            _boardRepository.Verify(x => x.AddAsync(It.Is<Board>(b => b.Name == "Board" && b.Description == "Desc" && b.OwnerId == ownerId)), Times.Once);
            _boardRepository.Verify(x => x.AddMemberAsync(It.Is<BoardMember>(m => m.UserId == ownerId && m.Role == BoardRole.Owner)), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_CreateIsSuccessful_ReturnsCreatedBoard()
        {
            var ownerId = Guid.NewGuid();
            var createdBoard = new Board { Id = Guid.NewGuid(), Name = "Board", Description = "Desc", OwnerId = ownerId };
            _userRepository.Setup(x => x.GetByIdAsync(ownerId))
                .ReturnsAsync(new User { Id = ownerId, Email = "test@test.com" });
            _boardRepository.Setup(x => x.AddAsync(It.IsAny<Board>()))
                .ReturnsAsync(createdBoard);
            _boardRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(createdBoard);

            var result = await _service.CreateAsync("Board", "Desc", ownerId);

            Assert.Equal(createdBoard, result);
        }
        #endregion

        #region UpdateAsync Tests
        [Fact]
        public async Task UpdateAsync_BoardNotFound_ReturnsFalse()
        {
            _boardRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Board?)null);

            var result = await _service.UpdateAsync(Guid.NewGuid(), "New Name", "New Desc");

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAsync_BoardFound_UpdatesBoardWithArguments()
        {
            var testBoard = new Board
            {
                Id = Guid.NewGuid(),
                Name = "Old Name",
                Description = "Old Desc"
            };
            _boardRepository.Setup(x => x.GetByIdAsync(testBoard.Id))
                .ReturnsAsync(testBoard);

            var newName = "New Name";
            var newDesc = "New Desc";
            await _service.UpdateAsync(testBoard.Id, newName, newDesc);

            Assert.Equal(newName, testBoard.Name);
            Assert.Equal(newDesc, testBoard.Description);
        }

        [Fact]
        public async Task UpdateAsync_ParametersWithExtraSpace_TrimsDescriptionBeforeUpdating()
        {
            var testBoard = new Board
            {
                Id = Guid.NewGuid(),
                Name = "Old Name",
                Description = "Old Desc"
            };
            _boardRepository.Setup(x => x.GetByIdAsync(testBoard.Id))
                .ReturnsAsync(testBoard);

            var newName = "  New Name  ";
            var newDesc = "  New Desc  ";
            await _service.UpdateAsync(testBoard.Id, newName, newDesc);

            Assert.Equal("New Name", testBoard.Name);
            Assert.Equal("New Desc", testBoard.Description);
        }

        [Fact]
        public async Task UpdateAsync_NullNewDescription_SetsDescriptionToNull()
        {
            var testBoard = new Board
            {
                Id = Guid.NewGuid(),
                Name = "Old Name",
                Description = "Old Desc"
            };
            _boardRepository.Setup(x => x.GetByIdAsync(testBoard.Id))
                .ReturnsAsync(testBoard);

            await _service.UpdateAsync(testBoard.Id, "New Name", null);

            Assert.Null(testBoard.Description);
        }

        [Fact]
        public async Task UpdateAsync_BoardFound_CallsRepositoryAndReturnsTrues()
        {
            var testBoard = new Board
            {
                Id = Guid.NewGuid(),
                Name = "Test Name",
                Description = "Test Desc"
            };
            _boardRepository.Setup(x => x.GetByIdAsync(testBoard.Id))
                .ReturnsAsync(testBoard);

            var result = await _service.UpdateAsync(testBoard.Id, "New Name", "New Desc");

            Assert.True(result);
            _boardRepository.Verify(x => x.UpdateAsync(testBoard), Times.Once);
        }
        #endregion

        #region DeleteAsync Tests
        [Fact]
        public async Task DeleteAsync_BoardNoteFound_ReturnsFalse()
        {
            _boardRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Board?)null);

            var result = await _service.DeleteAsync(Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_BoardFound_CallsRepositoryAndReturnsTrue()
        {
            var boardId = Guid.NewGuid();
            _boardRepository.Setup(x => x.GetByIdAsync(boardId))
                .ReturnsAsync(new Board { Id = boardId, OwnerId = Guid.NewGuid(), Name = "Board" });

            var result = await _service.DeleteAsync(boardId);

            Assert.True(result);
            _boardRepository.Verify(x => x.DeleteAsync(It.IsAny<Board>()), Times.Once);
        }
        #endregion

        #region AddMemberAsync Tests
        [Fact]
        public async Task AddMemberAsync_BoardNotFound_ReturnsFalse()
        {
            _boardRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Board?)null);

            var result = await _service.AddMemberAsync(Guid.NewGuid(), Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public async Task AddMemberAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            var boardId = Guid.NewGuid();

            _boardRepository.Setup(x => x.GetByIdAsync(boardId))
                .ReturnsAsync(new Board { Id = boardId, OwnerId = Guid.NewGuid(), Name = "Board" });
            _userRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.AddMemberAsync(boardId, Guid.NewGuid()));
        }

        [Fact]
        public async Task AddMemberAsync_ShouldThrow_WhenMemberAlreadyExists()
        {
            var boardId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _boardRepository.Setup(x => x.GetByIdAsync(boardId))
                .ReturnsAsync(new Board { Id = boardId, OwnerId = Guid.NewGuid(), Name = "Board" });
            _userRepository.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(new User { Id = userId, Email = "a@a.com", FullName = "A" });
            _boardRepository.Setup(x => x.MemberExistsAsync(boardId, userId))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddMemberAsync(boardId, userId));
        }

        [Fact]
        public async Task AddMemberAsync_ShouldAddMember_WhenSuccessful()
        {
            var boardId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _boardRepository.Setup(x => x.GetByIdAsync(boardId))
                .ReturnsAsync(new Board { Id = boardId, OwnerId = Guid.NewGuid(), Name = "Board" });
            _userRepository.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(new User { Id = userId, Email = "test@test.com"});
            _boardRepository.Setup(x => x.MemberExistsAsync(boardId, userId))
                .ReturnsAsync(false);
            
            var result = await _service.AddMemberAsync(boardId, userId);

            Assert.True(result);
            _boardRepository.Verify(x => x.AddMemberAsync(It.Is<BoardMember>(m => m.BoardId == boardId && m.UserId == userId && m.Role == BoardRole.Member)), Times.Once);
        }
        #endregion

        #region RemoveMemberAsync Tests
        [Fact]
        public async Task RemoveMemberAsync_ShouldThrow_WhenTryingToRemoveOwner()
        {
            var boardId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            _boardRepository.Setup(x => x.GetByIdAsync(boardId))
                .ReturnsAsync(new Board { Id = boardId, OwnerId = ownerId, Name = "Board" });

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.RemoveMemberAsync(boardId, ownerId));
        }

        [Fact]
        public async Task RemoveMemberAsync_WhenBoardIsNotFound_ReturnsFalse()
        {
            _boardRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Board?)null);

            var result = await _service.RemoveMemberAsync(Guid.NewGuid(), Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public async Task RemoveMemberAsync_WhenMemberIsNotFound_ReturnsFalse()
        {
            var boardId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _boardRepository.Setup(x => x.GetByIdAsync(boardId))
                .ReturnsAsync(new Board { Id = boardId, OwnerId = Guid.NewGuid(), Name = "Board" });
            _boardRepository.Setup(x => x.GetMemberAsync(boardId, userId))
                .ReturnsAsync((BoardMember?)null);

            var result = await _service.RemoveMemberAsync(boardId, userId);

            Assert.False(result);
        }

        [Fact]
        public async Task RemoveMemberAsync_WhenSuccessful_ReturnsTrue()
        {
            var boardId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _boardRepository.Setup(x => x.GetByIdAsync(boardId))
                .ReturnsAsync(new Board { Id = boardId, OwnerId = Guid.NewGuid(), Name = "Board" });
            _boardRepository.Setup(x => x.GetMemberAsync(boardId, userId))
                .ReturnsAsync(new BoardMember { BoardId = boardId, UserId = userId });

            var result = await _service.RemoveMemberAsync(boardId, userId);

            Assert.True(result);
            _boardRepository.Verify(x => x.RemoveMemberAsync(It.Is<BoardMember>(m => m.BoardId == boardId && m.UserId == userId)), Times.Once);
        }
        #endregion
    }
}
