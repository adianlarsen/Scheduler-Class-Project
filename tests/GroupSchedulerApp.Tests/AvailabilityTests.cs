using GroupSchedulerApp.Controllers;
using GroupSchedulerApp.Models;

namespace GroupSchedulerApp.Tests;

public class AvailabilityTests
{
    private static DateTime At(int hour) => new DateTime(2026, 11, 9, hour, 0, 0);

    [Fact]
    public void FindsFreeTimeSharedByAllMembers()
    {
        var busy = new List<List<TimeBlock>>
        {
            new() { new TimeBlock { Start = At(9), End = At(11) } },
            new() { new TimeBlock { Start = At(10), End = At(12) } }
        };
        var range = new TimeBlock { Start = At(8), End = At(14) };

        var free = SchedulesController.FindCommonAvailability(busy, range);

        Assert.Equal(2, free.Count);
        Assert.Equal(At(8), free[0].Start);
        Assert.Equal(At(9), free[0].End);
        Assert.Equal(At(12), free[1].Start);
        Assert.Equal(At(14), free[1].End);
    }
}