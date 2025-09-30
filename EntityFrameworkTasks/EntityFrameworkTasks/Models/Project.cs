namespace EntityFrameworkTasks.Models;

public class Project
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    public virtual ICollection<User> Members { get; set; } = new List<User>();
}