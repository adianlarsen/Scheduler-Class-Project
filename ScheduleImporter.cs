using GroupSchedulerApp.Models;
using GroupSchedulerApp.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GroupSchedulerApp.Controllers;

// Layer 2: schedules, meetings, and common availability.
[ApiController]
[Route("api/schedules")]
public class SchedulesController : ControllerBase
{
    private readonly ScheduleRepository _schedules;
    private readonly MeetingRepository _meetings;

    public SchedulesController(ScheduleRepository schedules, MeetingRepository meetings)
    {
        _schedules = schedules;
        _meetings = meetings;
    }

    [HttpPost("import")]
    public IActionResult ImportSchedule() => StatusCode(501, "Not implemented");

    [HttpPost("meetings")]
    public IActionResult AddMeeting() => StatusCode(501, "Not implemented");

    [HttpGet("group/{groupId}/availability")]
    public IActionResult GetCommonAvailability(int groupId) => StatusCode(501, "Not implemented");

    // Core logic: given every member's busy blocks, return the free blocks shared by everyone.
    public static List<TimeBlock> FindCommonAvailability(
        IEnumerable<IEnumerable<TimeBlock>> busyByMember, TimeBlock range)
    {
        var allBusy = busyByMember
            .SelectMany(blocks => blocks)
            .OrderBy(b => b.Start)
            .ToList();

        var free = new List<TimeBlock>();
        var cursor = range.Start;

        foreach (var block in allBusy)
        {
            if (block.Start > cursor)
            {
                var end = block.Start < range.End ? block.Start : range.End;
                free.Add(new TimeBlock { Start = cursor, End = end });
            }
            if (block.End > cursor) cursor = block.End;
            if (cursor >= range.End) break;
        }

        if (cursor < range.End)
            free.Add(new TimeBlock { Start = cursor, End = range.End });

        return free;
    }
}
