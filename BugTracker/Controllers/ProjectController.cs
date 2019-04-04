using BugTracker.Models;
using BugTracker.Models.ViewModels.Project;
using BugTracker.Models.Domain;
using BugTracker.MyHelpers;
using BugTracker.MyHelpers.DB_Repositories;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IndexViewModel = BugTracker.Models.ViewModels.Project.IndexViewModel;

namespace BugTracker.Controllers
{
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
        public ActionResult Index()
        {
            List<Project> userProjects = ProjectRepository.GetUserProjects(User.Identity.GetUserId());
            List<IndexViewModel> viewModel;
            
            if (userProjects.Any())
            {
                viewModel = userProjects.Select(project => IndexViewModel.CreateViewModel(project)).ToList();
            }
            else
            {
                viewModel = new List<IndexViewModel>();
            }

            return View(viewModel);
        }

        // GET: Project/All
        [Authorize(Roles = nameof(UserRolesEnum.Admin) + "," + nameof(UserRolesEnum.ProjectManager))]
        public ActionResult All()
        {
            bool isUserAdmin = UserRoleRepository.IsUserInRole(User.Identity.GetUserId(), nameof(UserRolesEnum.Admin));
            return View();
        }

        // GET: Project/Details/{id}
        public ActionResult Details(Guid id)
        {
            return View();
        }

        // GET: Project/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Project/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Project/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Project/Edit/5
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

        // GET: Project/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Project/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
