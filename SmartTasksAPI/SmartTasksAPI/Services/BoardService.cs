using SmartTasksAPI.Models;
using SmartTasksAPI.Models.Enums;
using SmartTasksAPI.Repositories;

namespace SmartTasksAPI.Services
{
    public class BoardService(IBoardRepository boardRepository, IUserRepository userRepository) : IBoardService
    {
        public Task<List<Board>> GetAllAsync() => boardRepository.GetAllAsync();

        public Task<Board?> GetByIdAsync(Guid boardId) => boardRepository.GetByIdAsync(boardId);

        public async Task<Board> CreateAsync(string name, string? description, Guid ownerId)
        {
            var owner = await userRepository.GetByIdAsync(ownerId);
            if (owner is null)
            {
                throw new KeyNotFoundException("Owner not found.");
            }

            var board = new Board
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                Description = description?.Trim(),
                OwnerId = ownerId
            };

            await boardRepository.AddAsync(board);
            await boardRepository.AddMemberAsync(new BoardMember
            {
                BoardId = board.Id,
                UserId = ownerId,
                Role = BoardRole.Owner
            });

            return (await boardRepository.GetByIdAsync(board.Id))!;
        }

        public async Task<bool> UpdateAsync(Guid boardId, string name, string? description)
        {
            var board = await boardRepository.GetByIdAsync(boardId);
            if (board is null)
            {
                return false;
            }

            board.Name = name.Trim();
            board.Description = description?.Trim();
            await boardRepository.UpdateAsync(board);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid boardId)
        {
            var board = await boardRepository.GetByIdAsync(boardId);
            if (board is null)
            {
                return false;
            }

            await boardRepository.DeleteAsync(board);
            return true;
        }

        public async Task<bool> AddMemberAsync(Guid boardId, Guid userId)
        {
            var board = await boardRepository.GetByIdAsync(boardId);
            if (board is null)
            {
                return false;
            }

            var user = await userRepository.GetByIdAsync(userId);
            if (user is null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var exists = await boardRepository.MemberExistsAsync(boardId, userId);
            if (exists)
            {
                throw new InvalidOperationException("User is already a board member.");
            }

            await boardRepository.AddMemberAsync(new BoardMember
            {
                BoardId = boardId,
                UserId = userId,
                Role = BoardRole.Member
            });

            return true;
        }

        public async Task<bool> RemoveMemberAsync(Guid boardId, Guid userId)
        {
            var board = await boardRepository.GetByIdAsync(boardId);
            if (board is null)
            {
                return false;
            }

            if (board.OwnerId == userId)
            {
                throw new InvalidOperationException("Board owner cannot be removed.");
            }

            var member = await boardRepository.GetMemberAsync(boardId, userId);
            if (member is null)
            {
                return false;
            }

            await boardRepository.RemoveMemberAsync(member);
            return true;
        }
    }
}