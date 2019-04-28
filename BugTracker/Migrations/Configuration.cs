namespace BugTracker.Migrations
{
    using BugTracker.Models;
    using BugTracker.Models.Domain;
    using BugTracker.MyHelpers;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BugTracker.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "BugTracker.Models.ApplicationDbContext";
        }

        protected override void Seed(BugTracker.Models.ApplicationDbContext context)
        {
            if (System.Diagnostics.Debugger.IsAttached == false)
            {
                //! Uncomment line below for debugging this Seed() method
                //System.Diagnostics.Debugger.Launch();
            }

            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.

            //! Order matters

            CreateDefaultRoles(context);

            List<ApplicationUser> initialUsers = PopulateUsersAndRolesAndSave(context);
            PopulateDefaultProjectsAndSave(context, initialUsers);


            CreateTicketPropsAndSave(context);

            CreateTestTicket(context);

            context.SaveChanges();
        }
        private void CreateTestTicket(ApplicationDbContext context)
        {
            if (!context.Tickets.Any())
            {
                TicketPriorities priority = context.TicketPriorities.First(p => p.PriorityString == nameof(TicketPrioritiesEnum.Medium));
                TicketStatuses status = context.TicketStatuses.First(p => p.StatusString == nameof(TicketStatusesEnum.Open));
                TicketTypes type = context.TicketTypes.First(p => p.TypeString == nameof(TicketTypesEnum.Feature));

                //! Ran into weird bugs when using ".First()", ".Last()", and ".ElementAt()"
                ApplicationUser author = context.Users.First(user => user.Email.ToLower() == "submitter@mybugtracker.com");
                ApplicationUser assignedUser = context.Users.First(user => user.Email.ToLower() == "developer@mybugtracker.com");

                Project project = author.Projects.FirstOrDefault() ?? context.Projects.First();

                TicketAttachment attachment = new TicketAttachment()
                {
                    Description = "testing attachment description",
                    FilePath = "this is a file path",
                    FileUrl = "this is a file url",
                    UserId = author.Id,
                };

                TicketComment comment = new TicketComment()
                {
                    Comment = "this is a test comment",
                    UserId = assignedUser.Id,
                };

                Ticket testTicket = new Ticket()
                {
                    AssignedUser = assignedUser,
                    AssignedUserId = assignedUser.Id,
                    Author = author,
                    AuthorId = author.Id,
                    Title = "Test Ticket",
                    Description = "This is a test ticket",

                    PriorityId = priority.Id,
                    Priority = priority,

                    StatusId = status.Id,
                    Status = status,

                    TypeId = type.Id,
                    Type = type,

                    ProjectId = project.Id,
                    Project = project,

                    Attachments = new List<TicketAttachment>() { attachment },
                    Comments = new List<TicketComment>() { comment },
                };

                context.Tickets.AddOrUpdate(t => t.Title, testTicket);
            }
        }
        private void CreateTicketPropsAndSave(ApplicationDbContext context)
        {
            List<int> ticketPrioritiesList = Enum.GetValues(typeof(TicketPrioritiesEnum)).Cast<int>().ToList();
            if (context.TicketPriorities.Count() != ticketPrioritiesList.Count)
            {
                foreach (int ticketPrioritiesId in ticketPrioritiesList)
                {
                    bool isSuccessful = CONSTANTS.TicketPriorites.TryGetValue((TicketPrioritiesEnum)ticketPrioritiesId, out TicketPriorities currentPriority);

                    if (!isSuccessful || currentPriority == null)
                    {
                        throw new Exception("something bad happened");
                    }

                    context.TicketPriorities.AddOrUpdate(p => p.PriorityString, currentPriority);
                }
            }
            context.SaveChanges();


            List<int> ticketTypesList = Enum.GetValues(typeof(TicketTypesEnum)).Cast<int>().ToList();
            if (context.TicketTypes.Count() != ticketTypesList.Count)
            {
                foreach (int ticketTypeId in ticketTypesList)
                {
                    bool isSuccessful = CONSTANTS.TicketTypes.TryGetValue((TicketTypesEnum)ticketTypeId, out TicketTypes currentType);

                    if (!isSuccessful || currentType == null)
                    {
                        throw new Exception("something bad happened");
                    }

                    context.TicketTypes.AddOrUpdate(p => p.TypeString, currentType);
                }
            }
            context.SaveChanges();


            List<int> ticketStatusesList = Enum.GetValues(typeof(TicketStatusesEnum)).Cast<int>().ToList();
            if (context.TicketStatuses.Count() != ticketStatusesList.Count)
            {
                foreach (int ticketStatusId in ticketStatusesList)
                {
                    bool isSuccessful = CONSTANTS.TicketStatuses.TryGetValue((TicketStatusesEnum)ticketStatusId, out TicketStatuses currentStatus);

                    if (!isSuccessful || currentStatus == null)
                    {
                        throw new Exception("something bad happened");
                    }

                    context.TicketStatuses.AddOrUpdate(p => p.StatusString, currentStatus);
                }
            }
            context.SaveChanges();
        }

        private void CreateDefaultRoles(ApplicationDbContext context)
        {
            CreateRole(context, nameof(UserRolesEnum.Admin));
            CreateRole(context, nameof(UserRolesEnum.ProjectManager));
            CreateRole(context, nameof(UserRolesEnum.Developer));
            CreateRole(context, nameof(UserRolesEnum.Submitter));
        }

        private void CreateRole(ApplicationDbContext context, string newRole)
        {
            if (!string.IsNullOrWhiteSpace(newRole) && !context.Roles.Any(role => role.Name == newRole))
            {
                IdentityRole newIdentityRole = new IdentityRole(newRole);
                new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context)).Create(newIdentityRole);
            }
        }

        private List<ApplicationUser> PopulateUsersAndRolesAndSave(BugTracker.Models.ApplicationDbContext context)
        {
            RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            List<ApplicationUser> initialUsers = new List<ApplicationUser>();

            #region Creating Initial Users
            ApplicationUser admin = CreateUser(
                context: context,
                roleManager: roleManager,
                userManager: userManager,
                displayName: "Admin (DisplayName)",
                userName: "admin@mybugtracker.com",
                userEmail: "admin@mybugtracker.com",
                userRole: nameof(UserRolesEnum.Admin));

            ApplicationUser demoAdmin = CreateUser(
                context: context,
                roleManager: roleManager,
                userManager: userManager,
                displayName: "Demo-Admin",
                userName: "demo-admin@mybugtracker.com",
                userEmail: "demo-admin@mybugtracker.com",
                userRole: nameof(UserRolesEnum.Admin));

            ApplicationUser projectManager = CreateUser(
                context: context,
                roleManager: roleManager,
                userManager: userManager,
                displayName: "Project-Manager (DisplayName)",
                userName: "projectManager@mybugtracker.com",
                userEmail: "projectManager@mybugtracker.com",
                userRole: nameof(UserRolesEnum.ProjectManager));
            
            ApplicationUser demoProjectManager = CreateUser(
                context: context,
                roleManager: roleManager,
                userManager: userManager,
                displayName: "Demo-Project-Manager",
                userName: "demo-projectManager@mybugtracker.com",
                userEmail: "demo-projectManager@mybugtracker.com",
                userRole: nameof(UserRolesEnum.ProjectManager));

            ApplicationUser developer = CreateUser(
                context: context,
                roleManager: roleManager,
                userManager: userManager,
                displayName: "Developer (DisplayName)",
                userName: "developer@mybugtracker.com",
                userEmail: "developer@mybugtracker.com",
                userRole: nameof(UserRolesEnum.Developer));
            
            ApplicationUser demoDeveloper = CreateUser(
                context: context,
                roleManager: roleManager,
                userManager: userManager,
                displayName: "Demo-Developer",
                userName: "demo-developer@mybugtracker.com",
                userEmail: "demo-developer@mybugtracker.com",
                userRole: nameof(UserRolesEnum.Developer));

            ApplicationUser submitter = CreateUser(
                context: context,
                roleManager: roleManager,
                userManager: userManager,
                displayName: "Submitter (DisplayName)",
                userName: "submitter@mybugtracker.com",
                userEmail: "submitter@mybugtracker.com",
                userRole: nameof(UserRolesEnum.Submitter));

            ApplicationUser demoSubmitter = CreateUser(
                context: context,
                roleManager: roleManager,
                userManager: userManager,
                displayName: "Demo-Submitter",
                userName: "demo-submitter@mybugtracker.com",
                userEmail: "demo-submitter@mybugtracker.com",
                userRole: nameof(UserRolesEnum.Submitter));

            ApplicationUser everettGrassler = CreateUser(
                context: context,
                roleManager: roleManager,
                userManager: userManager,
                displayName: "Everett Grassler (DisplayName)",
                userName: "everettG@mybugtracker.com",
                userEmail: "everettG@mybugtracker.com",
                userPassword: "123Everett",
                userRole: nameof(UserRolesEnum.Admin));
            #endregion

            initialUsers.Add(admin);
            initialUsers.Add(projectManager);
            initialUsers.Add(developer);
            initialUsers.Add(submitter);
            initialUsers.Add(demoAdmin);
            initialUsers.Add(demoProjectManager);
            initialUsers.Add(demoDeveloper);
            initialUsers.Add(demoSubmitter);
            initialUsers.Add(everettGrassler);


            // Save changes made above to the database
            context.SaveChanges();

            return initialUsers;
        }

        private ApplicationUser CreateUser(
            BugTracker.Models.ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            string displayName,
            string userName,
            string userEmail,
            string userPassword = "Password-1",
            string userRole = null
            )
        {
            /// <summary>
            ///    Adding new role if it doesn't exist. 
            /// </summary>
            if (!string.IsNullOrEmpty(userRole) && !string.IsNullOrWhiteSpace(userRole) && !context.Roles.Any(role => role.Name == userRole))
            {
                IdentityRole newRole = new IdentityRole(userRole);
                roleManager.Create(newRole);
            }

            // Creating the newUser
            ApplicationUser newUser;


            /// <summary>
            ///     new User will be made if userName doesn't exist on the DB
            ///     and if no role was passed in the user won't be added to a role
            /// </summary>
            if (!context.Users.Any(user => user.UserName == userName))
            {
                newUser = new ApplicationUser()
                {
                    DisplayName = displayName,
                    UserName = userName,
                    Email = userEmail
                };

                newUser.EmailConfirmed = true;
                userManager.Create(newUser, userPassword);
            }
            else
            {
                /// <summary>
                ///     I'm using ".First()" and not ".FirstOrDefault()" because
                ///     the if statement above this will generate the user if
                ///     the user doesn't already exist in the database
                ///     (I'm 100% expecting this user to be in the database)
                /// </summary>
                newUser = context.Users.First(user => user.UserName == userName);
            }

            context.Users.AddOrUpdate(user => user.Email, newUser);
            context.SaveChanges();
            
            // Make sure the user is on the passed in role
            if (!string.IsNullOrEmpty(userRole) && !string.IsNullOrWhiteSpace(userRole) && !userManager.IsInRole(newUser.Id, userRole))
            {
                userManager.AddToRole(newUser.Id, userRole);
            }

            return newUser;
        }


        private void PopulateDefaultProjectsAndSave(BugTracker.Models.ApplicationDbContext context, List<ApplicationUser> initialUsers)
        {
            // make one default post for all initial users
            foreach (ApplicationUser user in initialUsers)
            {
                //List<string> userRoleNames = (from role in context.Roles
                //                              join r in user.Roles
                //                              on role.Id equals r.RoleId
                //                              select role.Name).ToList();
                //
                //List<string> userRoleNames = context
                //    .Roles.Join( // inner list/table
                //    user.Roles, // outer list/table
                //    role => role.Id, // (inner) property to compare
                //    userRole => userRole.RoleId, // (outer) property to compare
                //    (r, ur) => r.Name) // (select) this is basically a ".Select()" statement
                //    .ToList();

                List<string> userRoleNames = context.Roles
                    .ToList()
                    .Join(user.Roles, role => role.Id, userRole => userRole.RoleId, (r, ur) => r.Name)
                    .ToList();

                if ((userRoleNames.Contains(nameof(UserRolesEnum.Admin)) || userRoleNames.Contains(nameof(UserRolesEnum.ProjectManager))) && !user.Projects.Any())
                {
                    string name = $"{user.UserName.Replace("@mybugtracker.com", "")}'s Project";

                    Project newProject = new Project()
                    {
                        Name = name,
                        Users = new List<ApplicationUser>() { user },
                    };

                    user.Projects.Add(newProject);

                    // Add new post to database if the name of the post doesn't match any in the database
                    context.Projects.AddOrUpdate(post => post.Name, newProject);
                }
            }

            // Save changes made above to the database
            context.SaveChanges();
        }
    }
}
