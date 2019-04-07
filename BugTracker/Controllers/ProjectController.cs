using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.ViewModels;
using BugTracker.Models.ViewModels.Project;
using BugTracker.MyHelpers;
using BugTracker.MyHelpers.DB_Repositories;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using IndexViewModel = BugTracker.Models.ViewModels.Project.IndexViewModel;

namespace BugTracker.Controllers
{
    [Authorize(Roles = nameof(UserRolesEnum.Admin) + "," + nameof(UserRolesEnum.ProjectManager))]
    public class ProjectController : Controller
    {
        private ApplicationDbContext DbContext { get; }
        private ProjectRepository ProjectRepository { get; }
        private UserRepository UserRepository { get; }
        private UserRoleRepository UserRoleRepository { get; }

        public ProjectController()
        {
            DbContext = new ApplicationDbContext();
            ProjectRepository = new ProjectRepository(DbContext);
            UserRepository = new UserRepository(DbContext);
            UserRoleRepository = new UserRoleRepository(DbContext);
        }

        // GET: Project
        [AllowAnonymous]
        public ActionResult Index()
        {
            List<Project> userProjects = ProjectRepository.GetUserProjects(User.Identity.GetUserId());
            List<IndexViewModel> viewModels;

            if (userProjects.Any())
            {
                viewModels = userProjects.Select(project => IndexViewModel.CreateNewViewModel(project)).ToList();
            }
            else
            {
                viewModels = new List<IndexViewModel>();
            }

            return View(viewModels);
        }

        // GET: Project/All
        public ActionResult All()
        {
            bool isUserAdmin = UserRoleRepository.IsUserInRole(User.Identity.GetUserId(), nameof(UserRolesEnum.Admin));
            bool isUserProjectManager = UserRoleRepository.IsUserInRole(User.Identity.GetUserId(), nameof(UserRolesEnum.ProjectManager));

            // shouldn't happen ever
            if (!isUserAdmin && !isUserProjectManager)
            {
                ModelState.AddModelError("", "Error - You don't have permission to view this page");
                return RedirectToAction(nameof(Index));
            }

            List<Project> allProjects = ProjectRepository.GetAllProjects();
            List<IndexViewModel> viewModels;

            if (allProjects.Any())
            {
                viewModels = allProjects.Select(project => IndexViewModel.CreateNewViewModel(project)).ToList();
            }
            else
            {
                viewModels = new List<IndexViewModel>();
            }

            return View(viewModels);
        }

        // GET: Project/Details/{id}
        [AllowAnonymous]
        public ActionResult Details(string id)
        {
            string userId = User.Identity.GetUserId();
            Project foundProject = ProjectRepository.GetProject(id);

            if (foundProject == null)
            {
                return RedirectToAction(nameof(Index));
            }
            else if (string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction(nameof(Index));
            }

            bool isUserAllowToSeeProject = ProjectRepository.IsUserAssignedToProject(userId, foundProject);

            if (!string.IsNullOrWhiteSpace(userId) && UserRoleRepository.IsUserInRole(userId, nameof(UserRolesEnum.Admin)) || UserRoleRepository.IsUserInRole(userId, nameof(UserRolesEnum.ProjectManager)))
            {
                isUserAllowToSeeProject = true;
            }

            if (!isUserAllowToSeeProject)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(DetailsViewModel.CreateNewViewModel(foundProject, DbContext));
        }

        // GET: Project/Create
        public ActionResult Create()
        {
            SelectListGroup initialAssignedUserGroup = new SelectListGroup()
            {
                Name = "Assign Initial Users",
                Disabled = false,
            };

            string userId = User.Identity.GetUserId();

            List<SelectListItem> userList = UserRepository.GetAllUsers().Select(user => new SelectListItem()
            {
                Text = user.DisplayName,
                Value = user.Id,
                Selected = false,
                Disabled = false,
                Group = initialAssignedUserGroup,
            }).ToList();

            SelectListItem projectCreator = userList.FirstOrDefault(user => user.Value == userId);

            if (projectCreator == null)
            {
                return RedirectToAction(nameof(Index));
            }

            projectCreator.Selected = true; // set the project creator selected by default

            CreateViewModel model = new CreateViewModel()
            {
                Name = null,
                SelectedUsersToAdd = null,
                Users = new List<HelperUserViewModel>(),
                UsersAddList = userList,
            };

            return View(model);
        }

        // POST: Project/Create
        [HttpPost]
        public ActionResult Create(CreateViewModel formData)
        {
            try
            {
                // TODO: Add insert logic here
                #region Valid FormData Checks
                if (!ModelState.IsValid || formData == null)
                {
                    ModelState.AddModelError("", "Error - Bad form data");
                    return View(new CreateViewModel()
                    {
                        Name = null,
                        SelectedUsersToAdd = null,
                        Users = new List<HelperUserViewModel>(),
                        UsersAddList = UserRepository.GetAllUsers().Select(user => new SelectListItem()
                        {
                            Text = user.DisplayName,
                            Value = user.Id,
                            Selected = false,
                            Disabled = false,
                            Group = new SelectListGroup()
                            {
                                Name = "Assign Initial Users",
                                Disabled = false,
                            },
                        }).ToList(),
                    });
                }

                if (string.IsNullOrWhiteSpace(formData.Name))
                {
                    ModelState.AddModelError("", "Error - Project Name is invalid");
                    return View(formData);
                }

                formData.Name = formData.Name.Trim();

                if (ProjectRepository.IsProjectNameAlreadyTaken(formData.Name))
                {
                    ModelState.AddModelError("", "Error - Project Name is already taken");
                    return View(formData);
                }
                #endregion

                Project newProject = new Project()
                {
                    Name = formData.Name,
                };

                bool isAddingUsers = formData.SelectedUsersToAdd != null;

                if (isAddingUsers)
                {
                    foreach (string userId in formData.SelectedUsersToAdd)
                    {
                        ApplicationUser foundUser = UserRepository.GetUserById(userId);

                        if (foundUser == null)
                        {
                            return RedirectToAction(nameof(Index));
                        }

                        newProject.Users.Add(foundUser);
                    }
                }

                DbContext.Projects.Add(newProject);

                DbContext.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Error");
                return View();
            }
        }



        // GET: Project/Edit/{id}
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Index));
            }

            Project foundProject = ProjectRepository.GetProject(id);

            if (foundProject == null)
            {
                return RedirectToAction(nameof(Index));
            }

            EditViewModel model = EditViewModel.CreateNewViewModel(foundProject, DbContext, UserRepository);

            return View(model);
        }

        // POST: Project/Edit/{id}
        [HttpPost]
        public ActionResult Edit(EditViewModel formData)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ModelState.AddModelError("", "Error - Bad Data");
                    EditViewModel model;

                    #region Fixes a bug where project add/remove users lists were null (when name was just whitespace)
                    if (formData.UsersAddList == null || formData.UsersRemoveList == null)
                    {
                        Project project = ProjectRepository.GetProject(formData.Id);
                        if (project == null)
                        {
                            return RedirectToAction(nameof(Index));
                        }
                        model = EditViewModel.CreateNewViewModel(project, DbContext, UserRepository);
                        model.Name = formData.Name;
                    }
                    else
                    {
                        model = formData;
                    }
                    #endregion
                    return View(model);
                }

                if (string.IsNullOrWhiteSpace(formData.Name))
                {
                    ModelState.AddModelError(nameof(formData.Name), "Project Name can't be left empty");
                    return View(formData);
                }

                Project foundProject = ProjectRepository.GetProject(formData.Id);

                if (foundProject == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                bool isAddingUsers = formData.SelectedUsersToAdd != null;
                bool isRemovingUsers = formData.SelectedUsersToRemove != null;

                if (isAddingUsers)
                {
                    foreach (string userId in formData.SelectedUsersToAdd)
                    {
                        bool isUserAlreadyAssignedToProject = ProjectRepository.IsUserAssignedToProject(userId, foundProject);

                        if (!isUserAlreadyAssignedToProject)
                        {
                            ApplicationUser foundUser = UserRepository.GetUserById(userId);
                            bool didUserGetAssignedToProject = ProjectRepository.AssignUserToProject(foundUser, foundProject.Id.ToString());

                            if (!didUserGetAssignedToProject)
                            {
                                return RedirectToAction(nameof(Index));
                            }
                        }
                    }
                }

                if (isRemovingUsers)
                {
                    foreach (string userId in formData.SelectedUsersToRemove)
                    {
                        bool isUserAssignedToProject = ProjectRepository.IsUserAssignedToProject(userId, foundProject);

                        if (isUserAssignedToProject)
                        {
                            //ApplicationUser foundUser = UserRepository.GetUserById(userId);
                            bool didUserGetUnassignedFromProject = ProjectRepository.UnassignUserFromProject(userId, foundProject.Id.ToString());

                            if (!didUserGetUnassignedFromProject)
                            {
                                return RedirectToAction(nameof(Index));
                            }
                        }
                    }
                }

                foundProject.Name = formData.Name;
                foundProject.DateUpdated = DateTime.Now;

                DbContext.SaveChanges();

                return RedirectToAction(nameof(Details), new { Id = formData.Id });
            }
            catch
            {
                return View(formData);
            }
        }

        //? Is project deletion needed?
        //// GET: Project/Delete/{id}
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: Project/Delete/{id}
        //[HttpPost]
        //public ActionResult Delete(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add delete logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}
