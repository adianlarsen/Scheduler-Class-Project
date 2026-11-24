using GroupSchedulerApp.Data;
using GroupSchedulerApp.Models;

namespace GroupSchedulerApp.Repositories;

// Saves meetings and work sessions using the shared JSON store.
public class MeetingRepository
{
    private readonly JsonDataStore _store;

    public MeetingRepository(JsonDataStore store) => _store = store;

    public Meeting SaveMeeting(Meeting meeting)
    {
        if (meeting.End <= meeting.Start)
            throw new ArgumentException("Meeting end must be after start.");

        return _store.Update(data =>
        {
            if (!data.Groups.Any(g => g.Id == meeting.GroupId))
                throw new ArgumentException("The group does not exist.");

            Meeting saved;

            if (meeting.Id == 0)
            {
                saved = new Meeting { Id = data.NextMeetingId++ };
                data.Meetings.Add(saved);
            }
            else
            {
                saved = data.Meetings.FirstOrDefault(m => m.Id == meeting.Id)
                    ?? throw new KeyNotFoundException("Meeting not found.");
            }

            saved.GroupId = meeting.GroupId;
            saved.Title = meeting.Title;
            saved.Start = meeting.Start;
            saved.End = meeting.End;

            meeting.Id = saved.Id;
            return saved;
        });
    }

    public List<Meeting> FindByGroup(int groupId) =>
        _store.Read(data => data.Meetings
            .Where(m => m.GroupId == groupId)
            .OrderBy(m => m.Start)
            .ToList());

    public Meeting? FindMeetingById(int id) =>
        _store.Read(data => data.Meetings.FirstOrDefault(m => m.Id == id));

    // Save replaces the full work-session member list.
    public WorkSession SaveWorkSession(WorkSession session)
    {
        if (session.End <= session.Start)
            throw new ArgumentException("Work session end must be after start.");

        return _store.Update(data =>
        {
            if (!data.Tasks.Any(t => t.Id == session.TaskItemId))
                throw new ArgumentException("The task does not exist.");

            var memberIds = session.Members.Select(u => u.Id).ToHashSet();
            var members = data.Users
                .Where(u => memberIds.Contains(u.Id)).ToList();

            if (members.Count != memberIds.Count)
                throw new ArgumentException("One or more users do not exist.");

            WorkSession saved;

            if (session.Id == 0)
            {
                saved = new WorkSession { Id = data.NextWorkSessionId++ };
                data.WorkSessions.Add(saved);
            }
            else
            {
                saved = data.WorkSessions
                    .FirstOrDefault(w => w.Id == session.Id)
                    ?? throw new KeyNotFoundException("Work session not found.");
            }

            saved.TaskItemId = session.TaskItemId;
            saved.Start = session.Start;
            saved.End = session.End;
            saved.Members = members;

            // Keep User.WorkSessions consistent with WorkSession.Members.
            foreach (var user in data.Users)
            {
                foreach (var previous in user.WorkSessions
                    .Where(w => w.Id == saved.Id).ToList())
                {
                    user.WorkSessions.Remove(previous);
                }

                if (memberIds.Contains(user.Id))
                    user.WorkSessions.Add(saved);
            }

            session.Id = saved.Id;
            return saved;
        });
    }

    public List<WorkSession> FindWorkSessionsByTask(int taskItemId) =>
        _store.Read(data => data.WorkSessions
            .Where(w => w.TaskItemId == taskItemId)
            .OrderBy(w => w.Start)
            .ToList());
}