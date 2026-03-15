using SmartTasksAPI.Models;
using SmartTasksAPI.Repositories;

namespace SmartTasksAPI.Services
{
    public class CommentService(ICommentRepository commentRepository, ICardRepository cardRepository, IUserRepository userRepository) : ICommentService
    {
        public async Task<List<CardComment>> GetByCardIdAsync(Guid cardId)
        {
            var card = await cardRepository.GetByIdAsync(cardId);
            if (card is null)
            {
                throw new KeyNotFoundException("Card not found.");
            }

            return await commentRepository.GetByCardIdAsync(cardId);
        }

        public async Task<CardComment> CreateAsync(Guid cardId, Guid authorId, string message)
        {
            var card = await cardRepository.GetByIdAsync(cardId);
            if (card is null)
            {
                throw new KeyNotFoundException("Card not found.");
            }

            var author = await userRepository.GetByIdAsync(authorId);
            if (author is null)
            {
                throw new KeyNotFoundException("Author not found.");
            }

            var comment = new CardComment
            {
                Id = Guid.NewGuid(),
                CardId = cardId,
                AuthorId = authorId,
                Message = message.Trim()
            };

            return await commentRepository.AddAsync(comment);
        }

        public async Task<bool> DeleteAsync(Guid commentId)
        {
            var comment = await commentRepository.GetByIdAsync(commentId);
            if (comment is null)
            {
                return false;
            }

            await commentRepository.DeleteAsync(comment);
            return true;
        }
    }

}
