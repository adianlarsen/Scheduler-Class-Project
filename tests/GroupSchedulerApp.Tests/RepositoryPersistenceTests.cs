using GroupSchedulerApp.Data;
using GroupSchedulerApp.Models;
using GroupSchedulerApp.Repositories;
using Xunit;
using SchedulerTaskStatus = GroupSchedulerApp.Models.TaskStatus;

namespace GroupSchedulerApp.Tests;

public class RepositoryPersistenceTests : IDisposable
{
    // xUnit creates a new test instance for each test, so each gets its own file.
    private readonly string _directory = Path.Combine(
        Path.GetTempPath(), "GroupSchedulerTests", Guid.NewGuid().ToString("N"));

    private JsonDataStore OpenStore() =>
        new JsonDataStore(Path.Combine(_directory, "scheduler.json"));

    private static User CreateUser(JsonDataStore store, string name)
    {
        // Set up a saved user for the schedule and task tests.
        return store.Update(data =>
        {
            var user = new User
            {
                Id = data.NextUserId++,
                Name = name,
                Email = $"{name}@example.com"
            };

            data.Users.Add(user);
            return user;
        });
    }

    private static TimeBlock Busy(int startHour, int endHour) => new TimeBlock
    {
        Start = new DateTime(2026, 11, 23, startHour, 0, 0),
        End = new DateTime(2026, 11, 23, endHour, 0, 0)
    };

    [Fact]
    public void SaveForUser_ReplacesBusyBlocksAndKeepsScheduleId()
    {
        var store = OpenStore();
        var user = CreateUser(store, "Adian");
        var schedules = new ScheduleRepository(store);

        var first = schedules.SaveForUser(user.Id, new() { Busy(9, 10) });
        var replacement = Busy(13, 15);
        schedules.SaveForUser(user.Id, new() { replacement });

        // Reopen the file to check what was actually saved.
        var reopened = new ScheduleRepository(OpenStore());
        var saved = Assert.Single(reopened.FindAll());

        Assert.Equal(first.Id, saved.Id);
        Assert.Equal(user.Id, saved.UserId);
        var block = Assert.Single(saved.BusyBlocks);
        Assert.Equal(replacement.Start, block.Start);
        Assert.Equal(replacement.End, block.End);
    }

    [Fact]
    public void FindByGroup_ReturnsOnlySavedSchedulesForGroupMembers()
    {
        var store = OpenStore();
        var member = CreateUser(store, "Adian");
        var memberWithoutSchedule = CreateUser(store, "Jace");
        var outsider = CreateUser(store, "Ben");

        var group = new Group { Name = "Study Group" };
        group.Members.Add(member);
        group.Members.Add(memberWithoutSchedule);
        group = new GroupRepository(store).Save(group);

        var schedules = new ScheduleRepository(store);
        schedules.SaveForUser(member.Id, new() { Busy(9, 10) });
        schedules.SaveForUser(outsider.Id, new() { Busy(11, 12) });

        var reopened = new ScheduleRepository(OpenStore());
        var results = reopened.FindByGroup(group.Id);

        Assert.NotNull(results);
        Assert.Equal(member.Id, Assert.Single(results!).UserId);
        Assert.Null(reopened.FindByGroup(-1));
    }

    [Fact]
    public void UpdateStatus_PersistsNewStatusAndPreservesTaskDetails()
    {
        var store = OpenStore();
        var user = CreateUser(store, "Adian");
        var group = new Group { Name = "Project Team" };
        group.Members.Add(user);
        group = new GroupRepository(store).Save(group);

        var tasks = new TaskRepository(store);
        var task = new TaskItem
        {
            GroupId = group.Id,
            Name = "Build schedule page",
            Description = "Display the group's available times.",
            EstimatedHours = 3,
            Status = SchedulerTaskStatus.ToDo
        };
        task.Members.Add(user);
        task = tasks.Save(task);

        tasks.UpdateStatus(task.Id, SchedulerTaskStatus.Done);

        var saved = new TaskRepository(OpenStore()).FindById(task.Id);
        Assert.NotNull(saved);
        Assert.Equal(SchedulerTaskStatus.Done, saved!.Status);
        Assert.Equal(task.Name, saved.Name);
        Assert.Equal(task.Description, saved.Description);
        Assert.Equal(task.EstimatedHours, saved.EstimatedHours);
        Assert.Equal(group.Id, saved.GroupId);
        Assert.Equal(user.Id, Assert.Single(saved.Members).Id);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, recursive: true);
    }
}