using GroupSchedulerApp.Importers;

namespace GroupSchedulerApp.Tests;

public class ScheduleImporterTests
{
    private const string Sample =
        "BEGIN:VCALENDAR\r\nVERSION:2.0\r\n" +
        "BEGIN:VEVENT\r\nDTSTART:20261201T090000\r\nDTEND:20261201T110000\r\nSUMMARY:Class\r\nEND:VEVENT\r\n" +
        "BEGIN:VEVENT\r\nDTSTART;VALUE=DATE:20261203\r\nDTEND;VALUE=DATE:20261204\r\nSUMMARY:Holiday\r\nEND:VEVENT\r\n" +
        "BEGIN:VEVENT\r\nDTSTART:20261202T090000\r\nDTEND:20261202T100000\r\nSTATUS:CANCELLED\r\nEND:VEVENT\r\n" +
        "END:VCALENDAR\r\n";

    [Fact]
    public void ParsesTimedAndAllDayEventsAndSkipsCancelled()
    {
        var blocks = ScheduleImporter.ParseSchedule(Sample);

        Assert.Equal(2, blocks.Count);
        Assert.Equal(new DateTime(2026, 12, 1, 9, 0, 0), blocks[0].Start);
        Assert.Equal(new DateTime(2026, 12, 1, 11, 0, 0), blocks[0].End);
        Assert.Equal(new DateTime(2026, 12, 3), blocks[1].Start);
        Assert.Equal(new DateTime(2026, 12, 4), blocks[1].End);
    }

    [Fact]
    public void RejectsAnEmptyFile()
    {
        Assert.Throws<ArgumentException>(() => ScheduleImporter.ParseSchedule(""));
    }

    [Fact]
    public void RejectsTextThatIsNotACalendar()
    {
        Assert.Throws<ArgumentException>(() => ScheduleImporter.ParseSchedule("hello"));
    }
}