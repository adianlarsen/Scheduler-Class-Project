using GroupSchedulerApp.Models;
using GroupSchedulerApp.Repositories;
using Microsoft.AspNetCore.Mvc;
using GroupSchedulerApp.Importers;

namespace GroupSchedulerApp.Controllers;

public record ImportScheduleRequest(int UserId, List<TimeBlock>? BusyBlocks, string? IcsContent);

public record CreateMeetingRequest(int GroupId, string Title, DateTime Start, DateTime End);

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

    // Send either busyBlocks (a list of start/end times) or icsContent (the text of a .ics file).
    [HttpPost("import")]
    public IActionResult ImportSchedule(ImportScheduleRequest request)
    {
        List<TimeBlock> blocks;

        if (!string.IsNullOrWhiteSpace(request.IcsContent))
        {
            try
            {
                blocks = ScheduleImporter.ParseSchedule(request.IcsContent);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        else if (request.BusyBlocks != null)
        {
            blocks = request.BusyBlocks;
        }
        else
        {
            return BadRequest("Send busy blocks or calendar (.ics) text.");
        }

        try
        {
            var saved = _schedules.SaveForUser(request.UserId, blocks);
            return Ok(saved);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPost("meetings")]
    public IActionResult AddMeeting(CreateMeetingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Meeting title is required.");

        try
        {
            var saved = _meetings.SaveMeeting(new Meeting
            {
                GroupId = request.GroupId,
                Title = request.Title.Trim(),
                Start = request.Start,
                End = request.End
            });

            return StatusCode(201, saved);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Example: GET /api/schedules/group/1/availability?start=2026-12-01T08:00:00&end=2026-12-01T18:00:00&minMinutes=30
    [HttpGet("group/{groupId}/availability")]
    public IActionResult GetCommonAvailability(
        int groupId,
        [FromQuery] DateTime start,
        [FromQuery] DateTime end,
        [FromQuery] int minMinutes = 0)
    {
        if (start == default || end == default)
            return BadRequest("The start and end query parameters are required.");
        if (end <= start)
            return BadRequest("The end of the range must be after the start.");

        // Null means the group does not exist.
        var schedules = _schedules.FindByGroup(groupId);
        if (schedules == null)
            return NotFound("Group not found.");

        var busyByMember = schedules
            .Select(s => (IEnumerable<TimeBlock>)s.BusyBlocks)
            .ToList();

        // Meetings already on the calendar take up everyone's time.
        var meetingBlocks = _meetings.FindByGroup(groupId)
            .Select(m => new TimeBlock { Start = m.Start, End = m.End })
            .ToList();
        busyByMember.Add(meetingBlocks);

        var free = FindCommonAvailability(busyByMember, new TimeBlock { Start = start, End = end })
            .Where(b => (b.End - b.Start).TotalMinutes >= minMinutes)
            .ToList();

        return Ok(new
        {
            groupId,
            start,
            end,
            schedulesCounted = schedules.Count,
            freeBlocks = free
        });
    }

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