using GroupSchedulerApp.Data;
using GroupSchedulerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GroupSchedulerApp.Repositories;

// Layer 3: saves and loads meetings and work sessions.
public class MeetingRepository
{
    private readonly AppDbContext _db;
    public MeetingRepository(AppDbContext db) => _db = db;

    // Adds a new meeting, or updates it if it already exists.
    public Meeting SaveMeeting(Meeting meeting)
    {
        if (meeting.Id == 0)
            _db.Meetings.Add(meeting);
        else
            _db.Meetings.Update(meeting);

        _db.SaveChanges();
        return meeting;
    }

    // Returns all meetings for a group, earliest first.
    public List<Meeting> FindByGroup(int groupId)
    {
        return _db.Meetings
            .Where(m => m.GroupId == groupId)
            .OrderBy(m => m.Start)
            .ToList();
    }

    // Returns the meeting, or null if it doesn't exist.
    public Meeting? FindMeetingById(int id)
    {
        return _db.Meetings.Find(id);
    }

    // Adds a new work session, or updates it if it already exists.
    public WorkSession SaveWorkSession(WorkSession session)
    {
        if (session.Id == 0)
            _db.WorkSessions.Add(session);
        else
            _db.WorkSessions.Update(session);

        _db.SaveChanges();
        return session;
    }

    // Returns all work sessions for a task, with their members, earliest first.
    public List<WorkSession> FindWorkSessionsByTask(int taskItemId)
    {
        return _db.WorkSessions
            .Include(w => w.Members)
            .Where(w => w.TaskItemId == taskItemId)
            .OrderBy(w => w.Start)
            .ToList();
    }
}