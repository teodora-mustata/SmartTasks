using Microsoft.EntityFrameworkCore;
using SmartTasksAPI.Models;
using SmartTasksAPI.Models.Enums;
using SmartTasksAPI.Repositories;

namespace SmartTasksAPI.Tests.Repository;

public class RepositoryTests
{
    [Fact]
    public async Task UserRepository_ShouldPersistAndQueryByEmail()
    {
        using var database = new RepositoryTestDatabase();
        await using var context = database.CreateContext();
        var repository = new UserRepository(context);

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Alice Example",
            Email = "alice@example.com"
        };

        var saved = await repository.AddAsync(user);

        Assert.Equal(user.Id, saved.Id);

        var allUsers = await repository.GetAllAsync();
        Assert.Single(allUsers);
        Assert.Equal("alice@example.com", allUsers[0].Email);

        var foundByEmail = await repository.GetByEmailAsync("alice@example.com");
        Assert.NotNull(foundByEmail);
        Assert.Equal(user.Id, foundByEmail!.Id);
    }

    [Fact]
    public async Task UserRepository_ShouldEnforceUniqueEmail()
    {
        using var database = new RepositoryTestDatabase();
        await using var context = database.CreateContext();
        var repository = new UserRepository(context);

        await repository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Alice Example",
            Email = "duplicate@example.com"
        });

        var secondUser = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Bob Example",
            Email = "duplicate@example.com"
        };

        await Assert.ThrowsAsync<DbUpdateException>(() => repository.AddAsync(secondUser));
    }

    [Fact]
    public async Task BoardRepository_ShouldLoadOwnerMembersAndOrderedLists()
    {
        using var database = new RepositoryTestDatabase();
        await using var context = database.CreateContext();
        var userRepository = new UserRepository(context);
        var boardRepository = new BoardRepository(context);
        var listRepository = new ListRepository(context);

        var owner = await userRepository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Owner",
            Email = "owner@example.com"
        });

        var member = await userRepository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Member",
            Email = "member@example.com"
        });

        var board = await boardRepository.AddAsync(new Board
        {
            Id = Guid.NewGuid(),
            Name = "Board",
            Description = "Board description",
            OwnerId = owner.Id
        });

        await boardRepository.AddMemberAsync(new BoardMember
        {
            BoardId = board.Id,
            UserId = owner.Id,
            Role = BoardRole.Owner
        });

        await boardRepository.AddMemberAsync(new BoardMember
        {
            BoardId = board.Id,
            UserId = member.Id,
            Role = BoardRole.Member
        });

        await context.Lists.AddRangeAsync(
            new BoardList
            {
                Id = Guid.NewGuid(),
                BoardId = board.Id,
                Name = "Second",
                Position = 2
            },
            new BoardList
            {
                Id = Guid.NewGuid(),
                BoardId = board.Id,
                Name = "First",
                Position = 1
            });

        await context.SaveChangesAsync();

        var loadedBoard = await boardRepository.GetByIdAsync(board.Id);

        Assert.NotNull(loadedBoard);
        Assert.Equal(owner.Id, loadedBoard!.OwnerId);
        Assert.NotNull(loadedBoard.Owner);
        Assert.Equal(owner.Email, loadedBoard.Owner!.Email);
        Assert.Equal(2, loadedBoard.Members.Count);
        Assert.Equal(2, loadedBoard.Lists.Count);
        Assert.Contains(loadedBoard.Lists, x => x.Name == "First");
        Assert.Contains(loadedBoard.Lists, x => x.Name == "Second");

        var orderedLists = await listRepository.GetByBoardIdAsync(board.Id);
        Assert.Equal(new[] { "First", "Second" }, orderedLists.Select(x => x.Name).ToArray());
    }

    [Fact]
    public async Task BoardRepository_ShouldTrackMembersAndPreventDuplicateMembership()
    {
        using var database = new RepositoryTestDatabase();
        await using var context = database.CreateContext();
        var userRepository = new UserRepository(context);
        var boardRepository = new BoardRepository(context);

        var owner = await userRepository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Owner",
            Email = "owner2@example.com"
        });

        var board = await boardRepository.AddAsync(new Board
        {
            Id = Guid.NewGuid(),
            Name = "Team Board",
            OwnerId = owner.Id
        });

        var member = await userRepository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Member",
            Email = "member2@example.com"
        });

        await boardRepository.AddMemberAsync(new BoardMember
        {
            BoardId = board.Id,
            UserId = member.Id,
            Role = BoardRole.Member
        });

        Assert.True(await boardRepository.MemberExistsAsync(board.Id, member.Id));

        var storedMember = await boardRepository.GetMemberAsync(board.Id, member.Id);
        Assert.NotNull(storedMember);
        Assert.Equal(BoardRole.Member, storedMember!.Role);

        await boardRepository.RemoveMemberAsync(storedMember);
        Assert.False(await boardRepository.MemberExistsAsync(board.Id, member.Id));
    }

    [Fact]
    public async Task BoardRepository_ShouldEnforceCompositeMemberKey()
    {
        using var database = new RepositoryTestDatabase();
        await using var context = database.CreateContext();
        var userRepository = new UserRepository(context);
        var boardRepository = new BoardRepository(context);

        var owner = await userRepository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Owner",
            Email = "owner3@example.com"
        });

        var board = await boardRepository.AddAsync(new Board
        {
            Id = Guid.NewGuid(),
            Name = "Duplicate Member Board",
            OwnerId = owner.Id
        });

        var member = await userRepository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Member",
            Email = "member3@example.com"
        });

        await boardRepository.AddMemberAsync(new BoardMember
        {
            BoardId = board.Id,
            UserId = member.Id,
            Role = BoardRole.Member
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => boardRepository.AddMemberAsync(new BoardMember
        {
            BoardId = board.Id,
            UserId = member.Id,
            Role = BoardRole.Member
        }));
    }

    [Fact]
    public async Task ListRepository_ShouldOrderByPositionAndIncrementNextPosition()
    {
        using var database = new RepositoryTestDatabase();
        await using var context = database.CreateContext();
        var userRepository = new UserRepository(context);
        var boardRepository = new BoardRepository(context);
        var listRepository = new ListRepository(context);

        var owner = await userRepository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Owner",
            Email = "owner4@example.com"
        });

        var board = await boardRepository.AddAsync(new Board
        {
            Id = Guid.NewGuid(),
            Name = "List Board",
            OwnerId = owner.Id
        });

        var firstPosition = await listRepository.GetNextPositionAsync(board.Id);
        Assert.Equal(1, firstPosition);

        await listRepository.AddAsync(new BoardList
        {
            Id = Guid.NewGuid(),
            BoardId = board.Id,
            Name = "Second",
            Position = 2
        });

        await listRepository.AddAsync(new BoardList
        {
            Id = Guid.NewGuid(),
            BoardId = board.Id,
            Name = "First",
            Position = 1
        });

        var lists = await listRepository.GetByBoardIdAsync(board.Id);
        Assert.Equal(new[] { "First", "Second" }, lists.Select(x => x.Name).ToArray());
        Assert.Equal(3, await listRepository.GetNextPositionAsync(board.Id));
    }

    [Fact]
    public async Task ListRepository_ShouldUpdateAndDelete()
    {
        using var database = new RepositoryTestDatabase();
        await using var context = database.CreateContext();
        var userRepository = new UserRepository(context);
        var boardRepository = new BoardRepository(context);
        var listRepository = new ListRepository(context);

        var owner = await userRepository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Owner",
            Email = "owner5@example.com"
        });

        var board = await boardRepository.AddAsync(new Board
        {
            Id = Guid.NewGuid(),
            Name = "Update Board",
            OwnerId = owner.Id
        });

        var list = await listRepository.AddAsync(new BoardList
        {
            Id = Guid.NewGuid(),
            BoardId = board.Id,
            Name = "Todo",
            Position = 1
        });

        list.Name = "Doing";
        list.Position = 2;
        await listRepository.UpdateAsync(list);

        var loaded = await listRepository.GetByIdAsync(list.Id);
        Assert.NotNull(loaded);
        Assert.Equal("Doing", loaded!.Name);
        Assert.Equal(2, loaded.Position);

        await listRepository.DeleteAsync(loaded);
        Assert.Null(await listRepository.GetByIdAsync(list.Id));
    }

    [Fact]
    public async Task CardRepository_ShouldOrderCardsAndLoadAssignmentsAndComments()
    {
        using var database = new RepositoryTestDatabase();
        await using var context = database.CreateContext();
        var userRepository = new UserRepository(context);
        var boardRepository = new BoardRepository(context);
        var listRepository = new ListRepository(context);
        var cardRepository = new CardRepository(context);
        var commentRepository = new CommentRepository(context);

        var owner = await userRepository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Owner",
            Email = "owner6@example.com"
        });

        var assignee = await userRepository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Assignee",
            Email = "assignee6@example.com"
        });

        var board = await boardRepository.AddAsync(new Board
        {
            Id = Guid.NewGuid(),
            Name = "Card Board",
            OwnerId = owner.Id
        });

        var list = await listRepository.AddAsync(new BoardList
        {
            Id = Guid.NewGuid(),
            BoardId = board.Id,
            Name = "Backlog",
            Position = 1
        });

        await cardRepository.AddAsync(new CardItem
        {
            Id = Guid.NewGuid(),
            ListId = list.Id,
            Title = "Second",
            Position = 2
        });

        var firstCard = await cardRepository.AddAsync(new CardItem
        {
            Id = Guid.NewGuid(),
            ListId = list.Id,
            Title = "First",
            Position = 1
        });

        await cardRepository.AddAssignmentAsync(new CardAssignment
        {
            CardId = firstCard.Id,
            UserId = assignee.Id
        });

        await commentRepository.AddAsync(new CardComment
        {
            Id = Guid.NewGuid(),
            CardId = firstCard.Id,
            AuthorId = assignee.Id,
            Message = "Second comment",
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(1)
        });

        await commentRepository.AddAsync(new CardComment
        {
            Id = Guid.NewGuid(),
            CardId = firstCard.Id,
            AuthorId = assignee.Id,
            Message = "First comment",
            CreatedAtUtc = DateTime.UtcNow
        });

        var cards = await cardRepository.GetByListIdAsync(list.Id);
        Assert.Equal(new[] { "First", "Second" }, cards.Select(x => x.Title).ToArray());

        var loadedCard = await cardRepository.GetByIdAsync(firstCard.Id);
        Assert.NotNull(loadedCard);
        Assert.Single(loadedCard!.Assignments);
        Assert.Equal(assignee.Id, loadedCard.Assignments.First().UserId);
        Assert.Equal(2, loadedCard.Comments.Count);
        Assert.All(loadedCard.Comments, comment => Assert.NotNull(comment.Author));

        var orderedComments = await commentRepository.GetByCardIdAsync(firstCard.Id);
        Assert.Equal(new[] { "First comment", "Second comment" }, orderedComments.Select(x => x.Message).ToArray());
    }

    [Fact]
    public async Task CardRepository_ShouldIncrementPositionAndManageAssignments()
    {
        using var database = new RepositoryTestDatabase();
        await using var context = database.CreateContext();
        var userRepository = new UserRepository(context);
        var boardRepository = new BoardRepository(context);
        var listRepository = new ListRepository(context);
        var cardRepository = new CardRepository(context);

        var owner = await userRepository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Owner",
            Email = "owner7@example.com"
        });

        var assignee = await userRepository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Assignee",
            Email = "assignee7@example.com"
        });

        var board = await boardRepository.AddAsync(new Board
        {
            Id = Guid.NewGuid(),
            Name = "Assignment Board",
            OwnerId = owner.Id
        });

        var list = await listRepository.AddAsync(new BoardList
        {
            Id = Guid.NewGuid(),
            BoardId = board.Id,
            Name = "Doing",
            Position = 1
        });

        Assert.Equal(1, await cardRepository.GetNextPositionAsync(list.Id));

        var card = await cardRepository.AddAsync(new CardItem
        {
            Id = Guid.NewGuid(),
            ListId = list.Id,
            Title = "Task",
            Position = 1
        });

        Assert.Equal(2, await cardRepository.GetNextPositionAsync(list.Id));

        var assignment = new CardAssignment
        {
            CardId = card.Id,
            UserId = assignee.Id
        };

        await cardRepository.AddAssignmentAsync(assignment);
        var loadedAssignment = await cardRepository.GetAssignmentAsync(card.Id, assignee.Id);
        Assert.NotNull(loadedAssignment);

        await cardRepository.RemoveAssignmentAsync(loadedAssignment!);
        Assert.Null(await cardRepository.GetAssignmentAsync(card.Id, assignee.Id));
    }

    [Fact]
    public async Task CardRepository_ShouldEnforceCompositeAssignmentKey()
    {
        using var database = new RepositoryTestDatabase();
        await using var context = database.CreateContext();
        var userRepository = new UserRepository(context);
        var boardRepository = new BoardRepository(context);
        var listRepository = new ListRepository(context);
        var cardRepository = new CardRepository(context);

        var owner = await userRepository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Owner",
            Email = "owner8@example.com"
        });

        var assignee = await userRepository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Assignee",
            Email = "assignee8@example.com"
        });

        var board = await boardRepository.AddAsync(new Board
        {
            Id = Guid.NewGuid(),
            Name = "Composite Board",
            OwnerId = owner.Id
        });

        var list = await listRepository.AddAsync(new BoardList
        {
            Id = Guid.NewGuid(),
            BoardId = board.Id,
            Name = "Backlog",
            Position = 1
        });

        var card = await cardRepository.AddAsync(new CardItem
        {
            Id = Guid.NewGuid(),
            ListId = list.Id,
            Title = "Task",
            Position = 1
        });

        await cardRepository.AddAssignmentAsync(new CardAssignment
        {
            CardId = card.Id,
            UserId = assignee.Id
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => cardRepository.AddAssignmentAsync(new CardAssignment
        {
            CardId = card.Id,
            UserId = assignee.Id
        }));
    }

    [Fact]
    public async Task CommentRepository_ShouldOrderByCreatedAtAndDelete()
    {
        using var database = new RepositoryTestDatabase();
        await using var context = database.CreateContext();
        var userRepository = new UserRepository(context);
        var boardRepository = new BoardRepository(context);
        var listRepository = new ListRepository(context);
        var cardRepository = new CardRepository(context);
        var commentRepository = new CommentRepository(context);

        var owner = await userRepository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Owner",
            Email = "owner9@example.com"
        });

        var author = await userRepository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Author",
            Email = "author9@example.com"
        });

        var board = await boardRepository.AddAsync(new Board
        {
            Id = Guid.NewGuid(),
            Name = "Comment Board",
            OwnerId = owner.Id
        });

        var list = await listRepository.AddAsync(new BoardList
        {
            Id = Guid.NewGuid(),
            BoardId = board.Id,
            Name = "Inbox",
            Position = 1
        });

        var card = await cardRepository.AddAsync(new CardItem
        {
            Id = Guid.NewGuid(),
            ListId = list.Id,
            Title = "Task",
            Position = 1
        });

        var laterComment = await commentRepository.AddAsync(new CardComment
        {
            Id = Guid.NewGuid(),
            CardId = card.Id,
            AuthorId = author.Id,
            Message = "Later",
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(1)
        });

        var earlierComment = await commentRepository.AddAsync(new CardComment
        {
            Id = Guid.NewGuid(),
            CardId = card.Id,
            AuthorId = author.Id,
            Message = "Earlier",
            CreatedAtUtc = DateTime.UtcNow
        });

        var comments = await commentRepository.GetByCardIdAsync(card.Id);
        Assert.Equal(new[] { "Earlier", "Later" }, comments.Select(x => x.Message).ToArray());

        var loadedComment = await commentRepository.GetByIdAsync(earlierComment.Id);
        Assert.NotNull(loadedComment);
        await commentRepository.DeleteAsync(loadedComment!);
        Assert.Null(await commentRepository.GetByIdAsync(earlierComment.Id));
        Assert.NotNull(await commentRepository.GetByIdAsync(laterComment.Id));
    }
}
