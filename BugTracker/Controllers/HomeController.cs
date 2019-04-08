using BugTracker.Models.ViewModels.Home;
using BugTracker.MyHelpers.DB_Repositories;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class HomeController : Controller
    {
        private Models.ApplicationDbContext DbContext { get; }
        private UserRepository UserRepository { get; }

        public HomeController()
        {
            DbContext = new Models.ApplicationDbContext();
            UserRepository = new UserRepository(DbContext);
        }


        public ActionResult Index()
        {
            IndexViewModel model;
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();
                var currentUser = UserRepository.GetUserById(userId);
                model = IndexViewModel.CreateNewViewModel(currentUser, DbContext, 5);
            }
            else
            {
                model = new IndexViewModel()
                {
                    UserId = "",
                    DisplayName = "Guest User",
                    Email = "",
                    LatestProjects = new List<Models.ViewModels.Project.IndexViewModel>(),
                    Roles = new List<IdentityRole>(),
                };
            }

            return View(model);
        }
    }
}