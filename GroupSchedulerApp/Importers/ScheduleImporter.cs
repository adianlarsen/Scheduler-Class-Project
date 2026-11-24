
using GroupSchedulerApp.Models;

namespace GroupSchedulerApp.Importers;

// Turns an imported schedule (for example an .ics calendar file) into busy blocks.
public static class ScheduleImporter
{
    public static List<TimeBlock> ParseSchedule(string fileContents)
    {
        // TODO: implement parsing
        return new List<TimeBlock>();
    }
}