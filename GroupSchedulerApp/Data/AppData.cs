using GroupSchedulerApp.Models;

namespace GroupSchedulerApp.Data;

// Represents the entire contents of scheduler.json.
// These collections replace database tables for our JSON storage.
//
// Relationships use the existing model properties, such as Group.Members.
// JsonDataStore preserves shared and circular object references.
public class AppData
{
    public List<User> Users { get; set; } = new();
    public List<Group> Groups { get; set; } = new();
    public List<Schedule> Schedules { get; set; } = new();
    public List<Meeting> Meetings { get; set; } = new();
    public List<TaskItem> Tasks { get; set; } = new();
    public List<WorkSession> WorkSessions { get; set; } = new();

    // Repositories assign these IDs when creating records.
    // Increment the appropriate counter INSIDE JsonDataStore.Update.
    // Persisting counters prevents IDs from being reused after deletion.
    //
    // Example:
    // user.Id = data.NextUserId++;
    // data.Users.Add(user);
    public int NextUserId { get; set; } = 1;
    public int NextGroupId { get; set; } = 1;
    public int NextScheduleId { get; set; } = 1;
    public int NextMeetingId { get; set; } = 1;
    public int NextTaskId { get; set; } = 1;
    public int NextWorkSessionId { get; set; } = 1;
}