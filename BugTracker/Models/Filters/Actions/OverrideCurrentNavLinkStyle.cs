using System.Web.Mvc;

namespace BugTracker.Models.Filters.Actions
{
    /// <summary>
    ///     <para/>Use this to set/override the active nav-link on the layout page
    ///     <para/>This will send the controller name and action name to the view
    /// </summary>
    public class OverrideCurrentNavLinkStyle : ActionFilterAttribute
    {
        public OverrideCurrentNavLinkStyle(string overrideString)
        {
            OverrideString = overrideString;
        }

        /// <summary>
        ///     <para/>This is for manually overriding the nav-link that gets the active class
        ///     <para/>"{controller}-{action}".ToLower(); 
        ///     <para/>Home Controller and Index Action --> "home-index"
        ///     <para/>To set all nav links to inactive set this property to "none" || to some non-relevant string
        /// </summary>
        public string OverrideString { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!string.IsNullOrWhiteSpace(OverrideString))
            {
                filterContext.Controller.ViewBag.OverrideCurrentPage = OverrideString;
            }
            else
            {
                string actionName = filterContext.ActionDescriptor.ActionName.ToLower();
                string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.ToLower();
                filterContext.Controller.ViewBag.OverrideCurrentPage = $"{controllerName}-{actionName}";
            }
        }
    }
}