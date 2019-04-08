using BugTracker.MyHelpers;
using BugTracker.MyHelpers.DB_Repositories;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace BugTracker.Models.ViewModels.UserRole
{
    public class ManageRolesViewModel
    {
        public string UserId { get; set; }

        [Display(Name = "Display Name")]
        public string UserDisplayName { get; set; }

        [Display(Name = "Email")]
        public string UserEmail { get; set; }

        [Display(Name = "Assigned Projects")]
        public int NumberOfAssginedProjects { get; set; }
        public List<SelectListItem> RolesAddList { get; set; }
        public List<SelectListItem> RolesRemoveList { get; set; }

        [Display(Name = "Assign Roles")]
        public string[] SelectedRolesToAdd { get; set; }

        [Display(Name = "Revoke Roles")]
        public string[] SelectedRolesToRemove { get; set; }

        public static ManageRolesViewModel CreateNewViewModel(string currentUserId, ApplicationUser user, List<IdentityRole> allRoles, ApplicationDbContext dbContext)
        {
            if (allRoles == null || dbContext == null || user == null)
            {
                throw new ArgumentNullException();
            }
            UserRoleRepository repo = new UserRoleRepository(dbContext);
            List<IdentityRole> rolesWithoutUser = new List<IdentityRole>();
            foreach (IdentityRole role in allRoles)
            {
                if (!repo.IsUserInRole(user.Id, role.Name))
                {
                    rolesWithoutUser.Add(role);
                }
            }
            try
            {
                List<SelectListItem> rolesRemove = new List<SelectListItem>();
                List<SelectListItem> rolesAdd = new List<SelectListItem>();

                SelectListGroup rolesAddGroup = new SelectListGroup()
                {
                    Name = "Roles To Add",
                    Disabled = false,
                };
                SelectListGroup rolesRemoveGroup = new SelectListGroup()
                {
                    Name = "Roles To Remove",
                    Disabled = false,
                };

                // remove roles
                foreach (IdentityUserRole userRole in user.Roles)
                {
                    string currentRoleName = allRoles.FirstOrDefault(r => r.Id == userRole.RoleId)?.Name ?? throw new Exception("Role not found");

                    //! Double check if the user is in role and Disable an SelectListItem to prevent the admin role from getting revoked
                    //! you can now revoke other admin's admin roles but you yourself can't remove your own admin role
                    if (repo.IsUserInRole(user.Id, currentRoleName) && currentUserId == userRole.UserId && currentRoleName == nameof(UserRolesEnum.Admin))
                    {
                        rolesRemove.Add(new SelectListItem()
                        {
                            Text = currentRoleName,
                            Value = null,
                            Group = rolesRemoveGroup,
                            Selected = false,
                            Disabled = true,
                        });
                    }
                    else if (repo.IsUserInRole(user.Id, currentRoleName))
                    {
                        rolesRemove.Add(new SelectListItem()
                        {
                            Text = currentRoleName,
                            Value = currentRoleName,
                            Group = rolesRemoveGroup,
                            Selected = false,
                            Disabled = false,
                        });
                    }
                }

                // add roles
                foreach (IdentityRole role in rolesWithoutUser)
                {
                    if (!repo.IsUserInRole(user.Id, role.Name))
                    {
                        rolesAdd.Add(new SelectListItem()
                        {
                            Text = role.Name,
                            Value = role.Name,
                            Group = rolesAddGroup,
                            Selected = false,
                            Disabled = false,
                        });
                    }
                }


                ManageRolesViewModel model = new ManageRolesViewModel()
                {
                    UserId = user.Id.ToString(),
                    UserDisplayName = user.DisplayName,
                    UserEmail = user.Email,
                    NumberOfAssginedProjects = (user.Projects?.Any() ?? false) ? user.Projects.Count : 0,
                    RolesAddList = rolesAdd,
                    RolesRemoveList = rolesRemove,
                    SelectedRolesToAdd = null,
                    SelectedRolesToRemove = null,
                };

                return model;
            }
            catch
            {
                throw new Exception();
            }
        }
    }
}