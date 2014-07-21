using System.Web.Optimization;

namespace BrandValues
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/Bundles/webapp").Include(
                        "~/Scripts/webapp/bootstrap.min.js",
                        "~/Scripts/webapp/app.min.js",
                        "~/Scripts/webapp/slimscroll/jquery.slimscroll.min.js",
                        "~/Scripts/webapp/app.plugin.js"
                        ));

            bundles.Add(new ScriptBundle("~/Bundles/account").Include(
                        "~/Scripts/webapp/bootstrap.min.js",
                        "~/Scripts/webapp/app.min.js",
                        "~/Scripts/webapp/app.plugin.js"
                        ));

            bundles.Add(new ScriptBundle("~/Bundles/video").Include(
                        "~/Scripts/webapp/jwplayer/jwplayer.js",
                        "~/Scripts/webapp/jwplayer/jwplayer.html5.js",
                        "~/Scripts/webapp/jwplayer/demo.js"
                        ));

            bundles.Add(new ScriptBundle("~/Bundles/upload").Include(
                        "~/Scripts/webapp/parsley/parsley.min.js",
                        "~/Scripts/webapp/wizard/jquery.bootstrap.wizard.js",
                        "~/Scripts/webapp/file-input/bootstrap-filestyle.min.js",
                        "~/Scripts/webapp/wizard/demo.js"
                        ));

            bundles.Add(new StyleBundle("~/Bundles/css").Include(
                      "~/Content/webapp/css/bootstrap.css",
                      "~/Content/webapp/css/animate.css",
                      "~/Content/webapp/css/font.css",
                      "~/Content/webapp/css/font-awesome.min.css",
                      "~/Content/webapp/css/simple-line-icons.css",
                      "~/Content/webapp/less/app.css"
                      ));

            BundleTable.EnableOptimizations = true;
        }
    }
}
