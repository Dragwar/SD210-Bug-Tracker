using BugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Linq;

namespace BugTracker.MyHelpers.DB_Repositories
{
    public class UserRoleRepository
    {
        private UserManager<ApplicationUser> UserManager;
        private ApplicationDbContext DbContext;

        public UserRoleRepository(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(dbContext));
        }

        public List<IdentityRole> GetAllUserRoles() => DbContext.Roles.ToList();

        public bool IsUserInRole(string userId, string roleName) => UserManager.IsInRole(userId, roleName);

        public List<string> ListUserRoles(string userId) => UserManager.GetRoles(userId).ToList();

        public bool AddUserToRole(string userId, string roleName)
        {
            IdentityResult result = UserManager.AddToRole(userId, roleName);
            return result.Succeeded;
        }

        public bool RemoveUserFromRole(string userId, string roleName)
        {
            IdentityResult result = UserManager.RemoveFromRole(userId, roleName);
            return result.Succeeded;
        }

        public List<ApplicationUser> UsersInRole(string roleName)
        {
            List<ApplicationUser> resultList = new List<ApplicationUser>();
            List<ApplicationUser> List = UserManager.Users.ToList();

            foreach (ApplicationUser user in List)
            {
                if (IsUserInRole(user.Id, roleName))
                {
                    resultList.Add(user);
                }
            }

            return resultList;
        }

        public List<ApplicationUser> UsersNotInRole(string roleName)
        {
            List<ApplicationUser> resultList = new List<ApplicationUser>();
            List<ApplicationUser> List = UserManager.Users.ToList();

            foreach (ApplicationUser user in List)
            {
                if (!IsUserInRole(user.Id, roleName))
                {
                    resultList.Add(user);
                }
            }

            return resultList;
        }
    }
}