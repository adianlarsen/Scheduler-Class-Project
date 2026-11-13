using GroupSchedulerApp.Data;
using GroupSchedulerApp.Models;

namespace GroupSchedulerApp.Repositories;

// Saves groups and maintains membership in both directions.
// Returned objects are snapshots; call a repository method to save changes.
public class GroupRepository
{
    private readonly JsonDataStore _store;

    public GroupRepository(JsonDataStore store) => _store = store;

    // Id == 0 creates a group. Otherwise, updates an existing group.
    // Save replaces the group's full member list.
    public Group Save(Group group)
    {
        return _store.Update(data =>
        {
            var memberIds = group.Members.Select(u => u.Id).ToHashSet();
            var members = data.Users
                .Where(u => memberIds.Contains(u.Id)).ToList();

            if (members.Count != memberIds.Count)
                throw new ArgumentException("One or more users do not exist.");

            Group saved;

            if (group.Id == 0)
            {
                saved = new Group { Id = data.NextGroupId++ };
                data.Groups.Add(saved);
            }
            else
            {
                saved = data.Groups.FirstOrDefault(g => g.Id == group.Id)
                    ?? throw new KeyNotFoundException("Group not found.");
            }

            saved.Name = group.Name;

            // Use users from this snapshot so the file preserves shared
            // objects rather than saving duplicate copies of each user.
            saved.Members = members;

            foreach (var user in data.Users)
            {
                foreach (var previous in user.Groups
                    .Where(g => g.Id == saved.Id).ToList())
                {
                    user.Groups.Remove(previous);
                }

                if (memberIds.Contains(user.Id))
                    user.Groups.Add(saved);
            }

            group.Id = saved.Id;
            return saved;
        });
    }

    public Group? FindById(int id) =>
        _store.Read(data => data.Groups.FirstOrDefault(g => g.Id == id));

    public List<Group> FindAll() =>
        _store.Read(data => data.Groups.ToList());

    public bool AddMember(int groupId, int userId)
    {
        return _store.Update(data =>
        {
            var group = data.Groups.FirstOrDefault(g => g.Id == groupId);
            var user = data.Users.FirstOrDefault(u => u.Id == userId);

            if (group == null || user == null ||
                group.Members.Any(u => u.Id == userId))
            {
                return false;
            }

            group.Members.Add(user);

            if (!user.Groups.Any(g => g.Id == groupId))
                user.Groups.Add(group);

            return true;
        });
    }

    public bool RemoveMember(int groupId, int userId)
    {
        return _store.Update(data =>
        {
            var group = data.Groups.FirstOrDefault(g => g.Id == groupId);
            var member = group?.Members.FirstOrDefault(u => u.Id == userId);

            if (group == null || member == null)
                return false;

            group.Members.Remove(member);

            foreach (var previous in member.Groups
                .Where(g => g.Id == groupId).ToList())
            {
                member.Groups.Remove(previous);
            }

            return true;
        });
    }
}