using Moq;
using SmartTasksAPI.Models;
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
        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenOwnerDoesNotExist()
        {
            var boardRepository = new Mock<IBoardRepository>();
            var userRepository = new Mock<IUserRepository>();

            userRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((User?)null);

            var service = new BoardService(boardRepository.Object, userRepository.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.CreateAsync("Board", "Desc", Guid.NewGuid()));
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
            boardRepository.Setup(x => x.MemberExistsAsync(boardId, userId))
                .ReturnsAsync(true);

            var service = new BoardService(boardRepository.Object, userRepository.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AddMemberAsync(boardId, userId));
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

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.RemoveMemberAsync(boardId, ownerId));
        }
    }
}
