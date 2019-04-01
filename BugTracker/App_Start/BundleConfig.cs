using System.Web.Optimization;

namespace BugTracker
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            // My Bundles
            bundles.Add(new StyleBundle(@"~/MyContent/css").Include(
                @"~/Content\MyStyles\MyStyles.css"));

            bundles.Add(new ScriptBundle(@"~/MyContent/js").Include(
                @"~/Scripts\MyScripts\MyScript.js"));

            bundles.Add(new StyleBundle(@"~/Template/css").Include(
                @"~/Content\TemplateCSS\animate.min.css",

                // I think this was just for the demo in the live preview of the template
                //@"~/Content\TemplateCSS\demo.css",

                @"~/Content\TemplateCSS\light-bootstrap-dashboard.css",
                @"~/Content\TemplateCSS\pe-icon-7-stroke.css",
                @"~/Content\site.css"));

            bundles.Add(new ScriptBundle(@"~/Template/js").Include(
                @"~/Scripts\TemplateScripts\bootstrap-notify.js",
                @"~/Scripts\TemplateScripts\bootstrap-select.js",
                @"~/Scripts\TemplateScripts\chartist.min.js",

                // I think this was just for the demo in the live preview of the template
                //@"~/Scripts\TemplateScripts\demo.js",

                @"~/Scripts\TemplateScripts\light-bootstrap-dashboard.js"));

            bundles.Add(new StyleBundle(@"~/font-awesome", @"http://maxcdn.bootstrapcdn.com/font-awesome/4.2.0/css/font-awesome.min.css"));
            bundles.Add(new StyleBundle(@"~/fonts-google", @"http://fonts.googleapis.com/css?family=Roboto:400,700,300"));

            BundleTable.EnableOptimizations = true;
            bundles.UseCdn = true;
        }
    }
}
