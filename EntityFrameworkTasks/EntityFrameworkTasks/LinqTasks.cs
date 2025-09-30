using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityFrameworkTasks.Tasks
{
    public static class LinqTasks
    {
        // 1. Print info about each project
        public static IEnumerable<object> GetProjectsInfo(AppDbContext context)
        {
            return context.Projects
                .Select(p => new
                {
                    p.Name,
                    p.Description,
                    TasksCount = p.Tasks.Count,
                    MembersCount = p.Team.Users.Count
                })
                .ToList();
        }

        // 2. Find all tasks with more than 2 comments
        public static IEnumerable<Task> GetTasksWithMoreThanTwoComments(AppDbContext context)
        {
            return context.Tasks
                .Where(t => t.Comments.Count > 2)
                .ToList();
        }

        // 3. Find the user who created the most tasks with BUG tag
        public static object GetUserWithMostBugTasks(AppDbContext context)
        {
            return context.Users
                .Select(u => new
                {
                    u.Name,
                    BugTasksCount = u.CreatedTasks.Count(t => t.Tags.Any(tag => tag.Name == "BUG"))
                })
                .OrderByDescending(u => u.BugTasksCount)
                .FirstOrDefault();
        }

        // 4. Count the number of tasks for each tag
        public static IEnumerable<object> GetTasksCountPerTag(AppDbContext context)
        {
            return context.Tags
                .Select(t => new
                {
                    Tag = t.Name,
                    TasksCount = t.Tasks.Count
                })
                .ToList();
        }

        // 5. Find all tasks where creator and assignee are the same person
        public static IEnumerable<Task> GetTasksWithSameCreatorAndAssignee(AppDbContext context)
        {
            return context.Tasks
                .Where(t => t.CreatorId == t.AssigneeId)
                .ToList();
        }

        // 6. Get the latest comment for each task (first 15 tasks with comments)
        public static IEnumerable<object> GetLatestComments(AppDbContext context)
        {
            return context.Tasks
                .Where(t => t.Comments.Any())
                .Select(t => new
                {
                    Task = t.Title,
                    LatestComment = t.Comments
                        .OrderByDescending(c => c.CreatedAt)
                        .FirstOrDefault()
                })
                .Take(15)
                .ToList();
        }

        // 7. Find tasks that have more than one tag
        public static IEnumerable<Task> GetTasksWithMultipleTags(AppDbContext context)
        {
            return context.Tasks
                .Where(t => t.Tags.Count > 1)
                .ToList();
        }

        // 8. Calculate total number of comments per user (descending)
        public static IEnumerable<object> GetCommentsCountPerUser(AppDbContext context)
        {
            return context.Users
                .Select(u => new
                {
                    u.Name,
                    CommentsCount = u.Comments.Count
                })
                .OrderByDescending(u => u.CommentsCount)
                .ToList();
        }

        // 9. Rank teams by number of created and assigned tasks
        public static IEnumerable<object> RankTeamsByTasks(AppDbContext context)
        {
            return context.Teams
                .Select(t => new
                {
                    Team = t.Name,
                    CreatedTasks = t.Users.Sum(u => u.CreatedTasks.Count),
                    AssignedTasks = t.Users.Sum(u => u.AssignedTasks.Count)
                })
                .OrderByDescending(t => t.CreatedTasks + t.AssignedTasks)
                .ToList();
        }

        // 10. Get info about users who commented under STORY tasks
        public static IEnumerable<object> GetUsersCommentedOnStoryTasks(AppDbContext context)
        {
            return context.Comments
                .Where(c => c.Task.Tags.Any(tag => tag.Name == "STORY"))
                .Select(c => new
                {
                    UserName = c.User.Name,
                    UserEmail = c.User.Email,
                    TaskTitle = c.Task.Title,
                    ProjectName = c.Task.Project.Name,
                    TeamName = c.Task.Project.Team.Name
                })
                .ToList();
        }

        // 11. For each user, find the most frequently used tag in created tasks
        public static IEnumerable<object> GetMostUsedTagPerUser(AppDbContext context)
        {
            return context.Users
                .Select(u => new
                {
                    User = u.Name,
                    MostUsedTag = u.CreatedTasks
                        .SelectMany(t => t.Tags)
                        .GroupBy(tag => tag.Name)
                        .OrderByDescending(g => g.Count())
                        .Select(g => g.Key)
                        .FirstOrDefault()
                })
                .ToList();
        }

        // 12. List projects ordered by avg number of comments per task
        public static IEnumerable<object> GetProjectsOrderedByAverageComments(AppDbContext context)
        {
            return context.Projects
                .Select(p => new
                {
                    Project = p.Name,
                    AvgComments = p.Tasks.Any() 
                        ? p.Tasks.Average(t => t.Comments.Count) 
                        : 0
                })
                .OrderByDescending(p => p.AvgComments)
                .ToList();
        }

        // 13. Find users who commented on tasks they did not create
        public static IEnumerable<object> GetUsersCommentedOnForeignTasks(AppDbContext context)
        {
            return context.Comments
                .Where(c => c.Task.CreatorId != c.UserId)
                .Select(c => new
                {
                    UserName = c.User.Name,
                    TaskTitle = c.Task.Title
                })
                .Distinct()
                .ToList();
        }
    }
}
