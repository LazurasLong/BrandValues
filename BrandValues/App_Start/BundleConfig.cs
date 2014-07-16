﻿using System.Web.Optimization;

namespace BrandValues
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/scripts/webapp").Include(
                        "~/Scripts/webapp/bootstrap.min.js",
                        "~/Scripts/webapp/app.min.js",
                        "~/Scripts/webapp/slimscroll/jquery.slimscroll.min.js",
                        "~/Scripts/webapp/app.plugin.js",
                        "~/Scripts/webapp/jwplayer/jwplayer.js",
                        "~/Scripts/webapp/jwplayer/jwplayer.html5.js",
                        "~/Scripts/webapp/parsley/parsley.min.js",
                        "~/Scripts/webapp/wizard/jquery.bootstrap.wizard.js"
                        ));


            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/webapp/css/bootstrap.css",
                      "~/Content/webapp/css/animate.css",
                      "~/Content/webapp/css/font-awesome.min.css",
                      "~/Content/webapp/css/simple-line-icons.css",
                      "~/Content/webapp/css/font.css",
                      "~/Content/webapp/less/app.css"
                      ));

            BundleTable.EnableOptimizations = false;
        }
    }
}
