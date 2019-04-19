using BugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BugTracker.MyHelpers.DB_Repositories
{
    [NotMapped]
    public class UserRoleRepository
    {
        private readonly UserManager<ApplicationUser> UserManager;
        private readonly ApplicationDbContext DbContext;

        public UserRoleRepository(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
            UserManager = OwinContextExtensions.GetUserManager<ApplicationUserManager>(HttpContext.Current.GetOwinContext());
        }

        public IQueryable<IdentityRole> GetAllUserRoles() => DbContext.Roles.AsQueryable();
        public bool IsUserInRole(string userId, string roleName) => UserManager.IsInRole(userId, roleName);
        public bool IsUserInRole(string userId, UserRolesEnum roleName) => UserManager.IsInRole(userId, roleName.ToString());
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
                IList<string> currentRoleList = UserManager.GetRoles(user.Id);
                if (IsUserInRole(user.Id, roleName.ToString()))
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

        /// <summary>
        ///     <para/>This relies on the UserRoleEnum to match the DataBase.    
        ///     <para/>See method below for implementation without using the UserRoleEnum.
        /// </summary>
        /// <param name="userId">Represents the user that will be checked on the database via UserManager</param>
        /// <returns>A read-only dictionary containing KeyValuePairs (Key: UserRolesEnum RoleName, Value: bool isUserInRole)</returns>
        public IReadOnlyDictionary<UserRolesEnum, bool> GetIsUserInRoleDictionary(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Pass in a valid userId", nameof(userId));
            }

            //! Both of these ways requires the enum to match the database exactly
            Dictionary<UserRolesEnum, bool> isInRole = Enum.GetValues(typeof(UserRolesEnum))
                .Cast<UserRolesEnum>()
                .ToDictionary(key => key, x => false);

            //Dictionary<UserRolesEnum, bool> isInRole = DbContext.Roles
            //    .Select(role => role.Id)
            //    .Cast<UserRolesEnum>()
            //    .ToDictionary(key => key, x => false);

            if (IsUserInRole(userId, UserRolesEnum.Admin))
            {
                isInRole[UserRolesEnum.Admin] = true;
            }

            if (IsUserInRole(userId, UserRolesEnum.ProjectManager))
            {
                isInRole[UserRolesEnum.ProjectManager] = true;
            }

            if (IsUserInRole(userId, UserRolesEnum.Submitter))
            {
                isInRole[UserRolesEnum.Submitter] = true;
            }

            if (IsUserInRole(userId, UserRolesEnum.Developer))
            {
                isInRole[UserRolesEnum.Developer] = true;
            }

            if (isInRole.Any(pair => pair.Value == true))
            {
                isInRole[UserRolesEnum.None] = false;
            }
            else
            {
                isInRole[UserRolesEnum.None] = true;
            }

            return isInRole;
        }

        /// <summary>
        ///     This version doesn't rely on enum but if all the roleNames in rolesToCheck doesn't match any in the database then a exception will be thrown
        /// </summary>
        /// <param name="userId">Represents the user that will be checked on the database via UserManager</param>
        /// <param name="rolesToCheck">All the roles that will be in the dictionary (roleNames that aren't in the database will be ignored)</param>
        /// <returns>A read-only dictionary containing KeyValuePairs (Key: string RoleName, Value: bool isUserInRole)</returns>
        public IReadOnlyDictionary<string, bool> GetIsUserInRoleDictionary(string userId, params string[] rolesToCheck)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Pass in a valid userId", nameof(userId));
            }
            else if (rolesToCheck == null || rolesToCheck.Length <= 0)
            {
                throw new ArgumentException($"{nameof(rolesToCheck)} was null or less then or equal to zero");
            }

            Dictionary<string, bool> isInRole = DbContext.Roles
                .Where(role => rolesToCheck.Contains(role.Name))
                .Select(role => role.Name)
                .ToDictionary(key => key, x => false);

            if (!isInRole.Any())
            {
                throw new ArgumentException($"{nameof(rolesToCheck)}, roles passed in did not match any in the database");
            }

            foreach (string roleName in rolesToCheck)
            {
                if (IsUserInRole(userId, roleName))
                {
                    isInRole[roleName] = true;
                }
            }

            return isInRole;
        }
    }
}