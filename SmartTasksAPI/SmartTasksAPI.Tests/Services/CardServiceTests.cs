using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
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
        Mock<ICardRepository> _cardRepository = null!;
        Mock<IListRepository> _listRepository = null!;
        Mock<IUserRepository> _userRepository = null!;
        ICardService _service = null!;

        public CardServiceTests()
        {
            _cardRepository = new Mock<ICardRepository>();
            _listRepository = new Mock<IListRepository>();
            _userRepository = new Mock<IUserRepository>();
            _service = new CardService(_cardRepository.Object, _listRepository.Object, _userRepository.Object);
        }

        #region AssignUserAsync Tests
        [Fact]
        public async Task AssignUserAsync_ShouldThrow_WhenUserAlreadyAssigned()
        {
            var cardId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _cardRepository.Setup(x => x.GetByIdAsync(cardId))
                .ReturnsAsync(new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" });
            _userRepository.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(new User { Id = userId, Email = "user@test.com", FullName = "User" });
            _cardRepository.Setup(x => x.GetAssignmentAsync(cardId, userId))
                .ReturnsAsync(new CardAssignment { CardId = cardId, UserId = userId });

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AssignUserAsync(cardId, userId));
        }

        [Fact]
        public async Task AssignUserAsync_NullCardReturnedFromRepo_ReturnsFalse()
        {
            var cardId = Guid.NewGuid();
            _cardRepository.Setup(x => x.GetByIdAsync(cardId)).ReturnsAsync((CardItem?)null);

            var result = await _service.AssignUserAsync(cardId, Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public async Task AssignUserAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            var cardId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _cardRepository.Setup(x => x.GetByIdAsync(cardId))
                .ReturnsAsync(new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" });
            _userRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.AssignUserAsync(cardId, userId));
        }

        [Fact]
        public async Task AssignUserAsync_UserIsNotAssignedYet_AssignsAndReturnsTrue()
        {
            var cardId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _cardRepository.Setup(x => x.GetByIdAsync(cardId))
                .ReturnsAsync(new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" });
            _userRepository.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(new User { Id = userId, Email = "test@test.com", FullName = "Test User" });
            _cardRepository.Setup(x => x.GetAssignmentAsync(cardId, userId)).ReturnsAsync((CardAssignment?)null);

            var result = await _service.AssignUserAsync(cardId, userId);

            Assert.True(result);
            _cardRepository.Verify(x => x.AddAssignmentAsync(It.Is<CardAssignment>(a => a.CardId == cardId && a.UserId == userId)), Times.Once);
        }
        #endregion

        #region UnAssignUserAsync Tests
        [Fact]
        public async Task UnassignUserAsync_NullCardReturnedFromRepo_ReturnsFalse()
        {
            var cardId = Guid.NewGuid();
            _cardRepository.Setup(x => x.GetByIdAsync(cardId)).ReturnsAsync((CardItem?)null);

            var result = await _service.UnassignUserAsync(cardId, Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public async Task UnassignUserAsync_NoAssignmentFound_ReturnsFalse()
        {
            var cardId = Guid.NewGuid();
            _cardRepository.Setup(x => x.GetByIdAsync(cardId))
                .ReturnsAsync(new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" });
            _cardRepository.Setup(x => x.GetAssignmentAsync(cardId, It.IsAny<Guid>())).ReturnsAsync((CardAssignment?)null);

            var result = await _service.UnassignUserAsync(cardId, Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public async Task UnassignUserAsync_AssignmentFound_CallsRepoWithAssignmentAndReturnsTrue()
        {
            var cardId = Guid.NewGuid();
            _cardRepository.Setup(x => x.GetByIdAsync(cardId))
                .ReturnsAsync(new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" });
            _cardRepository.Setup(x => x.GetAssignmentAsync(cardId, It.IsAny<Guid>()))
                .ReturnsAsync(new CardAssignment { CardId = cardId, UserId = Guid.NewGuid() });

            var result = await _service.UnassignUserAsync(cardId, Guid.NewGuid());

            Assert.True(result);
            _cardRepository.Verify(x => x.RemoveAssignmentAsync(It.Is<CardAssignment>(a => a.CardId == cardId)), Times.Once);
        }
        #endregion

        #region DeleteAsync Tests
        [Fact]
        public async Task DeleteAsync_CardNotFound_ReturnsFalse()
        {
            var cardId = Guid.NewGuid();
            _cardRepository.Setup(x => x.GetByIdAsync(cardId)).ReturnsAsync((CardItem?)null);

            var result = await _service.DeleteAsync(cardId);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_CardFound_DeletesAndReturnsTrue()
        {
            var cardId = Guid.NewGuid();
            _cardRepository.Setup(x => x.GetByIdAsync(cardId))
                .ReturnsAsync(new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" });

            var result = await _service.DeleteAsync(cardId);

            Assert.True(result);
            _cardRepository.Verify(x => x.DeleteAsync(It.Is<CardItem>(x => x.Id == cardId)), Times.Once);
        }
        #endregion

        #region MoveAsync Tests
        [Fact]
        public async Task MoveAsync_CardNotFound_ReturnsFalse()
        {
            var cardId = Guid.NewGuid();
            _cardRepository.Setup(x => x.GetByIdAsync(cardId)).ReturnsAsync((CardItem?)null);

            var result = await _service.MoveAsync(cardId, Guid.NewGuid(), 0);

            Assert.False(result);
        }

        [Fact]
        public async Task MoveAsync_TargetListNotFound_ThrowsKeyNotFoundException()
        {
            var cardId = Guid.NewGuid();
            _cardRepository.Setup(x => x.GetByIdAsync(cardId))
                .ReturnsAsync(new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" });
            _listRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((BoardList?)null);

            var result = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.MoveAsync(cardId, Guid.NewGuid(), 0));
        }

        [Fact]
        public async Task MoveAsync_TargetListFound_UpdatesFoundCardProperties()
        {
            var targetListId = Guid.NewGuid();
            var targetPosition = 0;
            var cardId = Guid.NewGuid();

            var testCardItem = new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" };

            _cardRepository.Setup(x => x.GetByIdAsync(cardId))
                .ReturnsAsync(testCardItem);
            _listRepository.Setup(x => x.GetByIdAsync(targetListId))
                .ReturnsAsync(new BoardList { Id = Guid.NewGuid(), BoardId = targetListId});

            await _service.MoveAsync(cardId, targetListId, 0);

            Assert.Equal(testCardItem.ListId, targetListId);
            Assert.Equal(testCardItem.Position, targetPosition);
        }

        [Fact]
        public async Task MoveAsync_TargetListFound_UpdatesCardInRepositoryAndReturnsTrue()
        {
            var targetListId = Guid.NewGuid();
            var targetPosition = 0;
            var cardId = Guid.NewGuid();
            _cardRepository.Setup(x => x.GetByIdAsync(cardId))
                .ReturnsAsync(new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" });
            _listRepository.Setup(x => x.GetByIdAsync(targetListId))
                .ReturnsAsync(new BoardList { Id = Guid.NewGuid(), BoardId = targetListId });

            var result = await _service.MoveAsync(cardId, targetListId, targetPosition);

            Assert.True(result);
            _cardRepository.Verify(x => x.UpdateAsync(It.Is<CardItem>(c => c.Id == cardId && c.ListId == targetListId && c.Position == targetPosition)), Times.Once);
        }
        #endregion

        #region UpdateAsync Tests
        [Fact]
        public async Task UpdateAsync_CardNotFound_ReturnsFalse()
        {
            var cardId = Guid.NewGuid();
            _cardRepository.Setup(x => x.GetByIdAsync(cardId)).ReturnsAsync((CardItem?)null);

            var result = await _service.UpdateAsync(cardId, "New Title", "New Description", 0, DateTime.UtcNow);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAsync_CardFound_ItemUpdatedWithParameters()
        {
            var cardId = Guid.NewGuid();
            string newTitle = "New Title";
            string newDescription = "New Description";
            int position = 0;
            var dueDate = DateTime.UtcNow;

            var testCardItem = new CardItem 
            { 
                Id = cardId, ListId = Guid.NewGuid(), 
                Title = "Task", Description = "Description", 
                Position = 1, DueDateUtc = DateTime.UtcNow.AddDays(-7) 
            };

            _cardRepository.Setup(x => x.GetByIdAsync(cardId))
                .ReturnsAsync(testCardItem);

            await _service.UpdateAsync(cardId, newTitle, newDescription, position, dueDate);

            Assert.Equal(newTitle, testCardItem.Title);
            Assert.Equal(newDescription, testCardItem.Description);
            Assert.Equal(position, testCardItem.Position);
            Assert.Equal(dueDate, testCardItem.DueDateUtc);
        }

        [Fact]
        public async Task UpdateAsync_CardFound_UpdatesCardAndReturnsTrue()
        {
            var cardId = Guid.NewGuid();
            _cardRepository.Setup(x => x.GetByIdAsync(cardId))
                .ReturnsAsync(new CardItem { Id = cardId, ListId = Guid.NewGuid(), Title = "Task" });

            var result = await _service.UpdateAsync(cardId, "New Title", "New Description", 0, DateTime.UtcNow);

            Assert.True(result);
            _cardRepository.Verify(x => x.UpdateAsync(It.Is<CardItem>(c => c.Id == cardId && c.Title == "New Title" && c.Description == "New Description")), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_NewDescriptionHasExtraSpace_UpdatedValueIsTrimmed()
        {
            var cardId = Guid.NewGuid();
            var newDescription = "  New Description  ";
            var trimmedDescription = "New Description";

            var testCardItem = new CardItem
            {
                Id = cardId,
                ListId = Guid.NewGuid(),
                Title = "Task",
                Description = "Old Description"
            };

            _cardRepository.Setup(x => x.GetByIdAsync(cardId))
                .ReturnsAsync(testCardItem);

            await _service.UpdateAsync(cardId, "New Title", newDescription, 0, DateTime.UtcNow);

            Assert.Equal(trimmedDescription, testCardItem.Description);
        }

        [Fact]
        public async Task UpdateAsync_NullDescription_UpdatedCardHasNullDescription()
        {
            var cardId = Guid.NewGuid();
            var testCardItem = new CardItem
            {
                Id = cardId,
                ListId = Guid.NewGuid(),
                Title = "Task",
                Description = "Old Description"
            };
            _cardRepository.Setup(x => x.GetByIdAsync(cardId))
                .ReturnsAsync(testCardItem);

            await _service.UpdateAsync(cardId, "New Title", null, 0, DateTime.UtcNow);

            Assert.Null(testCardItem.Description);
        }
        #endregion

        #region CreateAsync Tests
        [Fact]
        public async Task CreateAsync_ListNotFound_ThrowsKeyNotFoundException()
        {
            _listRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((BoardList?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _service.CreateAsync(Guid.NewGuid(), "Title", "Description", DateTime.UtcNow));
        }

        [Fact]
        public async Task CreateAsync_CreatesCard_WithBaseProperties()
        {
            var listId = Guid.NewGuid();
            var testDescription = "Description";
            var testTitle = "Title";
            var testDueDateUtc = DateTime.UtcNow;
            var testPosition = 5;
            _listRepository.Setup(x => x.GetByIdAsync(listId)).ReturnsAsync(new BoardList { Id = listId, BoardId = Guid.NewGuid() });
            _cardRepository.Setup(x => x.GetNextPositionAsync(listId)).ReturnsAsync(testPosition);

            await _service.CreateAsync(listId, testTitle, testDescription, testDueDateUtc);

            _cardRepository.Verify(x => x.AddAsync(It.Is<CardItem>(c =>
                c.ListId == listId &&
                c.Title == testTitle &&
                c.Description == testDescription &&
                c.DueDateUtc == testDueDateUtc &&
                c.Position == testPosition
            )), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_FieldsWithExtraSpace_NewObjectTrimsFields()
        {
            var listId = Guid.NewGuid();
            var testDescription = "  Description  ";
            var testTitle = "  Title  ";
            var trimmedDescription = "Description";
            var trimmedTitle = "Title";

            _listRepository.Setup(x => x.GetByIdAsync(listId)).ReturnsAsync(new BoardList { Id = listId, BoardId = Guid.NewGuid() });

            await _service.CreateAsync(listId, testTitle, testDescription, null);

            _cardRepository.Verify(x => x.AddAsync(It.Is<CardItem>(c =>
                c.ListId == listId &&
                c.Title == trimmedTitle &&
                c.Description == trimmedDescription
            )), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_NullDescription_NewObjectHasNullDescription()
        {
            var listId = Guid.NewGuid();
            var testTitle = "Title";
            _listRepository.Setup(x => x.GetByIdAsync(listId)).ReturnsAsync(new BoardList { Id = listId, BoardId = Guid.NewGuid() });
            
            await _service.CreateAsync(listId, testTitle, null, null);
            
            _cardRepository.Verify(x => x.AddAsync(It.Is<CardItem>(c =>
                c.ListId == listId &&
                c.Title == testTitle &&
                c.Description == null
            )), Times.Once);
        }
        #endregion

        #region GetByIdAsync Tests
        [Fact]
        public async Task GetByIdAsync_CallsRepository()
        {
            var cardId = Guid.NewGuid();
            await _service.GetByIdAsync(cardId);
            _cardRepository.Verify(x => x.GetByIdAsync(cardId), Times.Once);
        }
        #endregion

        #region GetByListIdAsync Tests
        [Fact]
        public async Task GetByListIdAsync_ListNotFound_ThrowsKeyNotFoundException()
        {
            var listId = Guid.NewGuid();
            _listRepository.Setup(x => x.GetByIdAsync(listId)).ReturnsAsync((BoardList?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.GetByListIdAsync(listId));
        }

        [Fact]
        public async Task GetByListidAsync_FoundList_CallsCardRepository()
        {
            var listId = Guid.NewGuid();
            _listRepository.Setup(x => x.GetByIdAsync(listId)).ReturnsAsync(new BoardList { Id = listId, BoardId = Guid.NewGuid() });

            await _service.GetByListIdAsync(listId);

            _cardRepository.Verify(x => x.GetByListIdAsync(listId), Times.Once);
        }
        #endregion
    }

}
