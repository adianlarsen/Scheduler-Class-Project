using GroupSchedulerApp.Data;
using GroupSchedulerApp.Models;

namespace GroupSchedulerApp.Repositories;

// Stores users' busy blocks.
//
// Starter rule: one schedule per user. Each TimeBlock represents
// one actual time interval. Recurring class meetings must be expanded
// into dated blocks by the backend before saving.
public class ScheduleRepository
{
    private readonly JsonDataStore _store;

    public ScheduleRepository(JsonDataStore store) => _store = store;

    // Save replaces the full list of busy blocks.
    public Schedule Save(Schedule schedule)
    {
        if (schedule.BusyBlocks.Any(block => block.End <= block.Start))
            throw new ArgumentException("Busy block end must be after start.");

        return _store.Update(data =>
        {
            if (!data.Users.Any(u => u.Id == schedule.UserId))
                throw new ArgumentException("The user does not exist.");

            Schedule saved;

            if (schedule.Id == 0)
            {
                if (data.Schedules.Any(s => s.UserId == schedule.UserId))
                {
                    throw new InvalidOperationException(
                        "This user already has a schedule. Update it by ID.");
                }

                saved = new Schedule
                {
                    Id = data.NextScheduleId++,
                    UserId = schedule.UserId
                };

                data.Schedules.Add(saved);
            }
            else
            {
                saved = data.Schedules
                    .FirstOrDefault(s => s.Id == schedule.Id)
                    ?? throw new KeyNotFoundException("Schedule not found.");

                // A schedule cannot be reassigned to another user.
                if (saved.UserId != schedule.UserId)
                    throw new ArgumentException("Schedule owner cannot change.");
            }

            saved.BusyBlocks = schedule.BusyBlocks
                .OrderBy(block => block.Start)
                .Select(block => new TimeBlock
                {
                    Start = block.Start,
                    End = block.End
                })
                .ToList();

            schedule.Id = saved.Id;
            return saved;
        });
    }

    public Schedule? FindById(int id) =>
        _store.Read(data => data.Schedules.FirstOrDefault(s => s.Id == id));

    public Schedule? FindByUser(int userId) =>
        _store.Read(data => data.Schedules
            .FirstOrDefault(s => s.UserId == userId));

    public List<Schedule> FindAll() =>
        _store.Read(data => data.Schedules.ToList());
}