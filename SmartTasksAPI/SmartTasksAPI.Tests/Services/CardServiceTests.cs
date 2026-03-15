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

    public class CardServiceTests
    {
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

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AssignUserAsync(cardId, userId));
        }
    }

}
