namespace SmartTasks.API.Services;

public class TaskService
{
    public TaskItem CreateTask(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty");

        return new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            IsCompleted = false
        };
    }
}

public class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
}