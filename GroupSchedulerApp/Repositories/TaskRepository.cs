using GroupSchedulerApp.Data;
using GroupSchedulerApp.Models;

namespace GroupSchedulerApp.Repositories;

// Saves tasks and maintains their assigned-user relationships.
public class TaskRepository
{
    private readonly JsonDataStore _store;

    public TaskRepository(JsonDataStore store) => _store = store;

    // Id == 0 creates a task. Otherwise, updates an existing task.
    // Save replaces all editable fields, including the member list.
    public TaskItem Save(TaskItem task)
    {
        if (task.EstimatedHours < 0)
            throw new ArgumentException("Estimated hours cannot be negative.");

        return _store.Update(data =>
        {
            if (!data.Groups.Any(g => g.Id == task.GroupId))
                throw new ArgumentException("The group does not exist.");

            if (task.MeetingId.HasValue &&
                !data.Meetings.Any(m =>
                    m.Id == task.MeetingId.Value &&
                    m.GroupId == task.GroupId))
            {
                throw new ArgumentException(
                    "The meeting must exist and belong to the task's group.");
            }

            var memberIds = task.Members.Select(u => u.Id).ToHashSet();
            var members = data.Users
                .Where(u => memberIds.Contains(u.Id)).ToList();

            if (members.Count != memberIds.Count)
                throw new ArgumentException("One or more users do not exist.");

            TaskItem saved;

            if (task.Id == 0)
            {
                saved = new TaskItem { Id = data.NextTaskId++ };
                data.Tasks.Add(saved);
            }
            else
            {
                saved = data.Tasks.FirstOrDefault(t => t.Id == task.Id)
                    ?? throw new KeyNotFoundException("Task not found.");
            }

            saved.GroupId = task.GroupId;
            saved.Name = task.Name;
            saved.Description = task.Description;
            saved.Date = task.Date;
            saved.Priority = task.Priority;
            saved.Status = task.Status;
            saved.EstimatedHours = task.EstimatedHours;
            saved.MeetingId = task.MeetingId;
            saved.Members = members;

            // Update existing objects in place and synchronize both sides
            // of the task/user relationship before saving the snapshot.
            foreach (var user in data.Users)
            {
                foreach (var previous in user.Tasks
                    .Where(t => t.Id == saved.Id).ToList())
                {
                    user.Tasks.Remove(previous);
                }

                if (memberIds.Contains(user.Id))
                    user.Tasks.Add(saved);
            }

            task.Id = saved.Id;
            return saved;
        });
    }

    // Controller usage for PATCH /api/tasks/{taskId}/status:
    // var updatedTask = _tasks.UpdateStatus(taskId, newStatus);
    // if (updatedTask == null) return NotFound();
    // return Ok(updatedTask);
    //
    // newStatus uses the TaskStatus enum from our Models folder.
    // The controller should return BadRequest for an invalid status.
    public TaskItem? UpdateStatus(int taskId, GroupSchedulerApp.Models.TaskStatus newStatus)
    {
        if (!Enum.IsDefined(newStatus))
            throw new ArgumentOutOfRangeException(nameof(newStatus), "Unknown task status.");

        return _store.Update<TaskItem?>(data =>
        {
            var task = data.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
                return null;

            // Change the stored task inside the locked update so a status change
            // preserves any edits another request just made to its other fields.
            task.Status = newStatus;
            return task;
        });
    }
    
    public TaskItem? FindById(int id) =>
        _store.Read(data => data.Tasks.FirstOrDefault(t => t.Id == id));

    public List<TaskItem> FindByGroup(int groupId) =>
        _store.Read(data => data.Tasks
            .Where(t => t.GroupId == groupId)
            .OrderBy(t => t.Date)
            .ToList());

    public List<TaskItem> FindAll() =>
        _store.Read(data => data.Tasks.ToList());
}