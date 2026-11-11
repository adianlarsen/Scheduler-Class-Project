
namespace GroupSchedulerApp.Models;

public class WorkSession
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public ICollection<User> Members { get; set; } = new List<User>();
}
