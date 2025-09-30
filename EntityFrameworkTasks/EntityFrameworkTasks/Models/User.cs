namespace EntityFrameworkTasks.Models;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public virtual ICollection<Task> CreatedTasks { get; set; } = new List<Task>();

    public virtual ICollection<Task> AssignedTasks { get; set; } = new List<Task>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
}