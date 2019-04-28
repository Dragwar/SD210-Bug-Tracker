using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using BugTracker.Models;

namespace BugTracker.MyHelpers.DB_Repositories
{
    [NotMapped]
    public class UserRepository
    {
        private ApplicationDbContext DBContext { get; }

        public UserRepository(ApplicationDbContext dBContext) => DBContext = dBContext ?? throw new ArgumentNullException(nameof(dBContext));

        public bool DoesUserExist(string id) => DBContext.Users.Any(user => user.Id == id);
        public ApplicationUser GetUserById(string id) => DBContext.Users.FirstOrDefault(user => user.Id == id);
        public ApplicationUser GetUserByEmail(string email) => DBContext.Users.FirstOrDefault(user => user.Email == email);
        public ApplicationUser GetUserByUserName(string userName) => DBContext.Users.FirstOrDefault(user => user.UserName == userName);
        public ApplicationUser GetUserByDisplayName(string displayName) => DBContext.Users.FirstOrDefault(user => user.UserName == displayName);

        /// <summary>Depends on "demo-" being in the demo users email</summary>
        public IQueryable<ApplicationUser> GetAllDemoUsers() => DBContext.Users.Where(user => user.Email.ToLower().Contains("demo-")).AsQueryable();

        /// <summary>Depends on "demo-" being in the demo users email and the "<paramref name="roleName" />" to be within the email</summary>
        public ApplicationUser GetDemoUser(string roleName) => GetAllDemoUsers().FirstOrDefault(user => user.Email.ToLower().Contains(roleName.ToLower()));

        /// <summary>Depends on "demo-" being in the demo users email and the "<paramref name="roleName" />" to be within the email</summary>
        public bool DoesDemoUserExist(string roleName) => GetAllDemoUsers().Any(user => user.Email.ToLower().Contains(roleName.ToLower()));

        /// <summary>Depends on "demo-" being in the demo users email and the "<paramref name="roleName" />" to be within the email</summary>
        public bool IsUserADemoUser(string id) => GetAllDemoUsers().Any(user => user.Id == id);

        public IQueryable<ApplicationUser> GetAllUsers() => DBContext.Users.AsQueryable();
    }
}