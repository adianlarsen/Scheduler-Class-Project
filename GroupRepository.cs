namespace GroupSchedulerApp.Models;

// A span of time (used for busy blocks and free blocks).
public class TimeBlock
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}
