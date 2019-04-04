namespace BugTracker.Migrations
{
    using BugTracker.Models;
    using BugTracker.Models.Domain;
    using BugTracker.MyHelpers;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
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
            //if (System.Diagnostics.Debugger.IsAttached == false)
            //{
            //    System.Diagnostics.Debugger.Launch();
            //}
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.

            CreateDefaultRoles(context);

            List<ApplicationUser> initialUsers = PopulateUsersAndRolesAndSave(context);
            PopulateDefaultProjectsAndSave(context, initialUsers);

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
            for (int i = 0; i < initialUsers.Count; i++)
            {
                string name = $"{initialUsers[i].UserName.Replace("@mybugtracker.com", "")}'s Project";

                Project newProject = new Project()
                {
                    Name = name,
                    Users = new List<ApplicationUser>() { initialUsers[i] },
                };

                if ((i + 1) < initialUsers.Count)
                {
                    newProject.Users.Add(initialUsers[i + 1]);
                }
                else if ((i - 1) >= 0)
                {
                    newProject.Users.Add(initialUsers[i - 1]);
                }

                // Add new post to database if the name of the post doesn't match any in the database
                context.Projects.AddOrUpdate(post => post.Name, newProject);
            }

            // Save changes made above to the database
            context.SaveChanges();
        }
    }
}
