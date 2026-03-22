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
    public class ListServiceTests
    {
        readonly Mock<IListRepository> _listRepository;
        readonly Mock<IBoardRepository> _boardRepository;
        readonly IListService _service;

        public ListServiceTests()
        {
            _listRepository = new Mock<IListRepository>();
            _boardRepository = new Mock<IBoardRepository>();
            _service = new ListService(_listRepository.Object, _boardRepository.Object);
        }

        #region GetByBoardIdAsync Tests
        [Fact]
        public async Task GetByBoardIdAsync_BoardNotFound_ThrowsKeyNotFoundException()
        {
            var boardId = Guid.NewGuid();
            _boardRepository.Setup(x => x.GetByIdAsync(boardId)).ReturnsAsync((Board?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetByBoardIdAsync(boardId));
        }

        [Fact]
        public async Task GetByBoardIdAsync_BoardExists_ReturnsLists()
        {
            var boardId = Guid.NewGuid();
            var lists = new List<BoardList>
            {
                new BoardList { Id = Guid.NewGuid(), BoardId = boardId, Name = "List 1", Position = 0 },
                new BoardList { Id = Guid.NewGuid(), BoardId = boardId, Name = "List 2", Position = 1 }
            };
            _boardRepository.Setup(x => x.GetByIdAsync(boardId)).ReturnsAsync(new Board { Id = boardId });
            _listRepository.Setup(x => x.GetByBoardIdAsync(boardId)).ReturnsAsync(lists);

            var result = await _service.GetByBoardIdAsync(boardId);

            Assert.Equal(2, result.Count);
            Assert.Equal("List 1", result[0].Name);
            Assert.Equal("List 2", result[1].Name);
        }
        #endregion

        #region GetByIdAsync Tests
        [Fact]
        public async Task GetByIdAsync_CallsRepositoryMethod()
        {
            var listId = Guid.NewGuid();

            await _service.GetByIdAsync(listId);

            _listRepository.Verify(x => x.GetByIdAsync(listId), Times.Once);
        }
        #endregion

        #region CreateAsync Tests
        [Fact]
        public async Task CreateAsync_BoardNotFound_ThrowsKeyNotFoundException()
        {
            var boardId = Guid.NewGuid();
            _boardRepository.Setup(x => x.GetByIdAsync(boardId)).ReturnsAsync((Board?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateAsync(boardId, "New List"));
        }

        [Fact]
        public async Task CreateAsync_BoardExists_CreatesListWithValues()
        {
            var boardId = Guid.NewGuid();
            var listName = " New List ";
            var testPosition = 2;
            _boardRepository.Setup(x => x.GetByIdAsync(boardId)).ReturnsAsync(new Board { Id = boardId });
            _listRepository.Setup(x => x.GetNextPositionAsync(boardId)).ReturnsAsync(testPosition);
            _listRepository.Setup(x => x.AddAsync(It.IsAny<BoardList>())).ReturnsAsync((BoardList list) => list);

            await _service.CreateAsync(boardId, listName);

            _listRepository.Verify(x => x.AddAsync(It.Is<BoardList>(l =>
                l.BoardId == boardId &&
                l.Name == listName.Trim() &&
                l.Position == testPosition
            )), Times.Once);
        }
        #endregion

        #region UpdateAsync Tests
        [Fact]
        public async Task UpdateAsync_ListNotFound_ReturnsFalse()
        {
            var listId = Guid.NewGuid();
            _listRepository.Setup(x => x.GetByIdAsync(listId)).ReturnsAsync((BoardList?)null);

            var result = await _service.UpdateAsync(listId, "Updated Name", 1);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAsync_ListExists_UpdatesValuesAndReturnsTrue()
        {
            var listId = Guid.NewGuid();
            var existingList = new BoardList
            {
                Id = listId,
                BoardId = Guid.NewGuid(),
                Name = "Old Name",
                Position = 0
            };
            _listRepository.Setup(x => x.GetByIdAsync(listId)).ReturnsAsync(existingList);

            var result = await _service.UpdateAsync(listId, " Updated Name ", 1);

            Assert.True(result);
            Assert.Equal("Updated Name", existingList.Name);
            Assert.Equal(1, existingList.Position);
            _listRepository.Verify(x => x.UpdateAsync(existingList), Times.Once);
        }
        #endregion

        #region DeleteAsync Tests
        [Fact]
        public async Task DeleteAsync_ListNotFound_ReturnsFalse()
        {
            var listId = Guid.NewGuid();
            _listRepository.Setup(x => x.GetByIdAsync(listId)).ReturnsAsync((BoardList?)null);

            var result = await _service.DeleteAsync(listId);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ListExists_DeletesListAndReturnsTrue()
        {
            var listId = Guid.NewGuid();
            var existingList = new BoardList { Id = listId };
            _listRepository.Setup(x => x.GetByIdAsync(listId)).ReturnsAsync(existingList);

            var result = await _service.DeleteAsync(listId);

            Assert.True(result);
            _listRepository.Verify(x => x.DeleteAsync(existingList), Times.Once);
        }
        #endregion
    }
}
