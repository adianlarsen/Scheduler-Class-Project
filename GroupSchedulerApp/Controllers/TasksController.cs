using GroupSchedulerApp.Models;
using GroupSchedulerApp.Repositories;
using Microsoft.AspNetCore.Mvc;
using TaskState = GroupSchedulerApp.Models.TaskStatus;

namespace GroupSchedulerApp.Controllers;

public record CreateTaskRequest(
    int GroupId,
    string Name,
    string? Description,
    DateTime? Date,
    string? Priority,
    double EstimatedHours,
    int? MeetingId,
    List<int>? MemberIds);

public record UpdateStatusRequest(string Status);

// Layer 2: tasks and the task board (to do, in progress, review, done).
[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private static readonly string[] Priorities = { "Low", "Medium", "High" };

    private readonly TaskRepository _tasks;
    private readonly GroupRepository _groups;

    public TasksController(TaskRepository tasks, GroupRepository groups)
    {
        _tasks = tasks;
        _groups = groups;
    }

    [HttpPost]
    public IActionResult AddTask(CreateTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Task name is required.");

        var priority = "Medium";
        if (!string.IsNullOrWhiteSpace(request.Priority))
        {
            var match = Priorities.FirstOrDefault(p =>
                string.Equals(p, request.Priority.Trim(), StringComparison.OrdinalIgnoreCase));

            if (match == null)
                return BadRequest("Priority must be Low, Medium, or High.");

            priority = match;
        }

        var memberIds = (request.MemberIds ?? new List<int>()).Distinct().ToList();

        // Assigned members must already belong to the task's group.
        var group = _groups.FindById(request.GroupId);
        if (group != null)
        {
            var groupMemberIds = group.Members.Select(m => m.Id).ToHashSet();
            if (memberIds.Any(id => !groupMemberIds.Contains(id)))
                return BadRequest("Task members must belong to the group.");
        }

        var task = new TaskItem
        {
            GroupId = request.GroupId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? "",
            Date = request.Date,
            Priority = priority,
            EstimatedHours = request.EstimatedHours,
            MeetingId = request.MeetingId,
            Members = memberIds.Select(id => new User { Id = id }).ToList()
        };

        try
        {
            var saved = _tasks.Save(task);
            return StatusCode(201, saved);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("group/{groupId}")]
    public IActionResult GetTaskBoard(int groupId)
    {
        if (_groups.FindById(groupId) == null)
            return NotFound("Group not found.");

        var tasks = _tasks.FindByGroup(groupId);

        return Ok(new
        {
            groupId,
            toDo = tasks.Where(t => t.Status == TaskState.ToDo).ToList(),
            inProgress = tasks.Where(t => t.Status == TaskState.InProgress).ToList(),
            review = tasks.Where(t => t.Status == TaskState.Review).ToList(),
            done = tasks.Where(t => t.Status == TaskState.Done).ToList()
        });
    }

    // Body example: {"status":"InProgress"}
    // A task can move one column forward or one column back.
    [HttpPatch("{taskId}/status")]
    public IActionResult UpdateStatus(int taskId, UpdateStatusRequest request)
    {
        if (!TryParseStatus(request.Status, out var newStatus))
            return BadRequest("Status must be ToDo, InProgress, Review, or Done.");

        var task = _tasks.FindById(taskId);
        if (task == null)
            return NotFound("Task not found.");

        var step = (int)newStatus - (int)task.Status;
        if (step != 1 && step != -1)
            return BadRequest(
                $"A task can only move one column at a time. It is currently {task.Status}.");

        var updated = _tasks.UpdateStatus(taskId, newStatus);
        return updated == null ? NotFound("Task not found.") : Ok(updated);
    }

    // Accepts "InProgress", "In Progress", "in-progress", and similar spellings.
    private static bool TryParseStatus(string? text, out TaskState status)
    {
        status = TaskState.ToDo;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        var cleaned = new string(text.Where(char.IsLetter).ToArray());

        return Enum.TryParse(cleaned, ignoreCase: true, out status)
            && Enum.IsDefined(status);
    }
}