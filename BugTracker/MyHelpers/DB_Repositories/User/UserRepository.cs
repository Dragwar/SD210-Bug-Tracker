using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace BugTracker.MyHelpers.DB_Repositories
{
    [NotMapped]
    public class UserRepository
    {
        private ApplicationDbContext DBContext { get; set; }

        public UserRepository(ApplicationDbContext dBContext)
        {
            DBContext = dBContext ?? throw new ArgumentNullException(nameof(dBContext));
        }

        public ApplicationUser GetUserById(string id) => DBContext.Users.FirstOrDefault(user => user.Id == id);
        public ApplicationUser GetUserByEmail(string email) => DBContext.Users.FirstOrDefault(user => user.Email == email);
        public ApplicationUser GetUserByUserName(string userName) => DBContext.Users.FirstOrDefault(user => user.UserName == userName);
        public ApplicationUser GetUserByDisplayName(string displayName) => DBContext.Users.FirstOrDefault(user => user.UserName == displayName);
        
        //! Flagged to be removed (similar method exists on the ProjectReposiory and this class should just worry about users and nothing else)
        //public List<ApplicationUser> GetUsersByProjectId(Guid projectId) => DBContext.Users
        //    .Where(user => user.Projects.Any(project => project.Id == projectId))
        //    .ToList();

        public IQueryable<ApplicationUser> GetAllUsers() => DBContext.Users.AsQueryable();
    }
}