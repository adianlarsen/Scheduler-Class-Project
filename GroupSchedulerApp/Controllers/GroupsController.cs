using GroupSchedulerApp.Models;
using GroupSchedulerApp.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GroupSchedulerApp.Controllers;

public record CreateGroupRequest(string Name, int CreatorUserId);
public record MemberRequest(int UserId);

[ApiController]
[Route("api/groups")]
public class GroupsController : ControllerBase
{
    private readonly GroupRepository _groups;
    public GroupsController(GroupRepository groups) => _groups = groups;

    [HttpGet("{groupId}")]
    public IActionResult GetGroup(int groupId)
    {
        var group = _groups.FindById(groupId);
        return group == null ? NotFound() : Ok(group);
    }

    [HttpPost]
    public IActionResult CreateGroup(CreateGroupRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Group name is required.");

        var group = new Group { Name = request.Name.Trim() };
        group.Members.Add(new User { Id = request.CreatorUserId });

        try
        {
            var saved = _groups.Save(group);
            return CreatedAtAction(nameof(GetGroup), new { groupId = saved.Id }, saved);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{groupId}/join")]
    public IActionResult JoinGroup(int groupId, MemberRequest request)
    {
        if (_groups.FindById(groupId) == null) return NotFound("Group not found.");

        return _groups.AddMember(groupId, request.UserId)
            ? Ok()
            : BadRequest("User not found or already a member.");
    }

    [HttpPost("{groupId}/members")]
    public IActionResult AddMember(int groupId, MemberRequest request) =>
        JoinGroup(groupId, request);

    [HttpDelete("{groupId}/members/{userId}")]
    public IActionResult RemoveMember(int groupId, int userId)
    {
        var group = _groups.FindById(groupId);
        if (group == null) return NotFound("Group not found.");
        if (group.Members.Count <= 1) return BadRequest("Cannot remove the last member.");

        return _groups.RemoveMember(groupId, userId)
            ? NoContent()
            : NotFound("That user is not in the group.");
    }
}