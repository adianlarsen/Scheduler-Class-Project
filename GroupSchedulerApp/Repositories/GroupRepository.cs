using GroupSchedulerApp.Data;
using GroupSchedulerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GroupSchedulerApp.Repositories;

// Layer 3: saves and loads groups and group members.
public class GroupRepository
{
    private readonly AppDbContext _db;
    public GroupRepository(AppDbContext db) => _db = db;

    // Adds a new group, or updates it if it already exists.
    public Group Save(Group group)
    {
        if (group.Id == 0)
            _db.Groups.Add(group);
        else
            _db.Groups.Update(group);

        _db.SaveChanges();
        return group;
    }

    // Returns the group with its members, or null if it doesn't exist.
    public Group? FindById(int id)
    {
        return _db.Groups
            .Include(g => g.Members)
            .FirstOrDefault(g => g.Id == id);
    }

    public List<Group> FindAll()
    {
        return _db.Groups
            .Include(g => g.Members)
            .ToList();
    }

    // Returns false if the group or user doesn't exist, or the user is already a member.
    public bool AddMember(int groupId, int userId)
    {
        var group = FindById(groupId);
        var user = _db.Users.Find(userId);

        if (group == null || user == null) return false;
        if (group.Members.Any(m => m.Id == userId)) return false;

        group.Members.Add(user);
        _db.SaveChanges();
        return true;
    }

    // Returns false if the group doesn't exist or the user isn't a member.
    public bool RemoveMember(int groupId, int userId)
    {
        var group = FindById(groupId);
        var member = group?.Members.FirstOrDefault(m => m.Id == userId);

        if (group == null || member == null) return false;

        group.Members.Remove(member);
        _db.SaveChanges();
        return true;
    }
}
