using System.Web.Optimization;

namespace BrandValues
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/Bundles/webapp").Include(
                        "~/Scripts/jquery.unobtrusive-ajax.js",
                        "~/Scripts/jquery.validate.js",
                        "~/Scripts/jquery.validate.unobtrusive.js",
                        "~/Scripts/webapp/bootstrap.min.js",
                        "~/Scripts/webapp/app.min.js",
                        "~/Scripts/webapp/slimscroll/jquery.slimscroll.min.js",
                        "~/Scripts/webapp/app.plugin.js",
                        "~/Scripts/webapp/search/search.js"
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
                        "~/Scripts/webapp/parsley/parsley.extend.js",
                        "~/Scripts/webapp/wizard/jquery.bootstrap.wizard.js",
                        "~/Scripts/webapp/file-input/bootstrap-filestyle.min.js",
                        "~/Scripts/webapp/wizard/demo.js"
                        ));

            bundles.Add(new ScriptBundle("~/Bundles/picker").Include(
                        "~/Scripts/webapp/image-picker/image-picker.js",
                        "~/Scripts/webapp/image-picker/demo.js"
                        ));

            bundles.Add(new StyleBundle("~/Bundles/css").Include(
                      "~/Content/webapp/css/bootstrap.css",
                      "~/Content/webapp/css/animate.css",
                      "~/Content/webapp/css/font.css",
                      "~/Content/webapp/css/font-awesome.min.css",
                      "~/Content/webapp/css/simple-line-icons.css",
                      "~/Content/webapp/less/app.css"
                      ));

            //http://joshua.perina.com/africa/gambia/fajara/post/internet-explorer-css-file-size-limit
            bundles.Add(new StyleBundle("~/Bundles/jqueryui").Include(
                      "~/Content/webapp/css/jquery-ui-1.10.0.custom.css"
            ));
                                  

            bundles.Add(new StyleBundle("~/Bundles/ie6css").Include(
                    "~/Content/webapp/ie/screen.css",    
                    "~/Content/webapp/ie/ie.css",  
                    "~/Content/webapp/less/ie6.css",
                    "~/Content/webapp/css/simple-line-icons.css"
                      ));

            bundles.Add(new StyleBundle("~/Bundles/ie6").Include(
                    "~/Scripts/jquery.unobtrusive-ajax.js"
                      ));

            bundles.Add(new ScriptBundle("~/Bundles/ie6upload").Include(
                        "~/Scripts/webapp/wizard/demoIE6.js"
                        ));

            bundles.Add(new ScriptBundle("~/Bundles/ie6video").Include(
                        "~/Scripts/webapp/jwplayer/ie6.js"
                        ));

            BundleTable.EnableOptimizations = true;
        }
    }
}
