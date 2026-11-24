using GroupSchedulerApp.Data;
using GroupSchedulerApp.Models;

namespace GroupSchedulerApp.Repositories;

// Each user can have one schedule containing their dated busy blocks.
public class ScheduleRepository
{
    private readonly JsonDataStore _store;

    public ScheduleRepository(JsonDataStore store)
    {
        _store = store;
    }

    // Controller usage:
    // var saved = _schedules.Save(schedule);
    //
    // An ID of 0 creates a schedule. An existing ID updates that schedule.
    public Schedule Save(Schedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        return SaveSchedule(schedule, findByUser: false);
    }

    // Controller usage:
    // var saved = _schedules.SaveForUser(userId, busyBlocks);
    //
    // Send the user's full List<TimeBlock> with each call. The first call
    // creates the schedule. Later calls keep its ID and replace its blocks.
    // An empty list clears the blocks while keeping the schedule.
    public Schedule SaveForUser(int userId, List<TimeBlock> busyBlocks)
    {
        ArgumentNullException.ThrowIfNull(busyBlocks);

        var schedule = new Schedule
        {
            UserId = userId,
            BusyBlocks = busyBlocks
        };

        return SaveSchedule(schedule, findByUser: true);
    }

    private Schedule SaveSchedule(Schedule schedule, bool findByUser)
    {
        ArgumentNullException.ThrowIfNull(schedule.BusyBlocks);

        if (schedule.BusyBlocks.Any(block => block == null || block.End <= block.Start))
            throw new ArgumentException("Each busy block must end after it starts.");

        return _store.Update(data =>
        {
            if (!data.Users.Any(user => user.Id == schedule.UserId))
                throw new ArgumentException("The user does not exist.");

            Schedule? saved;

            // Keep the lookup and write inside the same locked update so two
            // first-time submissions cannot create two schedules for one user.
            if (findByUser)
            {
                saved = data.Schedules.FirstOrDefault(s => s.UserId == schedule.UserId);
            }
            else if (schedule.Id != 0)
            {
                saved = data.Schedules.FirstOrDefault(s => s.Id == schedule.Id)
                    ?? throw new KeyNotFoundException("Schedule not found.");

                if (saved.UserId != schedule.UserId)
                    throw new ArgumentException("Schedule owner cannot change.");
            }
            else
            {
                if (data.Schedules.Any(s => s.UserId == schedule.UserId))
                    throw new InvalidOperationException(
                        "This user already has a schedule. Update it by ID.");

                saved = null;
            }

            if (saved == null)
            {
                saved = new Schedule
                {
                    Id = data.NextScheduleId++,
                    UserId = schedule.UserId
                };

                data.Schedules.Add(saved);
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

    // Controller usage:
    // var schedule = _schedules.FindById(scheduleId);
    public Schedule? FindById(int id)
    {
        return _store.Read(data =>
            data.Schedules.FirstOrDefault(schedule => schedule.Id == id));
    }

    // Controller usage:
    // var schedule = _schedules.FindByUser(userId);
    // A null result means there is no saved schedule for this user ID.
    public Schedule? FindByUser(int userId)
    {
        return _store.Read(data =>
            data.Schedules.FirstOrDefault(schedule => schedule.UserId == userId));
    }

    // Controller usage:
    // var schedules = _schedules.FindByGroup(groupId);
    //
    // Returns null if the group doesn't exist. For an existing group, only
    // members who have saved a schedule appear in the list. A missing member's
    // availability is unknown. A saved schedule can have zero busy blocks.
    public List<Schedule>? FindByGroup(int groupId)
    {
        return _store.Read<List<Schedule>?>(data =>
        {
            var group = data.Groups.FirstOrDefault(g => g.Id == groupId);

            if (group == null)
                return null;

            // Read members and schedules from the same snapshot of the JSON file.
            var memberIds = group.Members.Select(member => member.Id).ToHashSet();

            return data.Schedules
                .Where(schedule => memberIds.Contains(schedule.UserId))
                .ToList();
        });
    }

    // Controller usage:
    // var schedules = _schedules.FindAll();
    public List<Schedule> FindAll()
    {
        return _store.Read(data => data.Schedules.ToList());
    }
}