using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}