using BrandValues.Models;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace BrandValues
{
    // Note: For instructions on enabling IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=301868
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
			EnsureAuthIndexes.Exist();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            MvcHandler.DisableMvcResponseHeader = true;
        }

        #if !DEBUG
        protected void Application_BeginRequest()
        {
            //http://martin-brennan.github.io/aws/2013/04/08/force-https-asp-net-aws-load-balancer/
            string protocol = Request.Headers["X-Forwarded-Proto"];
            if (protocol != "https") {
                string redirectUrl = Request.Url.ToString().Replace("http:", "https:");
                Response.Redirect(redirectUrl);
            };
        }
        #endif


    }
}
