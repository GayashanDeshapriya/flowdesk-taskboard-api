using FlowDesk.TaskBoard.Domain.Entities;
using FlowDesk.TaskBoard.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.TaskBoard.Infrastructure.Persistence
{
    public static class SeedData
    {
        private static readonly IPasswordHasher<User> PasswordHasher = new PasswordHasher<User>();

        public static async Task InitializeDatabaseAsync(TaskBoardDbContext context, bool forceReseed)
        {
            await context.Database.MigrateAsync();

            if (forceReseed)
            {
                // ONLY use in Development!
                Console.WriteLine("Force reseeding database...");
                
                try
                {
                    await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"TaskItems\" CASCADE");
                    await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"ProjectMembers\" CASCADE");
                    await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Projects\" CASCADE");
                    await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Users\" CASCADE");
                    
                    context.ChangeTracker.Clear();
                    
                    Console.WriteLine("✅ Truncation successful. Proceeding with seed...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during truncation: {ex.Message}");
                    throw;
                }
            }

            // Only seed if empty
            if (context.Users.Any() || context.Projects.Any() || context.TaskItems.Any())
            {
                if (!forceReseed)
                {
                    Console.WriteLine("Database already contains data. Skipping seed.");
                    return;
                }
            }

            try
            {
                Console.WriteLine("Seeding users...");
                var users = CreateUsers();
                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();
                Console.WriteLine($"{users.Count} users seeded.");

                Console.WriteLine("Seeding project...");
                var project = CreateProject();
                await context.Projects.AddAsync(project);
                await context.SaveChangesAsync();
                Console.WriteLine($"Project '{project.Name}' seeded.");

                Console.WriteLine("Seeding project members...");
                var projectMembers = CreateProjectMembers(project.Id, users);
                await context.ProjectMembers.AddRangeAsync(projectMembers);
                await context.SaveChangesAsync();
                Console.WriteLine($"{projectMembers.Count} project members seeded.");

                Console.WriteLine("Seeding tasks...");
                var tasks = CreateTasks(project.Id, users);
                await context.TaskItems.AddRangeAsync(tasks);
                await context.SaveChangesAsync();
                Console.WriteLine($"{tasks.Count} tasks seeded.");
                
                Console.WriteLine("Database seeding completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during seeding: {ex.Message}");
                throw;
            }
        }

        private static List<User> CreateUsers()
        {
            var userData = new[]
            {
                ("nimal.perera@flowdesk.io", "Nimal Perera", SystemRole.Admin),
                ("kasun.silva@flowdesk.io", "Kasun Silva", SystemRole.TeamLead),
                ("amali.fernando@flowdesk.io", "Amali Fernando", SystemRole.TeamMember),
                ("sachin.jayawardena@flowdesk.io", "Sachin Jayawardena", SystemRole.TeamMember),
                ("dilani.peris@flowdesk.io", "Dilani Peris", SystemRole.TeamMember),
                ("chamod.senanayake@flowdesk.io", "Chamod Senanayake", SystemRole.TeamMember),
                ("isuru.gunawardena@flowdesk.io", "Isuru Gunawardena", SystemRole.TeamMember),
                ("tharushi.kumari@flowdesk.io", "Tharushi Kumari", SystemRole.TeamLead),
                ("roshan.weerasinghe@flowdesk.io", "Roshan Weerasinghe", SystemRole.TeamMember),
                ("madushi.samarasinghe@flowdesk.io", "Madushi Samarasinghe", SystemRole.TeamMember)
            };

            var passwords = new[]
            {
                "P@ssw0rd#2026!", "Secure#1234!", "Amali@2026!", "Sachin#Dev01", "Dilani@456!",
                "Chamod#789!", "Isuru@Secure9", "Tharushi#2026!", "Roshan@Dev123", "Madushi#Pass1"
            };

            var users = new List<User>();
            for (int i = 0; i < userData.Length; i++)
            {
                var (email, fullName, role) = userData[i];
                var user = new User(fullName, email, role)
                {
                    Id = GetUserId(i)
                };
                user.PasswordHash = PasswordHasher.HashPassword(user, passwords[i]);
                users.Add(user);
            }

            return users;
        }

        private static Project CreateProject()
        {
            return new Project(
                "FlowDesk Task Management System",
                "Complete implementation of a task board API with full CRUD operations, role-based access control, and comprehensive task management features.")
            {
                Id = Guid.Parse("00b365b4-3e0a-4d83-a620-e179c98ce76c")
            };
        }

        private static List<ProjectMember> CreateProjectMembers(Guid projectId, List<User> users)
        {
            var projectMembers = new List<ProjectMember>();
            
            // Add all users as project members with their roles
            foreach (var user in users)
            {
                var roleInProject = user.Role switch
                {
                    SystemRole.Admin => "ProjectOwner",
                    SystemRole.TeamLead => "ProjectLead",
                    SystemRole.TeamMember => "Developer",
                    _ => throw new NotImplementedException()
                };

                projectMembers.Add(new ProjectMember
                {
                    ProjectId = projectId,
                    UserId = user.Id,
                    RoleInProject = roleInProject
                });
            }

            return projectMembers;
        }

        private static List<TaskItem> CreateTasks(Guid projectId, List<User> users)
        {
            var taskData = new[]
            {
                ("Set up project board structure", TaskPriority.Medium, "2026-04-02T10:00:00Z", 0, "Create initial columns: To Do, In Progress, Done."),
                ("Implement task creation endpoint", TaskPriority.Low, "2026-04-01T15:00:00Z", 1, "Develop POST API to create tasks with validation."),
                ("Add task status workflow", TaskPriority.Low, "2026-04-03T12:00:00Z", 2, "Define transitions: ToDo → InProgress → Done."),
                ("Create task update endpoint", TaskPriority.Medium, "2026-04-04T09:30:00Z", 3, "Allow updating title, description, priority, and due date."),
                ("Implement task filtering", TaskPriority.High, "2026-04-06T11:00:00Z", 4, "Filter tasks by status, priority, and assignee."),
                ("Add sorting functionality", TaskPriority.High, "2026-04-06T14:00:00Z", 5, "Sort tasks by due date and created date."),
                ("Implement pagination", TaskPriority.Medium, "2026-04-07T10:00:00Z", 6, "Add page number and page size support to task list."),
                ("Add soft delete (archiving)", TaskPriority.Low, "2026-04-05T16:00:00Z", 7, "Mark tasks as archived instead of deleting permanently."),
                ("Add validation rules", TaskPriority.Low, "2026-04-02T13:00:00Z", 8, "Ensure title is required and due date is not in the past."),
                ("Implement global exception handling", TaskPriority.Medium, "2026-04-07T15:30:00Z", 9, "Centralize error handling with consistent API responses.")
            };

            var tasks = new List<TaskItem>();
            var creatorId = users[0].Id; // Nimal (Admin)

            foreach (var (title, priority, dueDateStr, assigneeIndex, description) in taskData)
            {
                var task = new TaskItem(
                    projectId,
                    creatorId,
                    title,
                    priority,
                    DateTimeOffset.Parse(dueDateStr),
                    users[assigneeIndex].Id,
                    description)
                {
                    Id = Guid.NewGuid(),
                    CreatedAtUtc = DateTimeOffset.UtcNow.AddHours(-2)
                };
                tasks.Add(task);
            }

            return tasks;
        }

        private static Guid GetUserId(int index) => index switch
        {
            0 => Guid.Parse("d3d7b1f9-7aa8-4f17-bf53-95b0a8a9a001"),
            1 => Guid.Parse("49b42e23-8ef0-40da-b6a3-dd96642d4bc4"),
            2 => Guid.Parse("7c9d21e5-9c3a-4d7c-bb59-21a4f6f4f8e1"),
            3 => Guid.Parse("a1b3c45d-2f1e-4d23-bb3a-3f6b5c7d2a9e"),
            4 => Guid.Parse("b92e7c4a-1d5a-4c89-a67f-8e4b9d7e1234"),
            5 => Guid.Parse("c32f5d91-6e7a-4f92-9d3a-4b9e5c8f1e67"),
            6 => Guid.Parse("f532f2c2-1f3c-4bcb-9f81-51a4f1ca2202"),
            7 => Guid.Parse("e8b9f1a2-3c4d-5e6f-7a8b-9c0d1e2f3a4b"),
            8 => Guid.Parse("d0a1b2c3-4e5f-6a7b-8c9d-0e1f2a3b4c5d"),
            9 => Guid.Parse("c1b2a3d4-5f6e-7b8a-9c0d-1e2f3a4b5c6d"),
            _ => Guid.NewGuid()
        };
    }
}