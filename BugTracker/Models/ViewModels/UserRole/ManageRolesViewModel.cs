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
        public List<SelectListItem> RolesAddList { get; set; }
        public List<SelectListItem> RolesRemoveList { get; set; }

        [Display(Name = "Assign Roles")]
        public string[] SelectedRolesToAdd { get; set; }

        [Display(Name = "Revoke Roles")]
        public string[] SelectedRolesToRemove { get; set; }

        public static ManageRolesViewModel CreateNewViewModel(ApplicationUser user, List<IdentityRole> allRoles, ApplicationDbContext dbContext)
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
                List<SelectListItem> RolesRemove = new List<SelectListItem>();
                List<SelectListItem> RolesAdd = new List<SelectListItem>();

                SelectListGroup RolesAddGroup = new SelectListGroup()
                {
                    Name = "Roles To Add",
                    Disabled = false,
                };
                SelectListGroup RolesRemoveGroup = new SelectListGroup()
                {
                    Name = "Roles To Remove",
                    Disabled = false,
                };

                // remove roles
                foreach (IdentityUserRole userRole in user.Roles)
                {
                    string currentRoleName = allRoles.First(r => r.Id == userRole.RoleId)?.Name ?? throw new Exception("Role not found");

                    //! Double check if the user is in role and Disable an SelectListItem to prevent the admin role from getting revoked
                    if (repo.IsUserInRole(user.Id, currentRoleName) && currentRoleName == nameof(UserRolesEnum.Admin))
                    {
                        RolesRemove.Add(new SelectListItem()
                        {
                            Text = currentRoleName,
                            Value = null,
                            Group = RolesRemoveGroup,
                            Selected = false,
                            Disabled = true,
                        });
                    }
                    else if (repo.IsUserInRole(user.Id, currentRoleName))
                    {
                        RolesRemove.Add(new SelectListItem()
                        {
                            Text = currentRoleName,
                            Value = currentRoleName,
                            Group = RolesRemoveGroup,
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
                        RolesAdd.Add(new SelectListItem()
                        {
                            Text = role.Name,
                            Value = role.Name,
                            Group = RolesAddGroup,
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
                    RolesAddList = RolesAdd,
                    RolesRemoveList = RolesRemove,
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