using GroupSchedulerApp.Models;
using GroupSchedulerApp.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GroupSchedulerApp.Controllers;

public record CreateWorkSessionRequest(
    int TaskItemId,
    DateTime Start,
    DateTime End,
    List<int>? MemberIds);

// Layer 2: work sessions (task, time, members).
[ApiController]
[Route("api/work-sessions")]
public class WorkSessionsController : ControllerBase
{
    private readonly MeetingRepository _meetings;
    private readonly TaskRepository _tasks;
    private readonly ScheduleRepository _schedules;
    private readonly GroupRepository _groups;

    public WorkSessionsController(
        MeetingRepository meetings,
        TaskRepository tasks,
        ScheduleRepository schedules,
        GroupRepository groups)
    {
        _meetings = meetings;
        _tasks = tasks;
        _schedules = schedules;
        _groups = groups;
    }

    // Body example:
    // {"taskItemId":1,"start":"2026-12-01T13:00:00","end":"2026-12-01T14:00:00","memberIds":[1,2]}
    // If memberIds is left out, the task's assigned members are used.
    [HttpPost]
    public IActionResult ScheduleWorkSession(CreateWorkSessionRequest request)
    {
        if (request.End <= request.Start)
            return BadRequest("Work session end must be after start.");

        var task = _tasks.FindById(request.TaskItemId);
        if (task == null)
            return NotFound("Task not found.");

        var group = _groups.FindById(task.GroupId);
        if (group == null)
            return NotFound("The task's group was not found.");

        var requested = request.MemberIds ?? new List<int>();
        var memberIds = (requested.Count > 0
                ? requested
                : task.Members.Select(m => m.Id).ToList())
            .Distinct()
            .ToList();

        if (memberIds.Count == 0)
            return BadRequest("Add at least one member to the work session.");

        var groupMemberIds = group.Members.Select(m => m.Id).ToHashSet();
        if (memberIds.Any(id => !groupMemberIds.Contains(id)))
            return BadRequest("Work session members must belong to the group.");

        // The time must be free for every chosen member and not overlap a group meeting.
        var schedules = _schedules.FindByGroup(group.Id) ?? new List<Schedule>();

        var busyByMember = schedules
            .Where(s => memberIds.Contains(s.UserId))
            .Select(s => (IEnumerable<TimeBlock>)s.BusyBlocks)
            .ToList();

        busyByMember.Add(_meetings.FindByGroup(group.Id)
            .Select(m => new TimeBlock { Start = m.Start, End = m.End })
            .ToList());

        var range = new TimeBlock { Start = request.Start, End = request.End };
        var free = SchedulesController.FindCommonAvailability(busyByMember, range);

        var fits = free.Count == 1
            && free[0].Start == range.Start
            && free[0].End == range.End;

        if (!fits)
            return Conflict("The selected time is not free for every member.");

        try
        {
            var saved = _meetings.SaveWorkSession(new WorkSession
            {
                TaskItemId = task.Id,
                Start = request.Start,
                End = request.End,
                Members = memberIds.Select(id => new User { Id = id }).ToList()
            });

            return StatusCode(201, saved);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}