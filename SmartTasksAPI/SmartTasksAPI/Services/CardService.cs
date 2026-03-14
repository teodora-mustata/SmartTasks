using SmartTasksAPI.Models;
using SmartTasksAPI.Repositories;

namespace SmartTasksAPI.Services
{

    public class CardService(ICardRepository cardRepository, IListRepository listRepository, IUserRepository userRepository) : ICardService
    {
        public async Task<List<CardItem>> GetByListIdAsync(Guid listId)
        {
            var list = await listRepository.GetByIdAsync(listId);
            if (list is null)
            {
                throw new KeyNotFoundException("List not found.");
            }

            return await cardRepository.GetByListIdAsync(listId);
        }

        public Task<CardItem?> GetByIdAsync(Guid cardId) => cardRepository.GetByIdAsync(cardId);

        public async Task<CardItem> CreateAsync(Guid listId, string title, string? description, DateTime? dueDateUtc)
        {
            var list = await listRepository.GetByIdAsync(listId);
            if (list is null)
            {
                throw new KeyNotFoundException("List not found.");
            }

            var card = new CardItem
            {
                Id = Guid.NewGuid(),
                ListId = listId,
                Title = title.Trim(),
                Description = description?.Trim(),
                Position = await cardRepository.GetNextPositionAsync(listId),
                DueDateUtc = dueDateUtc
            };

            return await cardRepository.AddAsync(card);
        }

        public async Task<bool> UpdateAsync(Guid cardId, string title, string? description, int position, DateTime? dueDateUtc)
        {
            var card = await cardRepository.GetByIdAsync(cardId);
            if (card is null)
            {
                return false;
            }

            card.Title = title.Trim();
            card.Description = description?.Trim();
            card.Position = position;
            card.DueDateUtc = dueDateUtc;
            await cardRepository.UpdateAsync(card);
            return true;
        }

        public async Task<bool> MoveAsync(Guid cardId, Guid targetListId, int targetPosition)
        {
            var card = await cardRepository.GetByIdAsync(cardId);
            if (card is null)
            {
                return false;
            }

            var targetList = await listRepository.GetByIdAsync(targetListId);
            if (targetList is null)
            {
                throw new KeyNotFoundException("Target list not found.");
            }

            card.ListId = targetListId;
            card.Position = targetPosition;
            await cardRepository.UpdateAsync(card);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid cardId)
        {
            var card = await cardRepository.GetByIdAsync(cardId);
            if (card is null)
            {
                return false;
            }

            await cardRepository.DeleteAsync(card);
            return true;
        }

        public async Task<bool> AssignUserAsync(Guid cardId, Guid userId)
        {
            var card = await cardRepository.GetByIdAsync(cardId);
            if (card is null)
            {
                return false;
            }

            var user = await userRepository.GetByIdAsync(userId);
            if (user is null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var existing = await cardRepository.GetAssignmentAsync(cardId, userId);
            if (existing is not null)
            {
                throw new InvalidOperationException("User is already assigned to this card.");
            }

            await cardRepository.AddAssignmentAsync(new CardAssignment
            {
                CardId = cardId,
                UserId = userId
            });

            return true;
        }

        public async Task<bool> UnassignUserAsync(Guid cardId, Guid userId)
        {
            var card = await cardRepository.GetByIdAsync(cardId);
            if (card is null)
            {
                return false;
            }

            var assignment = await cardRepository.GetAssignmentAsync(cardId, userId);
            if (assignment is null)
            {
                return false;
            }

            await cardRepository.RemoveAssignmentAsync(assignment);
            return true;
        }
    }
}
