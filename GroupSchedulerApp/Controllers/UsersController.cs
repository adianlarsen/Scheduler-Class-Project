using GroupSchedulerApp.Models;
using GroupSchedulerApp.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GroupSchedulerApp.Controllers;

public record CreateUserRequest(string Name, string Email);

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserRepository _users;
    public UsersController(UserRepository users) => _users = users;

    [HttpGet("{userId}")]
    public IActionResult GetUser(int userId)
    {
        var user = _users.FindById(userId);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public IActionResult CreateUser(CreateUserRequest request)
    {
        try
        {
            var saved = _users.Save(new User { Name = request.Name, Email = request.Email });
            return CreatedAtAction(nameof(GetUser), new { userId = saved.Id }, saved);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}