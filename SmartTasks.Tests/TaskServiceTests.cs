using SmartTasks.API.Services;
using Xunit;

namespace SmartTasks.Tests;

public class TaskServiceTests
{
    [Fact]
    public void CreateTask_ShouldCreateTask_WhenTitleIsValid()
    {
        var service = new TaskService();
        string title = "Test task";

        var task = service.CreateTask(title);

        Assert.NotNull(task);
        Assert.Equal(title, task.Title);
        Assert.False(task.IsCompleted);
    }

    [Fact]
    public void CreateTask_ShouldThrowException_WhenTitleIsEmpty()
    {
        var service = new TaskService();

        Assert.Throws<ArgumentException>(() => service.CreateTask(""));
    }
}