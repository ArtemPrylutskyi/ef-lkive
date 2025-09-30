using EntityFrameworkTasks;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.Tasks;

Console.OutputEncoding = Encoding.UTF8;

await using (AppDbContext dbContext = new())
{
    
    await dbContext.Database.EnsureCreatedAsync(); 

    await RunTasksAsync(dbContext);
}

async Task RunTasksAsync(AppDbContext dbContext)
{
    Console.WriteLine("=========================================================================");
    Console.WriteLine("=== РОЗПОЧАТО ВИКОНАННЯ 13 ЗАВДАНЬ LINQ TO ENTITIES (Entity Framework) ===");
    Console.WriteLine("=========================================================================");
    
    
    // 1. Print info about each project (Name, Description, Tasks Count, Members Count)
    
    Console.WriteLine("\n--- 1. Інфо про проекти (Name, Tasks Count, Members Count) ---");

    var projectInfo = await dbContext.Projects
        .Select(p => new
        {
            p.Name,
            p.Description,
            TasksCount = p.Tasks.Count, 
            MembersCount = p.Members.Count 
        })
        .ToListAsync();

    foreach (var info in projectInfo)
    {
        Console.WriteLine($"Проект: {info.Name}");
        Console.WriteLine($"  Опис: {info.Description}");
        Console.WriteLine($"  Кількість завдань: {info.TasksCount}");
        Console.WriteLine($"  Кількість учасників: {info.MembersCount}");
        Console.WriteLine("-----------------------------");
    }
    
    
    // 2. Find all tasks with more than 2 comments
    Console.WriteLine("\n--- 2. Завдання з більш ніж 2 коментарями (Take 10) ---");

    var tasksWithManyComments = await dbContext.Tasks
        .Where(t => t.Comments.Count > 2)
        .Select(t => new { t.Id, t.Title, CommentsCount = t.Comments.Count })
        .OrderByDescending(x => x.CommentsCount)
        .Take(10) 
        .ToListAsync();

    foreach (var task in tasksWithManyComments)
    {
        Console.WriteLine($"ID: {task.Id}, Назва: {task.Title}, Коментарів: {task.CommentsCount}");
    }
    Console.WriteLine($"... Усього знайдено {await dbContext.Tasks.CountAsync(t => t.Comments.Count > 2)} завдань із > 2 коментарями.");
    
    
    // 3. Find the user who created the most tasks with `BUG` tag
    Console.WriteLine("\n--- 3. Користувач-лідер по BUG-завданням ---");

    var userWithMostBugs = await dbContext.Users
        .Select(u => new
        {
            u.Name,
            BugTaskCount = u.CreatedTasks
                .Count(t => t.Tags.Any(tag => tag.Name == "BUG"))
        })
        .OrderByDescending(x => x.BugTaskCount)
        .Take(1)
        .FirstOrDefaultAsync();

    if (userWithMostBugs != null && userWithMostBugs.BugTaskCount > 0)
    {
        Console.WriteLine($"Користувач: {userWithMostBugs.Name}");
        Console.WriteLine($"  Створено BUG-завдань: {userWithMostBugs.BugTaskCount}");
    }
    else
    {
        Console.WriteLine("Не знайдено користувачів, які створили завдання з тегом BUG.");
    }
    

    // 4. Count the number of tasks for each tag

    Console.WriteLine("\n--- 4. Кількість завдань для кожного тегу ---");

    var tasksPerTag = await dbContext.Tags
        .Select(tag => new
        {
            TagName = tag.Name,
            TaskCount = tag.Tasks.Count
        })
        .OrderByDescending(x => x.TaskCount)
        .ToListAsync();

    foreach (var tag in tasksPerTag)
    {
        Console.WriteLine($"Тег: {tag.TagName,-10}, Кількість завдань: {tag.TaskCount}");
    }
    
    
    // 5. Find all tasks where creator and assignee are the same person
    Console.WriteLine("\n--- 5. Завдання, де Creator == Assignee (Take 10) ---");

    var selfAssignedTasks = await dbContext.Tasks
        .Where(t => t.AssigneeId.HasValue && t.CreatorId == t.AssigneeId)
        .Select(t => new { t.Id, t.Title, Creator = t.Creator.Name })
        .Take(10) 
        .ToListAsync();

    foreach (var task in selfAssignedTasks)
    {
        Console.WriteLine($"ID: {task.Id}, Назва: {task.Title}, Виконавець/Створювач: {task.Creator}");
    }
    Console.WriteLine($"... Усього знайдено {await dbContext.Tasks.CountAsync(t => t.AssigneeId.HasValue && t.CreatorId == t.AssigneeId)} таких завдань.");
    
    
    // 6. Get the latest comment for each task (first 15 tasks with comments)
  
    Console.WriteLine("\n--- 6. Останній коментар для перших 15 завдань із коментарями ---");

    var latestComments = await dbContext.Tasks
        .Where(t => t.Comments.Any()) 
        .OrderBy(t => t.Id) 
        .Take(15)
        .Select(t => new
        {
            TaskTitle = t.Title,
            LatestComment = t.Comments
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new { c.Text, Author = c.Author.Name, c.CreatedAt })
                .FirstOrDefault()
        })
        .ToListAsync();

    foreach (var item in latestComments)
    {
        Console.WriteLine($"Завдання: {item.TaskTitle}");
        if (item.LatestComment != null)
        {
            Console.WriteLine($"  Останній коментар від {item.LatestComment.Author} ({item.LatestComment.CreatedAt:g}): {item.LatestComment.Text[..Math.Min(item.LatestComment.Text.Length, 50)]}...");
        }
        Console.WriteLine("---");
    }
    
   
    // 7. Find tasks that have more than one tag
    Console.WriteLine("\n--- 7. Завдання з більш ніж одним тегом (Take 10) ---");

    var tasksWithMultipleTags = await dbContext.Tasks
        .Where(t => t.Tags.Count > 1)
        .Select(t => new { t.Id, t.Title, TagsCount = t.Tags.Count, TagNames = t.Tags.Select(tag => tag.Name) })
        .Take(10)
        .ToListAsync();

    foreach (var task in tasksWithMultipleTags)
    {
        Console.WriteLine($"ID: {task.Id}, Назва: {task.Title}, Тегів: {task.TagsCount} ({string.Join(", ", task.TagNames)})");
    }
    Console.WriteLine($"... Усього знайдено {await dbContext.Tasks.CountAsync(t => t.Tags.Count > 1)} таких завдань.");
    
   
    // 8. Calculate the total number of comments per user in descending order
  
    Console.WriteLine("\n--- 8. Загальна кількість коментарів на користувача (Top 10) ---");

    var commentsPerUser = await dbContext.Comments
        .GroupBy(c => c.Author)
        .Select(g => new
        {
            UserName = g.Key.Name,
            TotalComments = g.Count()
        })
        .OrderByDescending(x => x.TotalComments)
        .Take(10) 
        .ToListAsync();

    foreach (var user in commentsPerUser)
    {
        Console.WriteLine($"Користувач: {user.UserName,-30}, Коментарів: {user.TotalComments}");
    }
    
   
    // 9. Rank teams by the number of created and assigned tasks
    Console.WriteLine("\n--- 9. Рейтинг команд за сумарною кількістю створених та призначених завдань (Top 10) ---");

    var teamRank = await dbContext.Teams
        .Select(t => new
        {
            t.Name,
            CreatedTasksCount = t.Members
                .SelectMany(u => u.CreatedTasks)
                .Count(),
            AssignedTasksCount = t.Members
                .SelectMany(u => u.AssignedTasks)
                .Count()
        })
        .OrderByDescending(x => x.CreatedTasksCount + x.AssignedTasksCount)
        .Take(10)
        .ToListAsync();

    foreach (var team in teamRank)
    {
        var total = team.CreatedTasksCount + team.AssignedTasksCount;
        Console.WriteLine($"Команда: {team.Name,-40}, Сумарно: {total} (Створено: {team.CreatedTasksCount}, Призначено: {team.AssignedTasksCount})");
    }
    
    
 
    // 10. Get information about users who left comments under a task with the tag `STORY`

    Console.WriteLine("\n--- 10. Інфо про користувачів, які коментували STORY-завдання (Take 10) ---");

    var storyCommentersInfo = await dbContext.Comments
        .Where(c => c.Task.Tags.Any(tag => tag.Name == "STORY"))
        .Select(c => new
        {
            UserName = c.Author.Name,
            c.Author.Email,
            TaskTitle = c.Task.Title,
            ProjectName = c.Task.Project.Name,
            TeamNames = c.Author.Teams.Select(t => t.Name).ToList()
        })
        .Distinct() 
        .Take(10)
        .ToListAsync();

    foreach (var info in storyCommentersInfo)
    {
        Console.WriteLine($"Користувач: {info.UserName} ({info.Email})");
        Console.WriteLine($"  Проект: {info.ProjectName}, Команди: {string.Join(", ", info.TeamNames)}");
        Console.WriteLine("---");
    }
    
    
    // 11. For each user, find the tag they use most frequently on tasks they've created (Top 10)
    Console.WriteLine("\n--- 11. Найпопулярніший тег, який використовує користувач для своїх завдань (Top 10) ---");

    var favoriteTags = await dbContext.Users
        .Where(u => u.CreatedTasks.Any()) 
        .Select(u => new
        {
            UserName = u.Name,
            TagUsage = u.CreatedTasks
                .SelectMany(t => t.Tags)
                .GroupBy(tag => tag.Name)
                .Select(g => new
                {
                    TagName = g.Key,
                    TagCount = g.Count()
                })
                .OrderByDescending(x => x.TagCount)
                .FirstOrDefault()
        })
        .Where(u => u.TagUsage != null)
        .OrderByDescending(u => u.TagUsage.TagCount)
        .Take(10) 
        .ToListAsync();

    foreach (var user in favoriteTags)
    {
        Console.WriteLine($"Користувач: {user.UserName,-30} | Тег: {user.TagUsage.TagName,-10} ({user.TagUsage.TagCount} разів)");
    }
    
    
  
    // 12. List projects ordered by the average number of comments per task
    Console.WriteLine("\n--- 12. Проекти за середньою кількістю коментарів на завдання ---");

    var projectsByAvgComments = await dbContext.Projects
        .Where(p => p.Tasks.Any()) 
        .Select(p => new
        {
            ProjectName = p.Name,
            TaskCount = p.Tasks.Count,
            AverageCommentsPerTask = p.Tasks.Average(t => t.Comments.Count)
        })
        .OrderByDescending(x => x.AverageCommentsPerTask)
        .ToListAsync();

    foreach (var project in projectsByAvgComments)
    {
        Console.WriteLine($"Проект: {project.ProjectName,-35} | Завдань: {project.TaskCount,-5} | Середнє коментарів/завдання: {project.AverageCommentsPerTask:F2}");
    }
    
    
    // 13. Find users who have commented on tasks they did not create
    Console.WriteLine("\n--- 13. Користувачі, які коментували ЧУЖІ завдання (Take 20) ---");

    var externalCommenters = await dbContext.Comments

        .Where(c => c.AuthorId != c.Task.CreatorId)
        .Select(c => c.Author.Name)
        .Distinct()
        .OrderBy(name => name)
        .Take(20) 
        .ToListAsync();

    Console.WriteLine($"Знайдено {await dbContext.Comments.Where(c => c.AuthorId != c.Task.CreatorId).Select(c => c.AuthorId).Distinct().CountAsync()} унікальних користувачів, які коментували чужі завдання (перші 20):");
    foreach (var name in externalCommenters)
    {
        Console.WriteLine($"- {name}");
    }
    
    
}
