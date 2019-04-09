using BugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.Owin.Builder;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System.Data.Entity;
using System.Web;

namespace BugTracker.MyHelpers.DB_Repositories
{
    [NotMapped]
    public class UserRoleRepository
    {
        private UserManager<ApplicationUser> UserManager;
        private ApplicationDbContext DbContext;

        public UserRoleRepository(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
            UserManager = OwinContextExtensions.GetUserManager<ApplicationUserManager>(HttpContext.Current.GetOwinContext());
        }

        public List<IdentityRole> GetAllUserRoles() => DbContext.Roles.ToList();
        public bool IsUserInRole(string userId, string roleName) => UserManager.IsInRole(userId, roleName);
        public bool IsUserInRole(string userId, UserRolesEnum roleName) => UserManager.IsInRole(userId, nameof(roleName));
        public List<string> ListUserRoles(string userId) => UserManager.GetRoles(userId).ToList();
        public List<IdentityRole> GetUserRoles(string userId) => GetAllUserRoles().Where(role => role.Users.FirstOrDefault(user => user.UserId == userId) != null).ToList();
        public bool AddUserToRole(string userId, string roleName)
        {
            IdentityResult result = UserManager.AddToRole(userId, roleName);
            return result.Succeeded;
        }
        public bool AddUserToRole(string userId, UserRolesEnum roleName)
        {
            IdentityResult result = UserManager.AddToRole(userId, nameof(roleName));
            return result.Succeeded;
        }
        public bool RemoveUserFromRole(string userId, string roleName)
        {
            IdentityResult result = UserManager.RemoveFromRole(userId, roleName);
            return result.Succeeded;
        }
        public bool RemoveUserFromRole(string userId, UserRolesEnum roleName)
        {
            IdentityResult result = UserManager.RemoveFromRole(userId, nameof(roleName));
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
        public List<ApplicationUser> UsersInRole(UserRolesEnum roleName)
        {
            List<ApplicationUser> resultList = new List<ApplicationUser>();
            List<ApplicationUser> List = UserManager.Users.ToList();

            foreach (ApplicationUser user in List)
            {
                if (IsUserInRole(user.Id, nameof(roleName)))
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
        public List<ApplicationUser> UsersNotInRole(UserRolesEnum roleName)
        {
            List<ApplicationUser> resultList = new List<ApplicationUser>();
            List<ApplicationUser> List = UserManager.Users.ToList();

            foreach (ApplicationUser user in List)
            {
                if (!IsUserInRole(user.Id, nameof(roleName)))
                {
                    resultList.Add(user);
                }
            }

            return resultList;
        }
    }
}