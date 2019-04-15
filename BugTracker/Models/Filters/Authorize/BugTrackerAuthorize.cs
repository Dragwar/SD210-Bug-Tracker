using BugTracker.Controllers;
using System.Web.Mvc;
using System.Web.Routing;

namespace BugTracker.Models.Filters.Authorize
{
    public class BugTrackerAuthorize : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectToRouteResult(
                   new RouteValueDictionary()
                   {
                        { "controller", "Home" },
                        { "action", nameof(HomeController.UnauthorizedRequest) }
                   });
                filterContext.Controller.TempData["PermissionError"] = $"You don't have the required permissions to access {string.Join(", ", filterContext.RequestContext.RouteData.Values.Values)} page";
            }
            else
            {
                filterContext.Controller.TempData["PermissionError"] = $"You need to be logged in to view {string.Join(", ", filterContext.RequestContext.RouteData.Values.Values)} page";
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}