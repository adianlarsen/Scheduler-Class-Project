namespace GroupSchedulerApp.Models;

public class Meeting
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string Title { get; set; } = "";
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}
