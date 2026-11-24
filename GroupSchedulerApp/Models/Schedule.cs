
namespace GroupSchedulerApp.Models;

// One member's schedule, stored as the blocks of time they are busy.
public class Schedule
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public List<TimeBlock> BusyBlocks { get; set; } = new();
}
