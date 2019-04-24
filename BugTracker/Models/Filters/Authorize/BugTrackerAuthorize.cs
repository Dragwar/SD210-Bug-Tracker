using System.Web.Mvc;
using System.Web.Routing;
using BugTracker.Controllers;
using BugTracker.MyHelpers;

namespace BugTracker.Models.Filters.Authorize
{
    public class BugTrackerAuthorize : AuthorizeAttribute
    {
        public BugTrackerAuthorize() { }

        public BugTrackerAuthorize(params string[] roles) => Roles = string.Join(",", roles);

        public BugTrackerAuthorize(params UserRolesEnum[] roles) => Roles = string.Join(",", roles);


        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // {string.Join(", ", filterContext.RequestContext.RouteData.Values.Values)}
            // Displays: {controller}, {action} (and maybe {QueryString/QueryVariables} as well)
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