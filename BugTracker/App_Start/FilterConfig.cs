using BugTracker.Models.Filters.Actions;
using BugTracker.Models.Filters.Authorize;
using System.Web.Mvc;

namespace BugTracker
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new Benchmark());
            filters.Add(new BugTrackerAuthorize());
            filters.Add(new OverrideCurrentNavLinkStyle(null));
        }
    }
}
