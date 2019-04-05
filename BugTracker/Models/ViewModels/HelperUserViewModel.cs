using BugTracker.MyHelpers.DB_Repositories;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels
{
    public class HelperUserViewModel
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }

        public List<IdentityRole> Roles { get; set; }

        public static HelperUserViewModel CreateViewModel(ApplicationUser applicationUser, ApplicationDbContext dbContext)
        {
            if (applicationUser == null)
            {
                throw new ArgumentNullException(nameof(applicationUser));
            }
            var roles = new UserRoleRepository(dbContext).GetUserRoles(applicationUser.Id);
            try
            {
                return new HelperUserViewModel()
                {
                    Id = string.IsNullOrWhiteSpace(applicationUser.Id) ? throw new ArgumentNullException() : applicationUser.Id,
                    DisplayName = string.IsNullOrWhiteSpace(applicationUser.DisplayName) ? throw new ArgumentNullException() : applicationUser.DisplayName,
                    Email = string.IsNullOrWhiteSpace(applicationUser.Email) ? throw new ArgumentNullException() : applicationUser.Email,
                    Roles = roles != null ? (!roles.Any() ? new List<IdentityRole>() : roles) : new List<IdentityRole>(),
                };
            }
            catch (ArgumentNullException e)
            {
                throw new ArgumentException($"{e.Message}");
            }
        }
    }
}