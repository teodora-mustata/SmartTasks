using System.ComponentModel.DataAnnotations;
using SmartTasksAPI.Contracts.Boards;
using SmartTasksAPI.Contracts.Cards;
using SmartTasksAPI.Contracts.Comments;
using SmartTasksAPI.Contracts.Lists;
using SmartTasksAPI.Contracts.Users;

namespace SmartTasksAPI.Tests.Validation;

/// <summary>
/// Tests to verify that request DTOs with invalid data are properly rejected by validation rules.
/// These tests ensure that ASP.NET model validation will correctly return BadRequest (400) for invalid payloads.
/// </summary>
public class ControllerInvalidPayloadTests
{
    private static bool IsModelValid<T>(T model) where T : class
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        return Validator.TryValidateObject(model, context, results, true);
    }

    #region UsersController Invalid Payloads

    [Fact]
    public void Create_User_WithNullFullName_ShouldBeInvalid()
    {
        var request = new CreateUserRequest { FullName = null!, Email = "test@example.com" };

        var isValid = IsModelValid(request);

        Assert.False(isValid);
    }

    [Fact]
    public void Create_User_WithInvalidEmail_ShouldBeInvalid()
    {
        var request = new CreateUserRequest { FullName = "John Doe", Email = "not-an-email" };

        var isValid = IsModelValid(request);

        Assert.False(isValid);
    }

    #endregion

    #region BoardsController Invalid Payloads

    [Fact]
    public void Create_Board_WithNameTooShort_ShouldBeInvalid()
    {
        var request = new CreateBoardRequest { Name = "X", OwnerId = Guid.NewGuid() };

        var isValid = IsModelValid(request);

        Assert.False(isValid);
    }

    [Fact]
    public void Create_Board_WithValidData_ShouldBeValid()
    {
        var request = new CreateBoardRequest { Name = "Product Board", OwnerId = Guid.NewGuid() };

        var isValid = IsModelValid(request);

        Assert.True(isValid);
    }

    [Fact]
    public void Update_Board_WithEmptyName_ShouldBeInvalid()
    {
        var request = new UpdateBoardRequest { Name = "", Description = "test" };

        var isValid = IsModelValid(request);

        Assert.False(isValid);
    }

    #endregion

    #region ListsController Invalid Payloads

    [Fact]
    public void Create_List_WithNameTooShort_ShouldBeInvalid()
    {
        var request = new CreateListRequest { Name = "T" };

        var isValid = IsModelValid(request);

        Assert.False(isValid);
    }

    [Fact]
    public void Create_List_WithValidName_ShouldBeValid()
    {
        var request = new CreateListRequest { Name = "Todo" };

        var isValid = IsModelValid(request);

        Assert.True(isValid);
    }

    [Fact]
    public void Update_List_WithZeroPosition_ShouldBeInvalid()
    {
        var request = new UpdateListRequest { Name = "Done", Position = 0 };

        var isValid = IsModelValid(request);

        Assert.False(isValid);
    }

    [Fact]
    public void Update_List_WithValidData_ShouldBeValid()
    {
        var request = new UpdateListRequest { Name = "Doing", Position = 2 };

        var isValid = IsModelValid(request);

        Assert.True(isValid);
    }

    #endregion

    #region CardsController Invalid Payloads

    [Fact]
    public void Create_Card_WithNullTitle_ShouldBeInvalid()
    {
        var request = new CreateCardRequest { Title = null! };

        var isValid = IsModelValid(request);

        Assert.False(isValid);
    }

    [Fact]
    public void Create_Card_WithValidTitle_ShouldBeValid()
    {
        var request = new CreateCardRequest { Title = "Implement feature", Description = "Details" };

        var isValid = IsModelValid(request);

        Assert.True(isValid);
    }

    [Fact]
    public void Update_Card_WithZeroPosition_ShouldBeInvalid()
    {
        var request = new UpdateCardRequest { Title = "Task", Position = 0 };

        var isValid = IsModelValid(request);

        Assert.False(isValid);
    }

    [Fact]
    public void Update_Card_WithValidData_ShouldBeValid()
    {
        var request = new UpdateCardRequest { Title = "Task", Position = 1 };

        var isValid = IsModelValid(request);

        Assert.True(isValid);
    }

    [Fact]
    public void Move_Card_WithZeroTargetPosition_ShouldBeInvalid()
    {
        var request = new MoveCardRequest { TargetListId = Guid.NewGuid(), TargetPosition = 0 };

        var isValid = IsModelValid(request);

        Assert.False(isValid);
    }

    [Fact]
    public void Move_Card_WithValidData_ShouldBeValid()
    {
        var request = new MoveCardRequest { TargetListId = Guid.NewGuid(), TargetPosition = 5 };

        var isValid = IsModelValid(request);

        Assert.True(isValid);
    }

    #endregion

    #region CommentsController Invalid Payloads

    [Fact]
    public void Create_Comment_WithEmptyMessage_ShouldBeInvalid()
    {
        var request = new CreateCommentRequest { AuthorId = Guid.NewGuid(), Message = "" };

        var isValid = IsModelValid(request);

        Assert.False(isValid);
    }

    [Fact]
    public void Create_Comment_WithValidData_ShouldBeValid()
    {
        var request = new CreateCommentRequest { AuthorId = Guid.NewGuid(), Message = "Nice work" };

        var isValid = IsModelValid(request);

        Assert.True(isValid);
    }

    #endregion
}
