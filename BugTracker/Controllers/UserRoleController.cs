using BugTracker.Models;
using BugTracker.Models.ViewModels;
using BugTracker.Models.ViewModels.UserRole;
using BugTracker.MyHelpers;
using BugTracker.MyHelpers.DB_Repositories;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    [Authorize(Roles = nameof(UserRolesEnum.Admin))]
    public class UserRoleController : Controller
    {
        public ApplicationDbContext DbContext { get; set; }
        public UserRepository UserRepository { get; set; }
        public UserRoleRepository UserRoleRepository { get; set; }

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

        // GET: UserRole/ManageRoles/{id}
        public ActionResult ManageRoles(string id)
        {
            ViewBag.OverrideCurrentPage = "userrole-index";
            ApplicationUser foundUser = UserRepository.GetUserById(id);
            if (foundUser == null)
            {
                return RedirectToAction(nameof(Index));
            }
            string currentAdminUserId = User.Identity.GetUserId();
            ManageRolesViewModel model = ManageRolesViewModel.CreateNewViewModel(currentAdminUserId, foundUser, UserRoleRepository.GetAllUserRoles(), DbContext);

            return View(model);
        }

        // POST: UserRole/ManageRoles/{id}
        [HttpPost]
        public ActionResult ManageRoles(ManageRolesViewModel formData)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(formData);
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
            catch
            {
                return View(formData);
            }
        }
    }
}
