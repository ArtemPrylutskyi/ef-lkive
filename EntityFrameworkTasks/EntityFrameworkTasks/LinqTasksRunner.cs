using System;
using System.Linq;

public class LinqTasksRunner
{
    private readonly AppDbContext context;

    public LinqTasksRunner(AppDbContext dbContext)
    {
        context = dbContext;
    }

    public void RunAllTasks()
    {
        // 1. Info about each project
        var projectsInfo = context.Projects
            .Select(p => new {
                p.Name,
                p.Description,
                TasksCount = p.Tasks.Count,
                MembersCount = p.Team.Users.Count
            });

        // 2. Tasks with more than 2 comments
        var tasksWithManyComments = context.Tasks
            .Where(t => t.Comments.Count > 2);

        // 3. User who created most BUG-tagged tasks
        var topBugCreator = context.Users
            .Select(u => new {
                User = u,
                BugTaskCount = u.CreatedTasks.Count(t => t.Tags.Any(tag => tag.Name == "BUG"))
            })
            .OrderByDescending(x => x.BugTaskCount)
            .FirstOrDefault();

        // 4. Number of tasks per tag
        var tagTaskCounts = context.Tags
            .Select(tag => new {
                tag.Name,
                TaskCount = tag.Tasks.Count
            });

        // 5. Tasks where creator = assignee
        var selfAssignedTasks = context.Tasks
            .Where(t => t.CreatorId == t.AssigneeId);

        // 6. Latest comment for each of first 15 tasks with comments
        var latestComments = context.Tasks
            .Where(t => t.Comments.Any())
            .OrderBy(t => t.Id)
            .Take(15)
            .Select(t => new {
                TaskTitle = t.Title,
                LatestComment = t.Comments
                    .OrderByDescending(c => c.CreatedAt)
                    .FirstOrDefault()
            });

        // 7. Tasks with more than one tag
        var multiTagTasks = context.Tasks
            .Where(t => t.Tags.Count > 1);

        // 8. Total comments per user (desc)
        var userCommentCounts = context.Users
            .Select(u => new {
                u.Name,
                CommentCount = u.Comments.Count
            })
            .OrderByDescending(x => x.CommentCount);

        // 9. Rank teams by created + assigned tasks
        var teamRanks = context.Teams
            .Select(team => new {
                team.Name,
                CreatedTasks = team.Users.SelectMany(u => u.CreatedTasks).Count(),
                AssignedTasks = team.Users.SelectMany(u => u.AssignedTasks).Count()
            })
            .OrderByDescending(x => x.CreatedTasks + x.AssignedTasks);

        // 10. Users who commented on tasks with tag STORY
        var storyCommenters = context.Comments
            .Where(c => c.Task.Tags.Any(tag => tag.Name == "STORY"))
            .Select(c => new {
                UserName = c.User.Name,
                UserEmail = c.User.Email,
                TaskTitle = c.Task.Title,
                ProjectName = c.Task.Project.Name,
                TeamName = c.Task.Project.Team.Name
            });

        // 11. Most used tag per user (on created tasks)
        var userTopTags = context.Users
            .Select(u => new {
                u.Name,
                TopTag = u.CreatedTasks
                    .SelectMany(t => t.Tags)
                    .GroupBy(tag => tag.Name)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefault()
            });

        // 12. Projects ordered by avg comments per task
        var projectCommentAverages = context.Projects
            .Select(p => new {
                p.Name,
                AvgComments = p.Tasks.Any()
                    ? p.Tasks.Average(t => t.Comments.Count)
                    : 0
            })
            .OrderByDescending(x => x.AvgComments);

        // 13. Users who commented on tasks they didn't create
        var externalCommenters = context.Comments
            .Where(c => c.Task.CreatorId != c.UserId)
            .Select(c => c.User)
            .Distinct();

        Console.WriteLine("Усі запити виконано успішно.");
    }
}
