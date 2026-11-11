namespace GroupSchedulerApp.Models;

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    public ICollection<User> Members { get; set; } = new List<User>();
}
