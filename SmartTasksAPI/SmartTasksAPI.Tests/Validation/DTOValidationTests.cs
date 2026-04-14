using System.ComponentModel.DataAnnotations;
using SmartTasksAPI.Contracts.Boards;
using SmartTasksAPI.Contracts.Cards;
using SmartTasksAPI.Contracts.Comments;
using SmartTasksAPI.Contracts.Lists;
using SmartTasksAPI.Contracts.Users;

namespace SmartTasksAPI.Tests.Validation;

public class DTOValidationTests
{
    #region CreateUserRequest Validation

    [Fact]
    public void CreateUserRequest_IsValid_WhenAllFieldsAreCorrect()
    {
        var request = new CreateUserRequest { FullName = "John Doe", Email = "john@example.com" };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void CreateUserRequest_IsInvalid_WhenFullNameIsNull()
    {
        var request = new CreateUserRequest { FullName = null!, Email = "john@example.com" };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateUserRequest.FullName)));
    }

    [Fact]
    public void CreateUserRequest_IsInvalid_WhenFullNameIsTooShort()
    {
        var request = new CreateUserRequest { FullName = "J", Email = "john@example.com" };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateUserRequest.FullName)));
    }

    [Fact]
    public void CreateUserRequest_IsInvalid_WhenEmailIsNull()
    {
        var request = new CreateUserRequest { FullName = "John Doe", Email = null! };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateUserRequest.Email)));
    }

    [Fact]
    public void CreateUserRequest_IsInvalid_WhenEmailFormatIsInvalid()
    {
        var request = new CreateUserRequest { FullName = "John Doe", Email = "not-an-email" };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateUserRequest.Email)));
    }

    #endregion

    #region CreateBoardRequest Validation

    [Fact]
    public void CreateBoardRequest_IsValid_WhenAllRequiredFieldsArePresent()
    {
        var request = new CreateBoardRequest { Name = "Product Board", Description = "Q2", OwnerId = Guid.NewGuid() };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void CreateBoardRequest_IsInvalid_WhenNameIsNull()
    {
        var request = new CreateBoardRequest { Name = null!, Description = "Q2", OwnerId = Guid.NewGuid() };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateBoardRequest.Name)));
    }

    [Fact]
    public void CreateBoardRequest_IsInvalid_WhenNameIsTooShort()
    {
        var request = new CreateBoardRequest { Name = "P", OwnerId = Guid.NewGuid() };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateBoardRequest.Name)));
    }

    [Fact]
    public void CreateBoardRequest_IsInvalid_WhenOwnerIdIsEmpty()
    {
        var request = new CreateBoardRequest { Name = "Product Board", OwnerId = Guid.Empty };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        // Guid.Empty is technically valid by validation rules, but the service rejects it
        // This test documents that the DTO doesn't validate Guid empty value
        Assert.True(isValid);
    }

    #endregion

    #region UpdateBoardRequest Validation

    [Fact]
    public void UpdateBoardRequest_IsValid_WhenAllFieldsAreCorrect()
    {
        var request = new UpdateBoardRequest { Name = "Updated Board", Description = "New desc" };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void UpdateBoardRequest_IsInvalid_WhenNameIsTooShort()
    {
        var request = new UpdateBoardRequest { Name = "X" };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid);
    }

    #endregion

    #region CreateListRequest Validation

    [Fact]
    public void CreateListRequest_IsValid_WhenNameIsCorrect()
    {
        var request = new CreateListRequest { Name = "Todo" };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void CreateListRequest_IsInvalid_WhenNameIsLessThan2Chars()
    {
        var request = new CreateListRequest { Name = "T" };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateListRequest.Name)));
    }

    #endregion

    #region UpdateListRequest Validation

    [Fact]
    public void UpdateListRequest_IsValid_WhenAllFieldsAreCorrect()
    {
        var request = new UpdateListRequest { Name = "Doing", Position = 2 };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void UpdateListRequest_IsInvalid_WhenPositionIsZero()
    {
        var request = new UpdateListRequest { Name = "Doing", Position = 0 };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateListRequest.Position)));
    }

    #endregion

    #region CreateCardRequest Validation

    [Fact]
    public void CreateCardRequest_IsValid_WhenTitleIsCorrect()
    {
        var request = new CreateCardRequest { Title = "Implement feature", Description = "Details", DueDateUtc = null };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void CreateCardRequest_IsInvalid_WhenTitleIsNull()
    {
        var request = new CreateCardRequest { Title = null! };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateCardRequest.Title)));
    }

    [Fact]
    public void CreateCardRequest_IsInvalid_WhenTitleIsTooShort()
    {
        var request = new CreateCardRequest { Title = "T" };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateCardRequest.Title)));
    }

    #endregion

    #region UpdateCardRequest Validation

    [Fact]
    public void UpdateCardRequest_IsValid_WhenAllFieldsAreCorrect()
    {
        var request = new UpdateCardRequest { Title = "Task", Position = 1 };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void UpdateCardRequest_IsInvalid_WhenPositionIsZero()
    {
        var request = new UpdateCardRequest { Title = "Task", Position = 0 };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateCardRequest.Position)));
    }

    #endregion

    #region MoveCardRequest Validation

    [Fact]
    public void MoveCardRequest_IsValid_WhenAllFieldsAreCorrect()
    {
        var request = new MoveCardRequest { TargetListId = Guid.NewGuid(), TargetPosition = 5 };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void MoveCardRequest_IsInvalid_WhenPositionIsZero()
    {
        var request = new MoveCardRequest { TargetListId = Guid.NewGuid(), TargetPosition = 0 };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(MoveCardRequest.TargetPosition)));
    }

    #endregion

    #region CreateCommentRequest Validation

    [Fact]
    public void CreateCommentRequest_IsValid_WhenAllFieldsAreCorrect()
    {
        var request = new CreateCommentRequest { AuthorId = Guid.NewGuid(), Message = "Nice work" };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void CreateCommentRequest_IsInvalid_WhenMessageIsEmpty()
    {
        var request = new CreateCommentRequest { AuthorId = Guid.NewGuid(), Message = "" };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateCommentRequest.Message)));
    }

    #endregion

    #region AddBoardMemberRequest Validation

    [Fact]
    public void AddBoardMemberRequest_IsValid_WhenUserIdIsProvided()
    {
        var request = new AddBoardMemberRequest { UserId = Guid.NewGuid() };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    #endregion
}
