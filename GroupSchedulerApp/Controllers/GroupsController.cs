using GroupSchedulerApp.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GroupSchedulerApp.Controllers;

// Layer 2: group use cases (create group, join group, add/remove members).
[ApiController]
[Route("api/groups")]
public class GroupsController : ControllerBase
{
    private readonly GroupRepository _groups;
    public GroupsController(GroupRepository groups) => _groups = groups;

    [HttpPost]
    public IActionResult CreateGroup() => StatusCode(501, "Not implemented");

    [HttpPost("{groupId}/join")]
    public IActionResult JoinGroup(int groupId) => StatusCode(501, "Not implemented");

    [HttpPost("{groupId}/members")]
    public IActionResult AddMember(int groupId) => StatusCode(501, "Not implemented");

    [HttpDelete("{groupId}/members/{userId}")]
    public IActionResult RemoveMember(int groupId, int userId) => StatusCode(501, "Not implemented");
}