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
                ApplicationUser author = context.Users.ToList()[context.Users.Count() - 1];
                ApplicationUser assignedUser = context.Users.ToList()[0];

                Project project = context.Projects.ToList()[0];

                TicketAttachments attachment = new TicketAttachments()
                {
                    Description = "testing attachment description",
                    FilePath = "this is a file path",
                    FileUrl = "this is a file url",
                    UserId = author.Id,
                };

                TicketComments comment = new TicketComments()
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

                    Attachments = new List<TicketAttachments>() { attachment },
                    Comments = new List<TicketComments>() { comment },
                };

                context.Tickets.AddOrUpdate(t => t.Title, testTicket);
            }
        }
        private void CreateTicketPropsAndSave(ApplicationDbContext context)
        {
            List<int> TicketPrioritiesList = Enum.GetValues(typeof(TicketPrioritiesEnum)).Cast<int>().ToList();
            if (context.TicketPriorities.Count() != TicketPrioritiesList.Count)
            {
                foreach (int TicketPrioritiesId in TicketPrioritiesList)
                {
                    bool isSuccessful = CONSTANTS.TicketPriorites.TryGetValue((TicketPrioritiesEnum)TicketPrioritiesId, out TicketPriorities currentPriority);

                    if (!isSuccessful || currentPriority == null)
                    {
                        throw new Exception("something bad happened");
                    }

                    context.TicketPriorities.AddOrUpdate(p => p.PriorityString, currentPriority);
                }
            }
            context.SaveChanges();


            List<int> TicketTypesList = Enum.GetValues(typeof(TicketTypesEnum)).Cast<int>().ToList();
            if (context.TicketTypes.Count() != TicketTypesList.Count)
            {
                foreach (int TicketTypeId in TicketTypesList)
                {
                    bool isSuccessful = CONSTANTS.TicketTypes.TryGetValue((TicketTypesEnum)TicketTypeId, out TicketTypes currentType);

                    if (!isSuccessful || currentType == null)
                    {
                        throw new Exception("something bad happened");
                    }

                    context.TicketTypes.AddOrUpdate(p => p.TypeString, currentType);
                }
            }
            context.SaveChanges();


            List<int> TicketStatusesList = Enum.GetValues(typeof(TicketStatusesEnum)).Cast<int>().ToList();
            if (context.TicketStatuses.Count() != TicketStatusesList.Count)
            {
                foreach (int TicketStatusId in TicketStatusesList)
                {
                    bool isSuccessful = CONSTANTS.TicketStatuses.TryGetValue((TicketStatusesEnum)TicketStatusId, out TicketStatuses currentStatus);

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
            ApplicationUser admin = CreateUser(context, roleManager, userManager, "Admin (DisplayName)", "admin@mybugtracker.com", "admin@mybugtracker.com", userRole: nameof(UserRolesEnum.Admin));

            ApplicationUser projectManager = CreateUser(context, roleManager, userManager, "Project Manager (DisplayName)", "projectManager@mybugtracker.com", "projectManager@mybugtracker.com", userRole: nameof(UserRolesEnum.ProjectManager));

            ApplicationUser developer = CreateUser(context, roleManager, userManager, "Developer (DisplayName)", "developer@mybugtracker.com", "developer@mybugtracker.com", userRole: nameof(UserRolesEnum.Developer));

            ApplicationUser submitter = CreateUser(context, roleManager, userManager, "Submitter (DisplayName)", "submitter@mybugtracker.com", "submitter@mybugtracker.com", userRole: nameof(UserRolesEnum.Submitter));

            ApplicationUser everettGrassler = CreateUser(context, roleManager, userManager, "Everett Grassler (DisplayName)", "everettG@mybugtracker.com", "everettG@mybugtracker.com", userPassword: "123Everett", userRole: nameof(UserRolesEnum.Admin));
            #endregion

            initialUsers.Add(admin);
            initialUsers.Add(projectManager); // TODO: Delete these after testing the role permissions
            initialUsers.Add(developer); // TODO: Delete these after testing the role permissions
            initialUsers.Add(submitter); // TODO: Delete these after testing the role permissions
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
