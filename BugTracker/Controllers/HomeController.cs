using System.Web.Mvc;
using BugTracker.Models;
using BugTracker.Models.Filters.Actions;
using BugTracker.Models.ViewModels.Home;
using BugTracker.MyHelpers.DB_Repositories;
using Microsoft.AspNet.Identity;

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
            string userId = User.Identity.GetUserId();
            ApplicationUser currentUser = UserRepository.GetUserById(userId);
            HomeIndexViewModel model = HomeIndexViewModel.CreateNewViewModel(currentUser, DbContext, 5, 5, 5);

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