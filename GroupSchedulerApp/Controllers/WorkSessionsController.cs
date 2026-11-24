using GroupSchedulerApp.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GroupSchedulerApp.Controllers;

// Layer 2: work sessions (task, time, members).
[ApiController]
[Route("api/work-sessions")]
public class WorkSessionsController : ControllerBase
{
    private readonly MeetingRepository _meetings;
    public WorkSessionsController(MeetingRepository meetings) => _meetings = meetings;

    [HttpPost]
    public IActionResult ScheduleWorkSession() => StatusCode(501, "Not implemented");
}
