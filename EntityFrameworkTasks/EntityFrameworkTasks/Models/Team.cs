namespace EntityFrameworkTasks.Models;

public class Team
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int ProjectId { get; set; }

    public virtual Project Project { get; set; }

    public virtual ICollection<User> Members { get; set; } = new List<User>();
}