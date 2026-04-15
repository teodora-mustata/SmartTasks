using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace SmartTasksAPI.Tests.Integration;

public class ApiIntegrationTests
{
    [Fact]
    public async Task FullWorkflow_ShouldCreateAndFetchEntitiesAcrossAllControllers()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var ownerId = await CreateUserAsync(client, "Owner One", "owner.integration@example.com");

        var createBoardResponse = await client.PostAsJsonAsync("/api/boards", new
        {
            name = "Integration Board",
            description = "Board for integration testing",
            ownerId
        });

        Assert.Equal(HttpStatusCode.Created, createBoardResponse.StatusCode);
        var boardId = await ReadIdAsync(createBoardResponse);

        var secondUserId = await CreateUserAsync(client, "Member One", "member.integration@example.com");

        var addMemberResponse = await client.PostAsJsonAsync($"/api/boards/{boardId}/members", new
        {
            userId = secondUserId
        });

        Assert.Equal(HttpStatusCode.NoContent, addMemberResponse.StatusCode);

        var createListResponse = await client.PostAsJsonAsync($"/api/boards/{boardId}/lists", new
        {
            name = "To Do"
        });

        Assert.Equal(HttpStatusCode.Created, createListResponse.StatusCode);
        var listId = await ReadIdAsync(createListResponse);

        var createCardResponse = await client.PostAsJsonAsync($"/api/lists/{listId}/cards", new
        {
            title = "Implement Task 4",
            description = "Add integration tests",
            dueDateUtc = DateTime.UtcNow.AddDays(2)
        });

        Assert.Equal(HttpStatusCode.Created, createCardResponse.StatusCode);
        var cardId = await ReadIdAsync(createCardResponse);

        var assignResponse = await client.PostAsync($"/api/cards/{cardId}/assignments/{secondUserId}", content: null);
        Assert.Equal(HttpStatusCode.NoContent, assignResponse.StatusCode);

        var createCommentResponse = await client.PostAsJsonAsync($"/api/cards/{cardId}/comments", new
        {
            authorId = secondUserId,
            message = "Looks good"
        });

        Assert.Equal(HttpStatusCode.Created, createCommentResponse.StatusCode);

        var boardByIdResponse = await client.GetAsync($"/api/boards/{boardId}");
        Assert.Equal(HttpStatusCode.OK, boardByIdResponse.StatusCode);

        var boardListsResponse = await client.GetAsync($"/api/boards/{boardId}/lists");
        Assert.Equal(HttpStatusCode.OK, boardListsResponse.StatusCode);

        var listCardsResponse = await client.GetAsync($"/api/lists/{listId}/cards");
        Assert.Equal(HttpStatusCode.OK, listCardsResponse.StatusCode);

        var cardCommentsResponse = await client.GetAsync($"/api/cards/{cardId}/comments");
        Assert.Equal(HttpStatusCode.OK, cardCommentsResponse.StatusCode);

        var commentsJson = await cardCommentsResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, commentsJson.ValueKind);
        Assert.True(commentsJson.GetArrayLength() >= 1);
    }

    [Fact]
    public async Task CreateBoard_WithMissingOwner_ShouldReturnNotFound()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/boards", new
        {
            name = "Orphan Board",
            description = "No owner",
            ownerId = Guid.NewGuid()
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_ShouldReturnConflict()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var first = await client.PostAsJsonAsync("/api/users", new
        {
            fullName = "Duplicate User",
            email = "duplicate.integration@example.com"
        });

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsJsonAsync("/api/users", new
        {
            fullName = "Duplicate User 2",
            email = "duplicate.integration@example.com"
        });

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task AddMember_Twice_ShouldReturnConflictOnSecondAttempt()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var ownerId = await CreateUserAsync(client, "Owner Two", "owner2.integration@example.com");
        var memberId = await CreateUserAsync(client, "Member Two", "member2.integration@example.com");

        var boardResponse = await client.PostAsJsonAsync("/api/boards", new
        {
            name = "Members Board",
            description = "Board members",
            ownerId
        });

        var boardId = await ReadIdAsync(boardResponse);

        var firstAdd = await client.PostAsJsonAsync($"/api/boards/{boardId}/members", new
        {
            userId = memberId
        });

        Assert.Equal(HttpStatusCode.NoContent, firstAdd.StatusCode);

        var secondAdd = await client.PostAsJsonAsync($"/api/boards/{boardId}/members", new
        {
            userId = memberId
        });

        Assert.Equal(HttpStatusCode.Conflict, secondAdd.StatusCode);
    }

    [Fact]
    public async Task GetLists_ForUnknownBoard_ShouldReturnNotFound()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/boards/{Guid.NewGuid()}/lists");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MoveCard_ToMissingList_ShouldReturnNotFound()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var ownerId = await CreateUserAsync(client, "Owner Three", "owner3.integration@example.com");

        var boardResponse = await client.PostAsJsonAsync("/api/boards", new
        {
            name = "Move Board",
            description = "Move cards",
            ownerId
        });

        var boardId = await ReadIdAsync(boardResponse);

        var listResponse = await client.PostAsJsonAsync($"/api/boards/{boardId}/lists", new
        {
            name = "Doing"
        });

        var listId = await ReadIdAsync(listResponse);

        var cardResponse = await client.PostAsJsonAsync($"/api/lists/{listId}/cards", new
        {
            title = "Card to move",
            description = "Move me"
        });

        var cardId = await ReadIdAsync(cardResponse);

        var moveResponse = await client.PatchAsJsonAsync($"/api/cards/{cardId}/move", new
        {
            targetListId = Guid.NewGuid(),
            targetPosition = 1
        });

        Assert.Equal(HttpStatusCode.NotFound, moveResponse.StatusCode);
    }

    [Fact]
    public async Task AssignCard_ToMissingUser_ShouldReturnNotFound()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var ownerId = await CreateUserAsync(client, "Owner Four", "owner4.integration@example.com");

        var boardResponse = await client.PostAsJsonAsync("/api/boards", new
        {
            name = "Assign Board",
            description = "Card assignment",
            ownerId
        });

        var boardId = await ReadIdAsync(boardResponse);

        var listResponse = await client.PostAsJsonAsync($"/api/boards/{boardId}/lists", new
        {
            name = "Done"
        });

        var listId = await ReadIdAsync(listResponse);

        var cardResponse = await client.PostAsJsonAsync($"/api/lists/{listId}/cards", new
        {
            title = "Card",
            description = "Assign me"
        });

        var cardId = await ReadIdAsync(cardResponse);

        var assignResponse = await client.PostAsync($"/api/cards/{cardId}/assignments/{Guid.NewGuid()}", content: null);

        Assert.Equal(HttpStatusCode.NotFound, assignResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteCard_ThenGetById_ShouldReturnNotFound()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var ownerId = await CreateUserAsync(client, "Owner Five", "owner5.integration@example.com");

        var boardResponse = await client.PostAsJsonAsync("/api/boards", new
        {
            name = "Delete Board",
            description = "Delete flow",
            ownerId
        });

        var boardId = await ReadIdAsync(boardResponse);

        var listResponse = await client.PostAsJsonAsync($"/api/boards/{boardId}/lists", new
        {
            name = "Archive"
        });

        var listId = await ReadIdAsync(listResponse);

        var cardResponse = await client.PostAsJsonAsync($"/api/lists/{listId}/cards", new
        {
            title = "Delete me",
            description = "Cleanup"
        });

        var cardId = await ReadIdAsync(cardResponse);

        var deleteResponse = await client.DeleteAsync($"/api/cards/{cardId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getDeletedResponse = await client.GetAsync($"/api/cards/{cardId}");
        Assert.Equal(HttpStatusCode.NotFound, getDeletedResponse.StatusCode);
    }

    private static async Task<Guid> CreateUserAsync(HttpClient client, string fullName, string email)
    {
        var response = await client.PostAsJsonAsync("/api/users", new
        {
            fullName,
            email
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await ReadIdAsync(response);
    }

    private static async Task<Guid> ReadIdAsync(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(payload.TryGetProperty("id", out var idProperty));
        return idProperty.GetGuid();
    }
}
