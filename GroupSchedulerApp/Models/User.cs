namespace GroupSchedulerApp.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";

    public ICollection<Group> Groups { get; set; } = new List<Group>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<WorkSession> WorkSessions { get; set; } = new List<WorkSession>();
}