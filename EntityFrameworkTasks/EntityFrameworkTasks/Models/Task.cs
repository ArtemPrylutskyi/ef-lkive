namespace EntityFrameworkTasks.Models;

public class Task
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public int ProjectId { get; set; }

    public virtual Project Project { get; set; }

    public int CreatorId { get; set; }

    public virtual User Creator { get; set; }

    public int? AssigneeId { get; set; }

    public virtual User? Assignee { get; set; }

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}