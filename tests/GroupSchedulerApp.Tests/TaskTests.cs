using GroupSchedulerApp.Models;

namespace GroupSchedulerApp.Tests;

public class TaskTests
{
    [Fact]
    public void NewTasksDefaultToToDo()
    {
        var task = new TaskItem { Name = "Write intro" };

        Assert.Equal(GroupSchedulerApp.Models.TaskStatus.ToDo, task.Status);
    }

    [Fact]
    public void NewTasksStartWithNoMembers()
    {
        var task = new TaskItem { Name = "Write intro" };

        Assert.Empty(task.Members);
    }

    [Fact]
    public void TaskKeepsTheValuesItWasGiven()
    {
        var task = new TaskItem
        {
            Name = "Build login page",
            Priority = "High",
            EstimatedHours = 3.5,
            GroupId = 1
        };

        Assert.Equal("Build login page", task.Name);
        Assert.Equal("High", task.Priority);
        Assert.Equal(3.5, task.EstimatedHours);
        Assert.Equal(1, task.GroupId);
    }
}