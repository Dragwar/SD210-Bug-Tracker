﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BugTracker.Models;
using BugTracker.Models.Filters.Actions;
using BugTracker.Models.Filters.Authorize;
using BugTracker.Models.ViewModels;
using BugTracker.Models.ViewModels.UserRole;
using BugTracker.MyHelpers;
using BugTracker.MyHelpers.DB_Repositories;
using Microsoft.AspNet.Identity;

namespace BugTracker.Controllers
{
    [BugTrackerAuthorize(nameof(UserRolesEnum.Admin))]
    public class UserRoleController : Controller
    {
        public readonly ApplicationDbContext DbContext;
        public readonly UserRepository UserRepository;
        public readonly UserRoleRepository UserRoleRepository;

        public UserRoleController()
        {
            DbContext = new ApplicationDbContext();
            UserRepository = new UserRepository(DbContext);
            UserRoleRepository = new UserRoleRepository(DbContext);
        }

        // GET: UserRole
        public ActionResult Index(string error)
        {
            List<HelperUserViewModel> model = UserRepository
                .GetAllUsers()
                .Where(user => !user.Email.ToLower().Contains("demo-"))
                .ToList()
                .Select(user => HelperUserViewModel.CreateNewViewModel(user, DbContext))
                .ToList();

            if (model == null)
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                ModelState.AddModelError("", error);
            }

            return View(model);
        }

        private ManageRolesViewModel GenerateManageRolesViewModel(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            ApplicationUser foundUser = UserRepository.GetUserById(id);
            if (foundUser == null)
            {
                return null;
            }
            string currentAdminUserId = User.Identity.GetUserId();
            ManageRolesViewModel model = ManageRolesViewModel.CreateNewViewModel(currentAdminUserId, foundUser, UserRoleRepository.GetAllUserRoles(), DbContext);
            return model;
        }

        // GET: UserRole/ManageRoles/{id}
        [OverrideCurrentNavLinkStyle("userrole-index")]
        public ActionResult ManageRoles(string id) => View(GenerateManageRolesViewModel(id));

        // POST: UserRole/ManageRoles/{id}
        [HttpPost]
        public ActionResult ManageRoles(ManageRolesViewModel formData)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(GenerateManageRolesViewModel(formData.UserId));
                }

                ApplicationUser foundUser = UserRepository.GetUserById(formData.UserId);

                if (foundUser == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                bool isAddingUsers = formData?.SelectedRolesToAdd != null;
                bool isRemovingUsers = formData?.SelectedRolesToRemove != null;

                string userId = User.Identity.GetUserId();

                //! other admins can revoke other admins role
                //! but they can't revoke their own admin role
                if (isRemovingUsers && formData.UserId == userId && formData.SelectedRolesToRemove.Contains(nameof(UserRolesEnum.Admin)))
                {
                    return RedirectToAction(nameof(Index), new { error = "You can't revoke your admin role (discarded all changes)" });
                }

                #region Adding and Removing Users
                if (isAddingUsers)
                {
                    foreach (string roleName in formData.SelectedRolesToAdd)
                    {
                        bool isUserAlreadyAssignedToRole = UserRoleRepository.IsUserInRole(formData.UserId, roleName);

                        if (!isUserAlreadyAssignedToRole)
                        {
                            bool didUserGetAssignedToRole = UserRoleRepository.AddUserToRole(formData.UserId, roleName);

                            if (!didUserGetAssignedToRole)
                            {
                                return RedirectToAction(nameof(Index));
                            }
                        }
                    }
                }

                if (isRemovingUsers)
                {
                    foreach (string roleName in formData.SelectedRolesToRemove)
                    {
                        bool isUserAlreadyAssignedToRole = UserRoleRepository.IsUserInRole(formData.UserId, roleName);

                        if (isUserAlreadyAssignedToRole)
                        {
                            //ApplicationUser foundUser = UserRepository.GetUserById(userId);
                            bool didUserGetRoleRevoked = UserRoleRepository.RemoveUserFromRole(formData.UserId, roleName);

                            if (!didUserGetRoleRevoked)
                            {
                                return RedirectToAction(nameof(Index));
                            }
                        }
                    }
                }
                #endregion

                DbContext.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException e)
            {
                ModelState.AddModelError("", e.Message);
                return View(GenerateManageRolesViewModel(formData.UserId));
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", e.Message);
                return View(GenerateManageRolesViewModel(formData.UserId));
            }
        }
    }
}
