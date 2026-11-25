using System.Globalization;
using GroupSchedulerApp.Models;

namespace GroupSchedulerApp.Importers;

// Turns calendar (.ics) text into busy blocks.
// Limits: repeating events (RRULE) count only their first occurrence, and an event with
// a time zone name is read as local time on the server.
public static class ScheduleImporter
{
    public static List<TimeBlock> ParseSchedule(string fileContents)
    {
        if (string.IsNullOrWhiteSpace(fileContents))
            throw new ArgumentException("The calendar file is empty.");

        var lines = Unfold(fileContents);

        if (!lines.Any(l => l.Equals("BEGIN:VCALENDAR", StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("This does not look like a calendar (.ics) file.");

        var blocks = new List<TimeBlock>();

        var inEvent = false;
        string? startValue = null, endValue = null;
        var startIsDate = false;
        var skip = false;

        foreach (var line in lines)
        {
            if (line.Equals("BEGIN:VEVENT", StringComparison.OrdinalIgnoreCase))
            {
                inEvent = true;
                startValue = null;
                endValue = null;
                startIsDate = false;
                skip = false;
                continue;
            }

            if (line.Equals("END:VEVENT", StringComparison.OrdinalIgnoreCase))
            {
                if (inEvent && !skip && startValue != null &&
                    TryParseDate(startValue, out var start))
                {
                    DateTime end;
                    if (endValue != null && TryParseDate(endValue, out var parsedEnd))
                        end = parsedEnd;
                    else if (startIsDate)
                        end = start.AddDays(1);
                    else
                        end = start;

                    // Skip events with no length.
                    if (end > start)
                        blocks.Add(new TimeBlock { Start = start, End = end });
                }

                inEvent = false;
                continue;
            }

            if (!inEvent)
                continue;

            var colon = line.IndexOf(':');
            if (colon <= 0)
                continue;

            var nameAndParams = line.Substring(0, colon);
            var value = line.Substring(colon + 1).Trim();
            var name = nameAndParams.Split(';')[0].ToUpperInvariant();

            switch (name)
            {
                case "DTSTART":
                    startValue = value;
                    startIsDate = value.Length == 8;
                    break;
                case "DTEND":
                    endValue = value;
                    break;
                case "STATUS" when value.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase):
                    skip = true;
                    break;
                case "TRANSP" when value.Equals("TRANSPARENT", StringComparison.OrdinalIgnoreCase):
                    skip = true; // Marked as free time.
                    break;
            }
        }

        return blocks.OrderBy(b => b.Start).ToList();
    }

    // Long lines in .ics files continue on the next line, which starts with a space or tab.
    private static List<string> Unfold(string text)
    {
        var result = new List<string>();

        foreach (var raw in text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
        {
            if ((raw.StartsWith(' ') || raw.StartsWith('\t')) && result.Count > 0)
                result[^1] += raw.Substring(1).TrimEnd();
            else
                result.Add(raw.TrimEnd());
        }

        return result;
    }

    private static bool TryParseDate(string value, out DateTime result)
    {
        var culture = CultureInfo.InvariantCulture;

        // All-day event: 20261201
        if (value.Length == 8)
            return DateTime.TryParseExact(value, "yyyyMMdd", culture, DateTimeStyles.None, out result);

        // UTC time: 20261201T090000Z, converted to the server's local time
        if (value.EndsWith("Z", StringComparison.OrdinalIgnoreCase))
        {
            if (DateTime.TryParseExact(value, "yyyyMMdd'T'HHmmss'Z'", culture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var utc))
            {
                result = DateTime.SpecifyKind(utc.ToLocalTime(), DateTimeKind.Unspecified);
                return true;
            }

            result = default;
            return false;
        }

        // Local time: 20261201T090000
        return DateTime.TryParseExact(value, "yyyyMMdd'T'HHmmss", culture, DateTimeStyles.None, out result);
    }
}