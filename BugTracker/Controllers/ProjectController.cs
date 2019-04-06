using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.ViewModels;
using BugTracker.Models.ViewModels.Project;
using BugTracker.MyHelpers;
using BugTracker.MyHelpers.DB_Repositories;
using Microsoft.AspNet.Identity;
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
            CreateViewModel model = new CreateViewModel() { AddProjectCreatorToNewProject = true, Users = new List<HelperUserViewModel>() };
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
                    return View(new CreateViewModel() { AddProjectCreatorToNewProject = true, Users = new List<HelperUserViewModel>() });
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

                if (formData.AddProjectCreatorToNewProject)
                {
                    ApplicationUser projectCreator = UserRepository.GetUserById(User.Identity.GetUserId());

                    if (projectCreator == null)
                    {
                        ModelState.AddModelError("", "Error - Current user was not found");
                        return RedirectToAction(nameof(Index));
                    }

                    newProject.Users.Add(projectCreator);
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
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Project/Edit/{id}
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
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
