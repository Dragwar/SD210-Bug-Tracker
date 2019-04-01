using BugTracker.Models;
using BugTracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BugTracker.MyHelpers.DB_Repositories
{
    public class UserRepository
    {
        private ApplicationDbContext DBContext { get; set; }

        public UserRepository(ApplicationDbContext dBContext)
        {
            DBContext = dBContext;
        }

        public ApplicationUser GetUserById(string id) => DBContext.Users.FirstOrDefault(user => user.Id == id);

        public ApplicationUser GetUserByEmail(string email) => DBContext.Users.FirstOrDefault(user => user.Email == email);

        public ApplicationUser GetUserByUserName(string userName) => DBContext.Users.FirstOrDefault(user => user.UserName == userName);

        public ApplicationUser GetUserByDisplayName(string displayName) => DBContext.Users.FirstOrDefault(user => user.UserName == displayName);

        public List<ApplicationUser> GetUsersByProject(Project projectToFind) => DBContext.Users
            .Where(user => user.Projects.Any(project => project == projectToFind))
            .ToList();

        public List<ApplicationUser> GetUsersByProject(Guid projectId) => DBContext.Users
            .Where(user => user.Projects.Any(project => project.Id == projectId))
            .ToList();

        public List<ApplicationUser> GetAllUsers() => DBContext.Users.ToList();
    }
}