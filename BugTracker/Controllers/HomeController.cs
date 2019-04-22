using BugTracker.Models;
using BugTracker.Models.Filters.Actions;
using BugTracker.Models.ViewModels.Home;
using BugTracker.Models.ViewModels.Project;
using BugTracker.MyHelpers.DB_Repositories;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext DbContext;
        private readonly UserRepository UserRepository;

        public HomeController()
        {
            DbContext = new ApplicationDbContext();
            UserRepository = new UserRepository(DbContext);
        }

        public ActionResult Index()
        {
            HomeIndexViewModel model;
            if (User.Identity.IsAuthenticated)
            {
                string userId = User.Identity.GetUserId();
                ApplicationUser currentUser = UserRepository.GetUserById(userId);
                model = HomeIndexViewModel.CreateNewViewModel(currentUser, DbContext, 5);
            }
            else
            {
                model = new HomeIndexViewModel()
                {
                    UserId = "",
                    DisplayName = "Guest User",
                    Email = "",
                    LatestProjects = new List<ProjectIndexViewModel>(),
                    Roles = new List<IdentityRole>(),
                };
            }

            return View(model);
        }

        // TODO: make a custom error page
        [OverrideCurrentNavLinkStyle("none")]
        public ActionResult UnauthorizedRequest(string error)
        {
            ViewBag.PermissionError = TempData?["PermissionError"]?.ToString();
            ViewBag.Error = error;
            return View();
        }
    }
}